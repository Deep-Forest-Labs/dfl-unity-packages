using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddScoped<T>()
            where T : new()
        {
            Type t = typeof(T);
            Validate(t);
            _scopedResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _scopedResolvers.TryAdd(t, _ =>
            {
                #if UNITY_EDITOR
                if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(T)))
                {
                    throw new DiException("Cannot 'new' a mono behaviour");
                }
                #endif
                return new T();
            });
            return this;
        }

        public IContainerBuilder AddScoped<T, U>()
            where U : T, new()
        {
            Type t = typeof(T);
            Validate(t);
            _scopedResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _scopedResolvers.TryAdd(t, _ => new U());
            return this;
        }

        public IContainerBuilder AddScoped<T>(Func<IDiCollection, T> factory)
        {
            Type t = typeof(T);
            Validate(t);
            _scopedResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _scopedResolvers.TryAdd(t, c => factory(c) ?? throw DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(t)));
            return this;
        }

        public IContainerBuilder AddScoped(Type t, Func<IDiCollection, object> factory)
        {
            Validate(t);
            _scopedResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _scopedResolvers.TryAdd(t, c => factory(c) ?? DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(t)));
            return this;
        }

        public IContainerBuilder AddScoped<T>(Func<IDiCollection, CancellationToken, UniTask<T>> asyncFactory)
        {
            Type type = typeof(T);
            Validate(type);
            _asyncScopedResolvers ??= new Dictionary<Type, Func<IDiCollection, CancellationToken, UniTask<object>>>();
            _asyncScopedResolvers.TryAdd(type,
                async (c, t) => await asyncFactory(c, t) ??
                                throw DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(type)));
            return this;
        }

        public IContainerBuilder AddSingleton(object singleton)
        {
            Type t = singleton.GetType();
            Validate(t);
            
            _container._singletons.TryAdd(t, singleton);
            return this;
        }
        
        public IContainerBuilder AddSingleton<T>(Func<IDiCollection, T> factory)
        {
            Type t = typeof(T);
            Validate(t);
            _singletonResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _singletonResolvers.TryAdd(t, c => factory(c) ?? throw DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(t)));
            return this;
        }
        
        public IContainerBuilder AddSingleton<T>(Func<IDiCollection, CancellationToken, UniTask<T>> asyncFactory)
        {
            Type type = typeof(T);
            Validate(type);
            _asyncSingletonResolvers ??= new Dictionary<Type, Func<IDiCollection, CancellationToken, UniTask<object>>>();
            _asyncSingletonResolvers.TryAdd(type,
                async (c, t) => await asyncFactory(c, t) ??
                                throw DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(type)));
            return this;
        }

        public IContainerBuilder PromoteFrom<T>(IContainerBuilder child)
        {
            Validate(typeof(T));
            _container._singletons.TryAdd(typeof(T), child.Get<T>());

            return this;
        }
        
        public IContainerBuilder Promote<T>(T singleton)
        {
            Type t = singleton.GetType();
            Validate(t);
            
            _container._singletons.TryAdd(t, singleton);
            return this;
        }
    }
}