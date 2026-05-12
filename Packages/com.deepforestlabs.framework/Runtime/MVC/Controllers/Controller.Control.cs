#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using ZLinq;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;

namespace DeepForestLabs.MVC.Controllers
{
    public abstract partial class Controller<TModel, TView, TResult>
    {
        private readonly HashSet<UniTaskCompletionSource<ResultV<TResult>?>> _processes = new();

        private CancellationTokenSource? _runTokenSource;
        private UniTaskCompletionSource<ResultV<TResult>>? _runCompletionSource;
        public ControlState ControlState { get; private set; } = ControlState.Disabled;

        private UniTask InitializeControl(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            return UniTask.CompletedTask; 
        }

        private void DisposeControl()
        {
            ControlState = ControlState.Disabled;
            _runCompletionSource = null;
            _runTokenSource?.Cancel();
            _runTokenSource?.Dispose();
            _runTokenSource = null;
            foreach (UniTaskCompletionSource<ResultV<TResult>?> process in _processes)
            {
                process.TrySetCanceled(_scope);
            }
            _processes.Clear();
        }

        public bool IsEnabled()
        {
            return _runTokenSource != null && !_runTokenSource.IsCancellationRequested;
        }
        
        public bool IsProcessing() => _processes.Count > 0;

        public void Enable()
        {
            Run(_scope).Forget();
        }
        
        public void Enable(TModel model)
        {
            _model.Value = model;
            Run(_scope).Forget();
        }
        
        async UniTask<VoidResult> IController<TModel>.Run(CancellationToken token)
        {
            ResultV<TResult> result = await Run(token);
            if (!result.IsValid)
            {
                return VoidResult.FromSuccess(); 
            }

            return VoidResult.FromError(result.Error);
        }

        async UniTask<VoidResult> IController<TModel>.Run(TModel model, CancellationToken token)
        {
            ResultV<TResult> result = await Run(model, token);
            if (!result.IsValid)
            {
                return VoidResult.FromSuccess(); 
            }

            return VoidResult.FromError(result.Error);
        }

        async UniTask IController.Run(CancellationToken token)
        {
            await Run(token);
        }

        public async UniTask<ResultV<TResult>> Run(CancellationToken token)
        {
            if (_runCompletionSource == null)
            {
                Log.Assert(_runTokenSource == null, "_runTokenSource == null");
                
                _runCompletionSource = new UniTaskCompletionSource<ResultV<TResult>>();
                _runTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _scope);
                RunInternal(_runTokenSource.Token).Forget();
            }

            return await _runCompletionSource.Task.AttachExternalCancellation(token);
        }

        public void Disable()
        {
            _runTokenSource?.Cancel();
            _runTokenSource?.Dispose();
            _runTokenSource = null;

            if (_runCompletionSource != null)
            {
                SetReturnValue(_controlModel.DefaultResult);
            }
        }

        public async UniTask<ResultV<TResult>> Disable(CancellationToken token)
        {
            ResultV<TResult> result = _controlModel.DefaultResult;
            if (_runCompletionSource != null)
            {
                result = await _runCompletionSource.Task
                    .AttachExternalCancellation(token);
            }
            Disable();

            await UniTask.WaitWhile(IsVisible, cancellationToken: token);

            if (result.IsValid)
            {
                return ResultV<TResult>.FromResult(result.Value);
            }

            Log.Assert(result.IsValid, "result.IsValid");

            return ResultV<TResult>.FromError(result.Error);
        }
        
        async UniTask<VoidResult> IController<TModel>.Disable(CancellationToken token)
        {
            ResultV<TResult> result = await Disable(token);
            if (!result.IsValid)
            {
                return VoidResult.FromSuccess(); 
            }

            return VoidResult.FromError(result.Error);
        }
        
        private async UniTask<ResultV<TResult>> RunInternal(CancellationToken token)
        {
            RunModel(token).Forget();
            RunInput(token).Forget();
            RunControl(token).Forget();

            // ReSharper disable once MethodSupportsCancellation
            Show();

            (bool IsCanceled, ResultV<TResult> Value) result;
            if (_runCompletionSource == null)
            {
                result = (false, _controlModel.DefaultResult);
            }
            else
            {

                result = await _runCompletionSource.Task
                    .AttachExternalCancellation(cancellationToken: token)
                    .SuppressCancellationThrow();
            }

            // ReSharper disable once MethodSupportsCancellation
            Disable();

            Hide();
            token.ThrowIfCancellationRequested();

            if (result.IsCanceled)
            {
                ResultV<TResult> defaultResult = _controlModel.DefaultResult;
                if (defaultResult.IsValid)
                {
                    return ResultV<TResult>.FromResult(defaultResult.Value);
                }
                else
                {
                    return ResultV<TResult>.FromError(GameException.FromFormat("No default result of type {0}",
                        typeof(TResult).Name));
                }
            }

            return result.Value;
        }

