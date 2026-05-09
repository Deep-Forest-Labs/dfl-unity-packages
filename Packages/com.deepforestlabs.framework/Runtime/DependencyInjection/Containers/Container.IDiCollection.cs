#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cysharp.Text;
using DeepForestLabs.Common;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        public object Get(Type type)
        {
            return GetRecursive(this, type);
        }

        public T Get<T>()
        {
            Type type = typeof(T);
            return (T)Get(type);
        }

        public T? Find<T>()
        {
            return FindRecursive<T>(this, typeof(T));
        }

        public bool TryGet<T>([NotNullWhen(true)] out T? instance)
        {
            T? found = Find<T>();
            if (found == null)
            {
                instance = default;
                return false;
            }

            instance = found;
            return true;
        }
        
        private object GetRecursive(Container origin, Type type)
        {
            if (_singletons != null && _singletons.TryGetValue(type, out object instance))
            {
                return instance;
            }

            if (_scoped != null && _scoped.TryGetValue(type, out instance))
            {
                return instance;
            }

            // Stop recursion if we just processed the root container.
            if (this != Root)
            {
                return _parent.Get(type);
            }

            throw new DiException(ZString.Format("[{0}] Failed to find registered dependency of type {1}.",
                origin._name, InternalUtils.FormatTypeName(type)));
        }
        
        public T? FindRecursive<T>(Container origin, Type type)
        {
            if (_singletons != null && _singletons.TryGetValue(type, out object singleton) && singleton is T castedSingleton)
            {
                return castedSingleton;
            }
            if (_scoped != null && _scoped.TryGetValue(type, out object scoped) && scoped is T castedScoped)
            {
                return castedScoped;
            }

            if (this != Root) // Stop recursion if we just processed the root container.
            {
                return _parent.FindRecursive<T>(origin, type);
            }

            return default;
        }
    }
}
#nullable disable