#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;

namespace DeepForestLabs.MVC.Controllers
{
    public abstract partial class Controller<TModel, TView, TResult>
    {
        private TView? _view;
        private AsyncReactiveProperty<TView>? _viewLoaded;
        private AsyncReactiveProperty<TView>? _viewRelease;
        private AsyncReactiveProperty<TView>? _viewLoadedGeneric;
        private AsyncReactiveProperty<TView>? _viewReleaseGeneric;
        private CancellationTokenSource? _viewTokenSource;
        private CancellationTokenSource? _viewModelTokenSource;

        protected virtual Transform? ViewParent { get; } = null;
        public ViewState ViewState { get; private set; } = ViewState.Hiding;
        public ViewModel<TView> ViewModel => _viewModel;

        public IUniTaskAsyncEnumerable<TView> ViewLoaded => _viewLoaded ??= new AsyncReactiveProperty<TView>(_view!);

        public IUniTaskAsyncEnumerable<TView> ViewReleased => _viewRelease ??= new AsyncReactiveProperty<TView>(_view!);

        private int _visibleCount = 0;

        private void DisposeView()
        {
            ViewState = ViewState.Hidden;

            _viewLoaded?.Dispose();
            _viewRelease?.Dispose();
            _viewLoadedGeneric?.Dispose();
            _viewReleaseGeneric?.Dispose();

            _viewTokenSource?.Cancel();
            _viewTokenSource?.Dispose();
            _viewTokenSource = null;
            
            _viewModelTokenSource?.Cancel();
            _viewModelTokenSource?.Dispose();
            _viewModelTokenSource = null;
        }

        public bool IsBlocking()
        {
            return _visibleCount > 0 &&
                   _viewTokenSource != null &&
                   !_viewTokenSource.IsCancellationRequested &&
                   ViewState == ViewState.Visible;
        }

        public bool IsVisible()
        {
            return _visibleCount > 0 &&
               _viewTokenSource != null &&
               !_viewTokenSource.IsCancellationRequested &&
               ViewState.IsRendering();
        }

        public void Show()
        {
            _visibleCount++;
            if (_visibleCount == 1)
            {
                Log.Assert(_viewTokenSource == null, "[{0}] _viewTokenSource == null", Model.Id);
                Log.Assert(_viewModelTokenSource == null, "[{0}] _viewModelTokenSource == null", Model.Id);
                _viewTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_scope);
                RunView(_viewTokenSource.Token).Forget();
            }
        }

        public async UniTask Show(CancellationToken token)
        {
            Show();

            await UniTask.WaitUntil(IsVisible, cancellationToken: token);
        }

        public void Refresh()
        {
            _model.Value = _model.Value;
        }

        public void Hide()
        {
            _visibleCount--;
            if (_visibleCount <= 0)
            {
                _viewTokenSource?.Cancel();
                _viewTokenSource?.Dispose();
                _viewTokenSource = null;
                
                _viewModelTokenSource?.Cancel();
                _viewModelTokenSource?.Dispose();
                _viewModelTokenSource = null;
            }
        }

        public UniTask Hide(CancellationToken token)
        {
            Hide();
            
            return UniTask.WaitWhile(IsVisible, cancellationToken:token); 
        }

        public async UniTask<(TView view, CancellationToken scope)> GetView(CancellationToken token)
        {
            while (true)
            {
                if (_view != null && _viewTokenSource != null &&
                    !_viewTokenSource.Token.IsCancellationRequested)
                {
                    return (_view, _viewTokenSource.Token);
                }

                await UniTask.NextFrame(token);
            }
        }

        public bool TryGetView([NotNullWhen(true)] out TView view, out CancellationToken token)
        {
            if (_view != null && _viewTokenSource != null &&
                !_viewTokenSource.Token.IsCancellationRequested)
            {
                view = _view;
                token = _viewTokenSource.Token;
                return true;
            }

            view = default!;
            token = default;
            return false;
        }

        protected virtual UniTask QueueLoad(CancellationToken token) => UniTask.CompletedTask;
        protected virtual bool IsQueued() => false;

        // ReSharper disable once UnusedParameter.Local
        private UniTask InitializeView(CancellationToken token)
        {
	        return UniTask.CompletedTask;
        }
        
        protected virtual void DisposeView(TView? view)
        {
        }

        protected virtual UniTask Update(TView view, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void Update(CancellationToken token)
        {
        }

        protected virtual UniTask Show(TView view, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask Hide(TView view, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid RunView(CancellationToken token)
        {
            try
            {
                await UniTask.WhenAll(
                    QueueLoad(token),
                    UniTask.WaitWhile(IsQueued, cancellationToken: token)
                );

                TView view = await _viewPool.Checkout(ViewParent, ViewModel.WorldPositionStays, token);
                token.Register(() => DisposeView(view));
                
                await Initialize(view, token);
                
                _viewModelTokenSource?.Cancel();
                _viewModelTokenSource?.Dispose();
                _viewModelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _model.WaitAsync(token).ToCancellationToken());
                await Update(view, _viewModelTokenSource.Token)
                    .SuppressCancellationThrow();
                token.ThrowIfCancellationRequested();

                SetViewLoaded(view);

                // Show
                ViewState = ViewState.Showing;
                await Show(view, token);

                // Wait while control is processing
                ViewState = ViewState.Visible;
                if (_runCompletionSource != null)
                {
                    await _runCompletionSource.Task.AttachExternalCancellation(token);
                }

                // Close
                ViewState = ViewState.Hiding;
                await Hide(view, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, "Unhandled exception while running {0}", GetType().Name);
                SetReturnValue(ResultV<TResult>.FromError(e.Message));
            }
            finally
            {
                ViewState = ViewState.Hidden;

                _viewTokenSource?.Cancel();
                _viewTokenSource?.Dispose();
                _viewTokenSource = null;
                
                _viewModelTokenSource?.Cancel();
                _viewModelTokenSource?.Dispose();
                _viewModelTokenSource = null;

                ReleaseView(_view);
            }
        }

        private void SetViewLoaded(TView view)
        {
            _view = view;
            {
                if (_viewLoaded != null)
                {
                    _viewLoaded.Value = view;
                }

                if (_viewLoadedGeneric != null)
                {
                    _viewLoadedGeneric.Value = view;
                }
            }
        }

        private void ReleaseView(TView? view)
        {
            _view = default;
            if (view == null)
            {
                return;
            }
            if (_viewRelease != null)
            {
                _viewRelease.Value = view;
            }

            if (_viewReleaseGeneric != null)
            {
                _viewReleaseGeneric.Value = view;
            }
        }
    }
}
#nullable disable
