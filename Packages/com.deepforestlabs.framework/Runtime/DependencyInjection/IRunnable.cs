#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public interface IRunnable
    {
        UniTask Run(CancellationToken token);
    }
    
    public interface IRunnableResult<TResult>
    {
        UniTask<TResult> Run(CancellationToken token);
    }
    
    public interface IRunnableWith<in TArgs>
    {
        UniTask Run(TArgs args, CancellationToken token);
    }
    
    public interface IRunnableWith<in TArgs1, in TArgs2>
    {
        UniTask Run(TArgs1 args1, TArgs2 args2, CancellationToken token);
    }
    
    public interface IRunnableWith<in TArgs1, in TArgs2, in TArgs3>
    {
        UniTask Run(TArgs1 arg1, TArgs2 args2, TArgs3 args3, CancellationToken token);
    }
    
    public interface IRunnableWithResult<in TArgs, TResult>
    {
        UniTask<TResult> Run(TArgs args, CancellationToken token);
    }
    
    public interface IRunnableWithResult<in TArgs1, in TArgs2, TResult>
    {
        UniTask<TResult> Run(TArgs1 args1, TArgs2 args2, CancellationToken token);
    }
    
    public interface IRunnableWithResult<in TArgs1, in TArgs2, in TArgs3, TResult>
    {
        UniTask<TResult> Run(TArgs1 arg1, TArgs2 args2, TArgs3 args3, CancellationToken token);
    }
    
    public interface IRunnableWithResult<in TArgs1, in TArgs2, in TArgs3, in TArgs4, TResult>
    {
        UniTask<TResult> Run(TArgs1 arg1, TArgs2 args2, TArgs3 args3, TArgs4 arg4, CancellationToken token);
    }
}
#nullable disable