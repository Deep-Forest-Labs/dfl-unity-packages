#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Common;
using DeepForestLabs.Reflections;
using DeepForestLabs.Utils;
using Resolver = System.Func<DeepForestLabs.IDiCollection, object>;

namespace DeepForestLabs
{
    internal sealed partial class Container
    {
        // Could make this async if we wanna keep, but that's pretty much adopt
        [Obsolete("//TODO - [2.5.+] Create JIRA -determine if we need/want this as it bypasses transients and scopes", false)]
        public void Inject(object instance)
        {
            Log.Assert(!IsDisposed, "[{0}] Inject - Container is already disposed", _name);
            Exception? ex = InjectRecursive(this, instance, 0);
            if (ex != null)
            {
                throw ex;
            }
        }

        [Obsolete("//TODO - [2.5.+] Create JIRA -determine if we need/want this as it bypasses transients and scopes", false)]
        private DiException? InjectRecursive(Container container, object instance, int depth)
        {
            if (depth > 25)
            {
                throw new DiException(ZString.Format("[{0}] Recursion loop depth is '{1}'.  Do we have a transient co dependency issue?", container.Name,
                    depth));
            }
            Type t = instance.GetType();
            if (NOT_INJECTED.Contains(t))
            {
                return null;
            }
            
            InternalUtils.VerboseLog(_name, ZString.Format("Injecting {0}.", InternalUtils.FormatTypeName(instance.GetType())), ContainerLogFlag.Injection);
            
            DiException? ex = null;
            CachedTypeInfo cachedTypeInfo = TypeInfoCache.GetTypeInfo(instance);
            List<object> createdTransients = new List<object>();
            foreach (FieldInfo fieldInfo in cachedTypeInfo._fields)
            {
#pragma warning disable CS0612
                FieldInjectResult result = InjectField(instance, fieldInfo);
#pragma warning restore CS0612
                if (result.Error != null)
                {
                    ex ??= new DiException(result.Error);
                }
                else if (result.Transient != null)
                {
                    createdTransients.Add(result.Transient);
                }
            }

            if (ex == null)
            {
                _transients.AddRange(createdTransients);
                foreach (object created in createdTransients)
                {
                    ex ??= InjectRecursive(container, created, depth + 1);
                }
            }
            
            return ex;
        }
        
        [Obsolete("//TODO - [2.5.+] Create JIRA -determine if we need/want this as it bypasses transients and scopes", false)]
        private FieldInjectResult InjectField(object instance, FieldInfo fieldInfo)
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

            if (_transientsResolvers != null && _transientsResolvers.TryGetValue(type, out Resolver? resolver))
            {
                InternalUtils.VerboseLog(_name, 
                    ZString.Format("Creating {0} for field {1} belong to {2}", InternalUtils.FormatTypeName(type), fieldInfo.Name,
                        InternalUtils.FormatTypeName(fieldInfo.DeclaringType!)), ContainerLogFlag.Instantiation);
                dependency = resolver(this);
                fieldInfo.SetValue(instance, dependency);
                return FieldInjectResult.FromSuccess(dependency);
            }

            // Stop recursion if we just processed the root container.
            if (this != Root)
            {
#pragma warning disable CS0612
                return _parent.InjectField(instance, fieldInfo);
#pragma warning restore CS0612
            }

            if (fieldInfo.IsNullableReference())
            {
                //VerboseWarn($"Optional type {InternalUtils.FormatTypeName(t)} for field {fieldInfo.Name} in type {InternalUtils.FormatTypeName(instance.GetType())} not found. ");
                return FieldInjectResult.FromSuccess();
            }

            string error = ZString.Format("Failed to located dependency of type {0} for field {1}.{2}.",
                InternalUtils.FormatTypeName(t), InternalUtils.FormatTypeName(instance.GetType()), fieldInfo.Name);
            InternalUtils.VerboseWarn(_name, error, ContainerLogFlag.Injection);
            return FieldInjectResult.FromError(error);
        }
    }
}
#nullable disable