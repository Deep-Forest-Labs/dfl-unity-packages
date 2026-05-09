#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using DeepForestLabs.Logger;
using DeepForestLabs.Common;

namespace DeepForestLabs.Reflections
{
    internal sealed class CachedTypeInfo
    {
        private const BindingFlags FIELD_BINDINGS = BindingFlags.Instance | BindingFlags.NonPublic;
        //private const BindingFlags PROPERTY_BINDINGS = BindingFlags.Instance | BindingFlags.NonPublic;
        
        public readonly Type _type;
        public readonly IReadOnlyList<FieldInfo> _fields;

        public CachedTypeInfo(Type type)
        {
            _type = type;
            _fields = FindInjectableFields(type);
        }

        private static IReadOnlyList<FieldInfo> FindInjectableFields(Type type, bool warn = false)
        {
            List<FieldInfo> fields = new List<FieldInfo>();

//            foreach (PropertyInfo propertyInfo in type.GetProperties(PROPERTY_BINDINGS))
//            {
//                if (!propertyInfo.IsDefined(typeof(DependencyAttribute)))
//                {
//                    continue;
//                }
//
//                if (propertyInfo.CanWrite)
//                {
//                    throw DiException.FromFormat("{0} {1} on type {2} must be readonly.", nameof(PropertyInfo), propertyInfo.Name, Container.FormatTypeName(type));
//                }
//                    
//                FieldInfo? fieldInfo = GetBackingField(propertyInfo);
//
//                if (fieldInfo == null)
//                {
//                    throw DiException.FromFormat("Failed to find backing {0} for {1} {2} on type {3} must be readonly.", nameof(FieldInfo), nameof(PropertyInfo), propertyInfo.Name, Container.FormatTypeName(type));
//                }
//
//                fields.Add(fieldInfo);
//            }

            foreach (FieldInfo fieldInfo in type.GetFields(FIELD_BINDINGS))
            {
                if (!fieldInfo.IsDefined(typeof(DependencyAttribute)))
                {
                    continue;
                }
                if (fieldInfo.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    continue;
                }
                    
                if (!fieldInfo.IsInitOnly)
                {
                    throw DiException.FromFormat("{0} {1} on type {2} must be readonly.", nameof(FieldInfo), fieldInfo.Name, InternalUtils.FormatTypeName(type));
                }

                if (fieldInfo.IsPrivate && warn)
                {
                    Log.Warning($"{type.Name}.{fieldInfo.Name} needs to be protected, not private.");
                }
                
                fields.Add(fieldInfo);
            }

            #if !RELEASE_BUILD
            //Recursive if we use "private" in base classes
            if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(UnityEngine.Object))
            {
                FindInjectableFields(type.BaseType, true);
            }
            #endif

            return fields;
        }
            
        // private static FieldInfo? GetBackingField(PropertyInfo propertyInfo)
        // {
        //     if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(true)
        //             .IsDefined(typeof(CompilerGeneratedAttribute), true))
        //     {
        //         return null;
        //     }
        //
        //     FieldInfo? backingField = propertyInfo.DeclaringType?.GetField(ZString.Format("<{0}>k__BackingField", propertyInfo.Name),
        //         BindingFlags.Instance | BindingFlags.NonPublic);
        //     if (backingField == null)
        //     {
        //         return null;
        //     }
        //
        //     if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), true))
        //     {
        //         return null;
        //     }
        //         
        //     return backingField;
        // }
    }
}
#nullable disable