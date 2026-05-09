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
        protected virtual UniTask AwaitSkip(TView screenView, CancellationToken token) => UniTask.Never(token);

        protected virtual void SendRunAnalytics() { }
        
        protected virtual UniTask PreRun(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual UniTask Initialize(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask PreOpen(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask Open(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask PostOpen(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected abstract UniTask<TResult> Run(TView view, CancellationToken token);

        protected virtual UniTask PreClose(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask Close(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask PostClose(TView view, CancellationToken token) => UniTask.CompletedTask;

        protected virtual UniTask PostRun(ResultV<TResult> result, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
        protected virtual void SendReturnAnalytics() { }
    }
}
#nullable disable
