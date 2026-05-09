#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;

namespace DeepForestLabs.MVC.Controllers
{
    public abstract partial class Controller<TModel, TView, TResult>
    {
	    public TModel Model
        {
            get => _model;
            set => _model.Value = value;
        }

        public IUniTaskAsyncEnumerable<TModel> ModelUpdated => _model;

        private UniTask InitializeModel(CancellationToken token)
        {
	        return UniTask.CompletedTask;
        }

        private void DisposeModel()
        {
            //_model.Dispose(); //Dependency, will get disposed with container.
        }

        private async UniTaskVoid RunModel(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using CancellationTokenSource modelScope = CancellationTokenSource.CreateLinkedTokenSource(token);
                
                Update(modelScope.Token);
                if (_viewModelTokenSource != null && TryGetView(out TView view, out CancellationToken viewScope))
                {
                    _viewModelTokenSource?.Cancel();
                    _viewModelTokenSource?.Dispose();
                    _viewModelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(modelScope.Token, viewScope);
                    Update(view, _viewModelTokenSource.Token).Forget();
                }
                
                await _model.WaitAsync(token).SuppressCancellationThrow();
                
                modelScope.Cancel();
                modelScope.Dispose();
            }
        }
    }
}
#nullable disable
