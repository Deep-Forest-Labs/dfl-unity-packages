#nullable enable
using System;
using System.Collections.Generic;

namespace DeepForestLabs.Reflections
{
    internal static class TypeInfoCache
    {
        private static readonly Dictionary<Type, CachedTypeInfo> CACHE = new();

        public static CachedTypeInfo GetTypeInfo(object obj) => GetTypeInfo(obj.GetType());
        
        public static CachedTypeInfo GetTypeInfo(Type type)
        {
            if (!CACHE.TryGetValue(type, out var info))
            {
                CACHE.Add(type, info = new CachedTypeInfo(type));
            }

            return info;
        }
    }
}
#nullable disable