        private async UniTaskVoid RunControl(CancellationToken token)
        {
            ResultV<TResult> result;

            try
            {
                ControlState = ControlState.Enabled;
                if (!Analytics.SuppressAnalyticsCalls)
                {
                    SendRunAnalytics();
                }
                await PreRun(token);

                while (true)
                {
                    if (_runCompletionSource != null)
                    {
                        (bool IsCanceled, ResultV<TResult> Result) controlResult = await _runCompletionSource.Task.AttachExternalCancellation(token)
                            .SuppressCancellationThrow();
                        token.ThrowIfCancellationRequested();
                        if (controlResult.IsCanceled)
                        {
                            continue;
                        }
                        
                        result = controlResult.Result;
                        break;
                    }
                    else
                    {
                        result = _controlModel.DefaultResult;
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Exception(e, ZString.Format("Unhandled exception while running {0}", GetType().Name));

                result = ResultV<TResult>.FromError(e.Message);
                SetReturnValue(result);
            }
            finally
            {
                if (!Analytics.SuppressAnalyticsCalls && !token.IsCancellationRequested)
                {
                    SendReturnAnalytics();
                }
            }
            
            await UniTask.WhenAll(
                UniTask.WaitWhile(IsVisible, cancellationToken: token),
                PostRun(result, token)
            );
        }

        private async UniTask RunInput(CancellationToken token)
        {
            while (true)
            {
                TView? view = null;
                CancellationToken viewScope = default;
                await UniTask.WaitUntil(() => TryGetView(out view, out viewScope), cancellationToken: token);


                Log.Assert(view != null, nameof(view) + " != null");
                await RunInput(view, viewScope)
                    .SuppressCancellationThrow();
                token.ThrowIfCancellationRequested();
            }
        }
        
        private async UniTask RunInput(TView view, CancellationToken token)
        {
            ResultV<TResult>? result = null;

            // Non-Skippable PreOpen
            ControlState = ControlState.PreOpen;
            await PreOpen(view, token);

            // Skippable Opening
            ControlState = ControlState.Opening;
            CancellationToken skipToken = CreateSkipToken(view, SkippableControlStates.Opening, token);
            await UniTask.WhenAny(
                view.OpenAnimation(skipToken),
                Open(view, skipToken)
            ).SuppressCancellationThrow();
            token.ThrowIfCancellationRequested();

            // Non-Skippable PostOpen
            ControlState = ControlState.PostOpen;
            await PostOpen(view, token);

            // Skippable Run
            ControlState = ControlState.Running;
            while (result == null)
            {
                // Finish processes before Running others
                while (_processes.Count > 0)
                {
                    result = (await UniTask.WhenAll(_processes.AsValueEnumerable().Select(p => p.Task.AttachExternalCancellation(token)).ToArray()))
                        .AsValueEnumerable().FirstOrDefault(r => r != null);
                }

                if (result == null)
                {
                    skipToken = CreateSkipToken(view, SkippableControlStates.Running, token);
                    skipToken = UniTask.WaitUntil(IsProcessing, cancellationToken: skipToken)
                        .ToCancellationToken();

                    (bool IsCanceled, TResult Value) runResult = await Run(view, skipToken)
                        .SuppressCancellationThrow();
                    token.ThrowIfCancellationRequested();

                    if (runResult.IsCanceled)
                    {
                        await UniTask.NextFrame(token);
                        continue;
                    }

                    result = runResult.IsCanceled
                        ? _controlModel.DefaultResult
                        : ResultV<TResult>.FromResult(runResult.Value);
                }

                if (_controlModel.Skippable.HasFlag(SkippableControlStates.Return))
                {
                    SetReturnValue(result.Value);
                }
            }

            // Non-Skippable PreClose
            ControlState = ControlState.PreClose;
            await PreClose(view, token);

            // Skippable Closing
            ControlState = ControlState.Closing;
            skipToken = CreateSkipToken(view, SkippableControlStates.Closing, token);
            await UniTask.WhenAny(
                view.CloseAnimation(skipToken),
                Close(view, skipToken)
            ).SuppressCancellationThrow();
            token.ThrowIfCancellationRequested();

            // Non-Skippable PostClose
            ControlState = ControlState.PostClose;
            await PostClose(view, token);
            SetReturnValue(result.Value);
        }

        private CancellationToken CreateSkipToken(TView view, SkippableControlStates skip, CancellationToken token)
        {
            UniTask skipTask = _controlModel.Skippable.HasFlag(skip)
                ? AwaitSkip(view, token).Preserve()
                : UniTask.Never(token);

            return skipTask
                .Preserve()
                .ToCancellationToken();
        }
        
        private void SetReturnValue(ResultV<TResult> result)
        {
            if (_runCompletionSource != null)
            {
                _runCompletionSource.TrySetResult(result);
                _runCompletionSource = null;
                _runTokenSource?.Cancel();
                _runTokenSource?.Dispose();
                _runTokenSource = null;
            }
        }
    }
}
#nullable disable
