#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.MVC.Controllers
{
    public partial interface IController
    {
        ControlState ControlState { get; }
        CancellationToken Scope { get; }
        
        bool IsEnabled();
        bool IsProcessing();
        UniTask Run(CancellationToken token);
        void Enable();
        void Disable();
        
        object GetEntityId();
    }
    public interface IController<TModel> : IController
    {
        TModel Model { get; set; }
        IUniTaskAsyncEnumerable<TModel> ModelUpdated { get; }

        UniTask<VoidResult> Run(TModel model, CancellationToken token);
        new UniTask<VoidResult> Run(CancellationToken token);
        UniTask<VoidResult> Disable(CancellationToken token);
    }

    public interface IController<TModel, TResult> : IController<TModel>
    {
        void Enable(TModel model);
        new UniTask<ResultV<TResult>> Run(TModel model, CancellationToken token);
        new UniTask<ResultV<TResult>> Disable(CancellationToken token);
    }
}
#nullable disable