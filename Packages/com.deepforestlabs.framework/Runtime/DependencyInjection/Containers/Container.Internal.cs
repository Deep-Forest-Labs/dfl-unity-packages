#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DeepForestLabs.BuildSystems;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
using DeepForestLabs.Reflections;
using DeepForestLabs.Utils;
using UnityEngine;

using AsyncResolver = System.Func<DeepForestLabs.IDiCollection, System.Threading.CancellationToken, Cysharp.Threading.Tasks.UniTask<object>>;
using Resolver = System.Func<DeepForestLabs.IDiCollection, object>;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        internal static IContainerBuilder CreateMain(string name, CancellationToken token)
        {
            GameObject gameObject = new(name);
            gameObject.hideFlags = HideFlags.DontSave;

            if (Application.isPlaying)
            {
                GameObject.DontDestroyOnLoad(gameObject);
            }

            void DisposeGameObject()
            {
                Root = null;
#if UNITY_EDITOR
                GameObject.DestroyImmediate(gameObject);
#else
                GameObject.Destroy(gameObject);
#endif
            }

            ContainerBuilder builder = new ContainerBuilder(name, gameObject, token);
            Root = builder._container;
            builder.Scope.Register(DisposeGameObject);
            
            return builder;
        }

        internal async UniTask<DiException?> InjectRecursive(Container origin, object instance, int depth,
            ICollection<object>? transients, CancellationToken token,
            IReadOnlyDictionary<Type, object>? additionalDependencies = null)
        {
            if (depth > 25)
            {
                return new DiException(ZString.Format(
                    "[{0}] Recursion loop depth is '{1}'.  Do we have a transient co dependency issue?", origin.Name,
                    depth));
            }

            Type t = instance.GetType();
            if (NOT_INJECTED.Contains(t))
            {
                return null;
            }

            InternalUtils.VerboseLog(_name,
                ZString.Format("Injecting {0}.", InternalUtils.FormatTypeName(instance.GetType())),
                ContainerLogFlag.Injection);

            DiException? ex = null;
            CachedTypeInfo cachedTypeInfo = TypeInfoCache.GetTypeInfo(instance);
            List<UniTask> tasks = new();
            foreach (FieldInfo fieldInfo in cachedTypeInfo._fields)
            {
                if (additionalDependencies != null
                    && additionalDependencies.TryGetValue(fieldInfo.FieldType, out object dependency))
                {
                    fieldInfo.SetValue(instance, dependency);
                    continue;
                }

                FieldInjectResult result = await InjectFieldRecursive(origin, instance, fieldInfo, token);
                if (result.Error != null)
                {
                    ex ??= new DiException(result.Error);
                }
                else if (result.Transient != null)
                {
                    transients ??= new List<object>();
                    transients.Add(result.Transient);
                    tasks.Add(InjectRecursive(origin, result.Transient, depth + 1, transients, token));
                }
            }

            if (ex == null)
            {
                await UniTask.WhenAll(tasks);
            }

            return ex;
        }
        
        internal async UniTask<FieldInjectResult> InjectFieldRecursive(Container origin, object instance, 
            FieldInfo fieldInfo, CancellationToken token)
        {
            Type t = fieldInfo.FieldType;
            Type type = t;

            if (_scoped != null && _scoped.TryGetValue(type, out object dependency))
            {
                fieldInfo.SetValue(instance, dependency);
                return FieldInjectResult.FromSuccess();
            }
            
            if (_singletons != null && _singletons.TryGetValue(type, out dependency))
            {
                fieldInfo.SetValue(instance, dependency);
                return FieldInjectResult.FromSuccess();
            }

            if (_transientsResolvers != null && _transientsResolvers.TryGetValue(type, out Resolver resolver))
            {
                InternalUtils.VerboseLog(_name, ZString.Format("Creating {0} for field {1} belong to {2}", 
                    InternalUtils.FormatTypeName(type), fieldInfo.Name,
                    InternalUtils.FormatTypeName(fieldInfo.DeclaringType!)), 
                    ContainerLogFlag.Instantiation);
                
                dependency = resolver(origin);
                fieldInfo.SetValue(instance, dependency);
                return FieldInjectResult.FromSuccess(dependency);
            }
            
            if (_asyncTransientResolvers != null && _asyncTransientResolvers.TryGetValue(type, out AsyncResolver? asyncResolver))
            {
                InternalUtils.VerboseLog(_name, ZString.Format("Creating {0} for field {1} belong to {2}", 
                    InternalUtils.FormatTypeName(type), fieldInfo.Name,
                    InternalUtils.FormatTypeName(fieldInfo.DeclaringType!)), 
                    ContainerLogFlag.Instantiation);
                
                dependency = await asyncResolver(origin, token);
                fieldInfo.SetValue(instance, dependency);
                return FieldInjectResult.FromSuccess(dependency);
            }

            if (this != Root)
            {
                return await _parent.InjectFieldRecursive(origin, instance, fieldInfo, token);
            }

            if (fieldInfo.IsNullableReference())
            {
                //VerboseWarn($"Optional type {FormatTypeName(t)} for field {fieldInfo.Name} in type {FormatTypeName(instance.GetType())} not found. ");
                return FieldInjectResult.FromSuccess();
            }

            string error = ZString.Format("Failed to located dependency of type {0} for field {1}.{2}.",
                InternalUtils.FormatTypeName(t), 
                InternalUtils.FormatTypeName(instance.GetType()), fieldInfo.Name);
            InternalUtils.VerboseWarn(_name, error, ContainerLogFlag.Injection);
            
            return FieldInjectResult.FromError(error);
        }

        internal static readonly HashSet<Type> NOT_INJECTED = new()
        {
            typeof(GameObject),
            typeof(Texture),
            typeof(Sprite),
            typeof(Mesh),
        };
    }
}
#nullable disable