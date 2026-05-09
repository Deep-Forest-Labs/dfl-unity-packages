#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public partial interface IContainer
    {
        UniTask Run(IRunnable runnable, CancellationToken token);

        UniTask Run<TRunnable>(CancellationToken token)
            where TRunnable : class, IRunnable;

        UniTask Run<TRunnable, TArgs>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnable;

        UniTask Run<TRunnable, TArgs1, TArgs2>(TArgs1 args1, TArgs2 args2, CancellationToken token)
            where TRunnable : class, IRunnable;

        UniTask Run<TRunnable, TArgs1, TArgs2, TArgs3>(TArgs1 args1, TArgs2 args2, TArgs3 args3,
            CancellationToken token)
            where TRunnable : class, IRunnable;

        UniTask<TResult> Run<TResult>(IRunnableResult<TResult> runnable, CancellationToken token);

        UniTask<TResult> Run<TRunnable, TResult>(CancellationToken token)
            where TRunnable : class, IRunnableResult<TResult>;

        UniTask<TResult> Run<TRunnable, TArgs, TResult>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnableResult<TResult>;

        UniTask<TResult> Run<TRunnable, TArgs1, TArgs2, TResult>(TArgs1 args1, TArgs2 args2, CancellationToken token)
            where TRunnable : class, IRunnableResult<TResult>;

        UniTask<TResult> Run<TRunnable, TArgs1, TArgs2, TArgs3, TResult>(TArgs1 args1, TArgs2 args2, TArgs3 args3,
            CancellationToken token)
            where TRunnable : class, IRunnableResult<TResult>;

        UniTask RunWith<TArgs>(IRunnableWith<TArgs> runnable, TArgs args, CancellationToken token);

        UniTask RunWith<TRunnable, TArgs>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs>;

        UniTask RunWith<TArgs1, TArgs2>(IRunnableWith<TArgs1, TArgs2> runnable, TArgs1 args, TArgs2 args2,
            CancellationToken token);

        UniTask RunWith<TRunnable, TArgs1, TArgs2>(TArgs1 args, TArgs2 args2, CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs1, TArgs2>;

        UniTask RunWith<TArgs1, TArgs2, TArgs3>(IRunnableWith<TArgs1, TArgs2, TArgs3> runnable, TArgs1 args,
            TArgs2 args2,
            TArgs3 args3, CancellationToken token);

        UniTask RunWith<TRunnable, TArgs1, TArgs2, TArgs3>(TArgs1 args, TArgs2 args2, TArgs3 args3,
            CancellationToken token)
            where TRunnable : class, IRunnableWith<TArgs1, TArgs2, TArgs3>;

        UniTask<TResult> RunWith<TRunnable, TArgs, TResult>(TArgs args, CancellationToken token)
            where TRunnable : class, IRunnableWithResult<TArgs, TResult>;

        UniTask<TResult> RunWith<TRunnable, TArgs1, TArgs2, TResult>(TArgs1 args1, TArgs2 args2,
            CancellationToken token)
            where TRunnable : class, IRunnableWithResult<TArgs1, TArgs2, TResult>;

        UniTask<TResult> RunWith<TRunnable, TArgs1, TArgs2, TArgs3, TResult>(TArgs1 args1, TArgs2 args2, TArgs3 args3,
            CancellationToken token)
            where TRunnable : class, IRunnableWithResult<TArgs1, TArgs2, TArgs3, TResult>;
    }
}
#nullable disable