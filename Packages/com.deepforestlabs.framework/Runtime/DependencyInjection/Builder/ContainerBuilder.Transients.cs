#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddTransient<T>()
            where T : new()
        {
            Type t = typeof(T);
            Validate(t);
            _container._transientsResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _container._transientsResolvers.TryAdd(t, _ => new T());
            return this;
        }

        public IContainerBuilder AddTransient<T, U>()
            where U : T, new()
        {
            Type t = typeof(T);
            Validate(t);
            _container._transientsResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _container._transientsResolvers.TryAdd(t, _ => new U());
            return this;
        }

        public IContainerBuilder AddTransient<T>(Func<IDiCollection, T> factory)
        {
            Type t = typeof(T);
            Validate(t);
            _container._transientsResolvers ??= new Dictionary<Type, Func<IDiCollection, object>>();
            _container._transientsResolvers.TryAdd(t,
                c => factory(c) ?? throw DiException.FromFormat("{0} is null.", InternalUtils.FormatTypeName(t)));
            return this;
        }

        public IContainerBuilder AddTransient<T>(Func<IDiCollection, CancellationToken, UniTask<T>> factory)
        {
            Type type = typeof(T);
            Validate(type);
            _container._asyncTransientResolvers ??=
                new Dictionary<Type, Func<IDiCollection, CancellationToken, UniTask<object>>>();
            _container._asyncTransientResolvers.TryAdd(type, async (c, t) =>
            {
                T transient = await factory(c, t);
                Log.Assert(transient != null, nameof(transient) + " != null");

                return transient;
            });
            return this;
        }
    }
}
#nullable disable