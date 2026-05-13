#nullable enable
using System;
using System.Collections.Generic;
using DeepForestLabs.Logger;

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
        public IContainerBuilder AddSingleton<T>(T instance)
        {
            Validate(typeof(T));
            Log.Assert(instance != null, "AddSingleton<{0}> received a null instance.", typeof(T).Name);
            _container._singletons.TryAdd(typeof(T), instance!);

            return this;
        }

        public IContainerBuilder AddAlias<A, T>()
            where T : A
        {
            Validate(typeof(A));
            
            _aliases ??= new Dictionary<Type, Type>();
            _aliases.TryAdd(typeof(A), typeof(T));
            return this;
        }
        
        public IContainerBuilder AddAlias(Type alias, Type impl)
        {
            Validate(alias);
            
            _aliases ??= new Dictionary<Type, Type>();
            _aliases.TryAdd(alias, impl);
            return this;
        }
    }
}
#nullable disable