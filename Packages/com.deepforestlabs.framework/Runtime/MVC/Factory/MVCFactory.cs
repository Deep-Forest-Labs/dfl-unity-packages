#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DeepForestLabs.MVC.Controllers;
using DeepForestLabs.MVC.Models;
using DeepForestLabs.MVC.Views;
using UnityEngine;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace DeepForestLabs.MVC.Factory
{
    public abstract class Factory<TId> : Factories.ContainerFactory
        where TId : Enum
    {
    }

    public abstract class Factory<TId, TModel, TView, TController> : Factory<TId, TModel, TView, VoidReturn, TController>
        where TId : Enum
        where TModel : Model<TId>
        where TView : MonoBehaviour, IView
        where TController : Controller<TModel, TView>, new()
    {
	    protected override IContainerBuilder AddComponents(IContainerBuilder builder)
	    {
		    return base.AddComponents(builder)
				.AddAlias<IController<TModel>, TController>();
	    }
	    
        protected override void ArrangeComponents(IParentContainerBuilder parent, IContainerBuilder builder,
	        IReadOnlyList<IContainer> children)
        {
            base.ArrangeComponents(parent, builder, children);

            if (Control.AutoPromoteController)
            {
	            parent.PromoteFrom<IController<TModel>>(builder);
            }
        }
    }
    
    	public abstract class Factory<TId, TModel, TView, TResult, TController> : Factory<TId>
    		where TId : Enum
    		where TModel : Model<TId>
    		where TView : MonoBehaviour, IView 
    		where TController : Controller<TModel, TView, TResult>, new()
    	{
    		[SerializeField] private TModel _model = default!;
    		[SerializeField] private ViewModel<TView> _view = default!;
    		[SerializeField] private ControlModel<TResult> _control = default!;
    		[SerializeField] private AnalyticsStringValues _analytics = default!;
    
    		protected TModel Model => _model;
    
    		protected ViewModel<TView> View => _view;
    
    		protected ControlModel<TResult> Control => _control;
    
    		protected AnalyticsStringValues Analytics => _analytics;

            public override IContainerBuilder Resolve(IContainerBuilder parent, IContainerBuilder builder)
            {
                builder.AddScoped(_ => new AsyncReactiveProperty<TModel>(_model))
                    .AddSingleton(_view)
                    .AddSingleton(_control)
                    .AddScoped(_ => new AsyncReactiveProperty<AnalyticsStringValues>(_analytics))
                    .AddGameObjectManager(_view.Prefab, _view.Options)
                    .AddScoped<TController>()
                    .AddAlias<IController<TModel, TResult>, TController>();

                AddComponents(builder);

	            parent.OnPreInitialize += OnPreInitialize;
	            parent.OnBuildComplete += OnBuildComplete;
	      
				return builder;
	        }
            
            public override void Arrange(IParentContainerBuilder parent, IContainerBuilder builder,
	            IReadOnlyList<IContainer> children)
            {
	            base.Arrange(parent, builder, children);

	            if (Control.AutoPromoteController)
	            {
		            parent.PromoteFrom<IController<TModel, TResult>>(builder);
	            }

	            ArrangeComponents(parent, builder, children);
            }

            protected virtual IContainerBuilder AddComponents(IContainerBuilder builder)
    		{
    			return builder;
    		}
    		
    		protected virtual void ArrangeComponents(IParentContainerBuilder parent, IContainerBuilder builder,
	            IReadOnlyList<IContainer> children)
    		{
            }

            protected virtual void OnPreInitialize(IContainer container)
            {
            }

            protected virtual void OnBuildComplete(IContainer container)
    		{
    		}
    	}
}
#nullable disable