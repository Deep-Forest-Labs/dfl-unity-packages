using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;

namespace DeepForestLabs.MVC.Controllers
{
    public abstract partial class Controller<TModel, TView, TResult>
    {
        protected async UniTask RunSubTask(Func<CancellationToken, UniTask<ResultV<TResult>?>> process,
            CancellationToken token)
        {
            UniTaskCompletionSource<ResultV<TResult>?> taskCompletionSource = new();
            _processes.Add(taskCompletionSource);
            process(_scope)
                .ContinueWith(r =>
                {
                    _processes.Remove(taskCompletionSource);
                    return taskCompletionSource.TrySetResult(r);
                })
                .Forget();

            await taskCompletionSource.Task.AttachExternalCancellation(token);
        }

        protected async UniTask RunSubTask(Func<CancellationToken, UniTask> process, CancellationToken token)
        {
            UniTaskCompletionSource<ResultV<TResult>?> taskCompletionSource = new();
            
            _processes.Add(taskCompletionSource);
            process(_scope)
                .ContinueWith(() =>
                {
                    _processes.Remove(taskCompletionSource);
                    return taskCompletionSource.TrySetResult(null);
                })
                .Forget();

            await taskCompletionSource.Task.AttachExternalCancellation(token);
        }

        private void QueueAndRunSubTask(Func<CancellationToken, UniTask> process,
            UniTaskCompletionSource<ResultV<TResult>?> taskCompletionSource)
        {
            _processes.Add(taskCompletionSource);
            taskCompletionSource.Task.ContinueWith(_ => _processes.Remove(taskCompletionSource));
            process(_scope).Forget();
        }

        private async UniTaskVoid QueueAndRunSubTask(Func<CancellationToken, UniTask<ResultV<TResult>?>> process,
            UniTaskCompletionSource<ResultV<TResult>?> taskCompletionSource, CancellationToken token)
        {

            await taskCompletionSource.Task.AttachExternalCancellation(token);
        }
    }
}