using System;
using System.Collections.Generic;
using ZLinq;
using DeepForestLabs.Logger;

#if !RELEASE_BUILD
using System.Threading;
using DeepForestLabs.BuildSystems;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
#endif

namespace DeepForestLabs
{
    internal sealed partial class ContainerBuilder
    {
#if !RELEASE_BUILD
        private readonly HashSet<Type> _registered = new();
        private readonly List<DiException> _registeredExceptions = new();
#endif

        [System.Diagnostics.Conditional("DEBUG_CONTAINER")]
        private void ValidatePreBuild()
        {
#if !RELEASE_BUILD
            if (_registeredExceptions.Count == 0)
            {
                return;
            }

            Log.Exception(_registeredExceptions.AsValueEnumerable().First(), "PreBuild Exception for container '{0}'", Name);
            throw _registeredExceptions.AsValueEnumerable().First();
#endif
        }
        
        [System.Diagnostics.Conditional("DEBUG_CONTAINER")]
        private void ValidatePostBuild()
        {
            Dictionary<Type, Exception> exceptions = new();
            if (_container._transientsResolvers != null)
            {
                foreach (var pair in _container._transientsResolvers)
                {
                    Func<IDiCollection, object> resolver = pair.Value;
                    try
                    {
                        resolver(_container);
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[{0}] - {1}", pair.Key.Name, e.Message);
                        exceptions.Add(pair.Key, e);
                    }
                }
            }

            if (exceptions.AsValueEnumerable().Any())
            {
                throw exceptions.AsValueEnumerable().First().Value;
            }
        }
        
        [System.Diagnostics.Conditional("DEBUG_CONTAINER")]
        private void Validate(Type type)
        {
#if !RELEASE_BUILD
            if (!_registered.Add(type))
            {
                string message = ZString.Format("Duplicate registration for type {0}.",
                    InternalUtils.FormatTypeName(type));
                Log.Warning(message);
                _registeredExceptions.Add(new DiException(message));
            }
#endif
        }
#if !RELEASE_BUILD
        private static async UniTask InitializeAndLog(string name, IInitializable initializable, CancellationToken token)
        {
            InternalUtils.VerboseLog(name, ZString.Format("Initializing {0}.", InternalUtils.FormatTypeName(initializable.GetType())), ContainerLogFlag.Initialization);
            await initializable.Initialize(token);
            InternalUtils.VerboseLog(name, ZString.Format("Initializing {0} complete.", InternalUtils.FormatTypeName(initializable.GetType())), ContainerLogFlag.Initialization);

        }
#endif
    }
}
