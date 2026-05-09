#nullable enable
using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Constants;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace DeepForestLabs.MVC.Controllers
{
    public abstract class Controller<TModel, TView> : Controller<TModel, TView, VoidReturn>, IController<TModel>
	    where TView : MonoBehaviour, IView
	    where TModel : IModel
    {
	    async UniTask<VoidResult> IController<TModel>.Run(TModel model, CancellationToken token)
	    {
		    return await base.Run(model, token);
	    }
	    
	    async UniTask<VoidResult> IController<TModel>.Run(CancellationToken token)
	    {
		    return await base.Run(token);
	    }

	    async UniTask<VoidResult> IController<TModel>.Disable(CancellationToken token)
	    {
		    return await base.Disable(token);
	    }
    }

    public abstract partial class Controller<TModel, TView, TResult> : IController<TModel, TView, TResult>, IInitializable, IDisposable
	    where TView : MonoBehaviour, IView
		where TModel : IModel
    {
        [Dependency] protected readonly IContainer _container = default!;
        [Dependency] protected readonly CancellationToken _scope = default;
        [Dependency] protected readonly AsyncReactiveProperty<TModel> _model = default!;
        [Dependency] protected readonly IGameObjectManagerT<TView> _viewPool = default!;
        [Dependency] protected readonly ViewModel<TView> _viewModel = default!;
        [Dependency] protected readonly ControlModel<TResult> _controlModel = default!;
        [Dependency] protected readonly AsyncReactiveProperty<AnalyticsStringValues> _analytics = default!;
        [Dependency] protected readonly IAnalyticsUIEventHelper _analyticsUIEventHelper = default!;
        
        public CancellationToken Scope => _scope;

        public virtual async UniTask Initialize(CancellationToken token)
        {
	        await UniTask.WhenAll(
				InitializeAnalytics(token),
				InitializeModel(token),
				InitializeView(token),
				InitializeControl(token)
	        );
        }

        public virtual void Dispose()
        {
            DisposeModel();
            DisposeView();
            DisposeControl();
        }
        
        public async UniTask<ResultV<TResult>> Run(TModel model, CancellationToken token)
        {
	        Model = model;
	        return await Run(token);
        }
        
        public object GetEntityId()
        {
	        return _model.Value.Id;
        }

        public override string ToString()
        {
	        return ZString.Format("{0}[{1}.{2} '{3}']{5} ", 
		        MVCConstants.COLOR_TAG_OPEN, 
		        Model.Id.GetType().Name, 
		        Model.Id.ToString(), 
		        Model.Name, 
		        MVCConstants.COLOR_TAG_CLOSE);
        }

        public virtual UniTask NextInput(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
#nullable disable
