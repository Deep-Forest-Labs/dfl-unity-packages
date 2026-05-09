#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
using Resolver = System.Func<DeepForestLabs.IDiCollection, object>;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        public async UniTask<T> Create<T>(CancellationToken token)
            where T : class
        {
            T instance = await CreateRecursive<T>(this, token);

            HashSet<object> transients = new();
            transients.Add(instance);
            DiException? exception = await InjectRecursive(this, instance, 0, transients, token);
            if (exception != null)
            {
                throw exception;
            }

            List<UniTask> tasks = new();
            foreach (IInitializable initializable in transients.OfType<IInitializable>())
            {
                tasks.Add(initializable.Initialize(token));
            }
            await UniTask.WhenAll(tasks);
            
            foreach (IDisposable disposable in transients.OfType<IDisposable>())
            {
                token.Register(disposable.Dispose);
            }

            return instance;
        }

        public async UniTask<T> Create<T, TArgs>(TArgs args, CancellationToken token)
            where T : class
        {
            T instance = await CreateRecursive<T>(this, token);

            HashSet<object> transients = new();
            transients.Add(instance);

            Log.Assert(args != null, "{0} != null", nameof(args));
            
            IReadOnlyDictionary<Type, object> additionalDependencies = new Dictionary<Type, object>()
            {
                { typeof(TArgs), args }
            };

            DiException? exception = await InjectRecursive(this, instance, 0, transients, token, additionalDependencies);
            if (exception != null)
            {
                throw exception;
            }

            List<UniTask> tasks = new();
            foreach (IInitializable initializable in transients.OfType<IInitializable>())
            {
                tasks.Add(initializable.Initialize(token));
            }
            await UniTask.WhenAll(tasks);
            
            foreach (IDisposable disposable in transients.OfType<IDisposable>())
            {
                token.Register(disposable.Dispose);
            }

            return instance;
        }

        public async UniTask<T> Create<T, TArgs1, TArgs2>(TArgs1 args1, TArgs2 args2, CancellationToken token) 
            where T : class
        {
            T instance = await CreateRecursive<T>(this, token);
            
            HashSet<object> transients = new();
            transients.Add(instance);

            Log.Assert(args1 != null, "{0} != null", nameof(args1));
            Log.Assert(args2 != null, "{0} != null", nameof(args2));
            
            IReadOnlyDictionary<Type, object> additionalDependencies = new Dictionary<Type, object>()
            {
                { typeof(TArgs1), args1 },
                { typeof(TArgs2), args2 }
            };

            DiException? exception = await InjectRecursive(this, instance, 0, transients, token, additionalDependencies);
            if (exception != null)
            {
                throw exception;
            }

            List<UniTask> tasks = new();
            foreach (IInitializable initializable in transients.OfType<IInitializable>())
            {
                tasks.Add(initializable.Initialize(token));
            }
            await UniTask.WhenAll(tasks);
            
            foreach (IDisposable disposable in transients.OfType<IDisposable>())
            {
                token.Register(disposable.Dispose);
            }

            return instance;
        }

        public async UniTask<T> Create<T, TArgs1, TArgs2, TArgs3>(TArgs1 args1, TArgs2 args2, TArgs3 args3, 
            CancellationToken token)
            where T : class
        {
            T instance = await CreateRecursive<T>(this, token);
            
            HashSet<object> transients = new();
            transients.Add(instance);

            Log.Assert(args1 != null, "{0} != null", nameof(args1));
            Log.Assert(args2 != null, "{0} != null", nameof(args2));
            Log.Assert(args3 != null, "{0} != null", nameof(args3));
            
            IReadOnlyDictionary<Type, object> additionalDependencies = new Dictionary<Type, object>()
            {
                { typeof(TArgs1), args1 },
                { typeof(TArgs2), args2 },
                { typeof(TArgs3), args3 }
            };

            DiException? exception = await InjectRecursive(this, instance, 0, transients, token, additionalDependencies);
            if (exception != null)
            {
                throw exception;
            }

            List<UniTask> tasks = new();
            foreach (IInitializable initializable in transients.OfType<IInitializable>())
            {
                tasks.Add(initializable.Initialize(token));
            }
            await UniTask.WhenAll(tasks);
            
            foreach (IDisposable disposable in transients.OfType<IDisposable>())
            {
                token.Register(disposable.Dispose);
            }

            return instance;
        }

        private async UniTask<T> CreateRecursive<T>(Container origin, CancellationToken token)
        {
            Type type = typeof(T);
            if (_transientsResolvers != null && _transientsResolvers.TryGetValue(type, out Resolver resolver))
            {
                object instance = resolver(origin);
                if (instance is not T casted)
                {
                    throw new DiException("Type to resolver miss match");
                }

                return casted;
            }

            if (_asyncTransientResolvers != null && _asyncTransientResolvers.TryGetValue(type,
                    out Func<IDiCollection, CancellationToken, UniTask<object>>? asyncResolver))
            {
                object instance = await asyncResolver(origin, token);
                if (instance is not T casted)
                {
                    throw new DiException("Type to resolver miss match");
                }

                return casted;
            }

            // Stop recursion if we just processed the root container. 
            if (this != Root)
            {
                return await _parent.CreateRecursive<T>(origin, token);
            }

            throw new DiException($"Failed to find registered dependency of type {InternalUtils.FormatTypeName(type)}.");
        }
    }
}
#nullable disable