#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DeepForestLabs.Assets.Addressables
{
    internal sealed class AssetReferenceRuntimeKeyComparer : IEqualityComparer<AssetReference>
    {
        public static readonly AssetReferenceRuntimeKeyComparer Instance = new();

        public bool Equals(AssetReference? x, AssetReference? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            // RuntimeKey can be string, GUID, or other object
            return Equals(x.RuntimeKey, y.RuntimeKey);
        }

        public int GetHashCode(AssetReference? obj)
        {
            return obj?.RuntimeKey?.GetHashCode() ?? 0;
        }
    }
    
    internal sealed class ResourceLocationRuntimeKeyComparer : IEqualityComparer<IResourceLocation>
    {
        public static readonly ResourceLocationRuntimeKeyComparer Instance = new();

        public bool Equals(IResourceLocation? x, IResourceLocation? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            
            return Equals(x.InternalId, y.InternalId);
        }

        public int GetHashCode(IResourceLocation? obj)
        {
            return obj?.InternalId?.GetHashCode() ?? 0;
        }
    }
    
    internal sealed class ResourceLocationComparer : IEqualityComparer<IResourceLocation>
    {
        public static readonly ResourceLocationComparer Instance = new();
        
        public bool Equals(IResourceLocation? x, IResourceLocation? y)
            => ReferenceEquals(x, y) ||
               (x != null && y != null &&
                x.ProviderId == y.ProviderId &&
                x.PrimaryKey == y.PrimaryKey &&
                x.InternalId == y.InternalId &&
                x.ResourceType == y.ResourceType);
        
        public int GetHashCode(IResourceLocation obj)
            => HashCode.Combine(obj.ProviderId, obj.PrimaryKey, obj.InternalId, obj.ResourceType);
    }
}