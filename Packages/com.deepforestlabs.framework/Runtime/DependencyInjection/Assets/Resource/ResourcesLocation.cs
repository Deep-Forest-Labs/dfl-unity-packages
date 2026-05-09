#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DeepForestLabs.Assets.Resource
{
    internal sealed class ResourcesLocation : IResourceLocation
    {
        public string InternalId { get; private set; }
        public string ProviderId => nameof(ResourcesManager);
        public IList<IResourceLocation> Dependencies => Array.Empty<IResourceLocation>();
        public int Hash(Type resultType)
        {
            throw new NotImplementedException();
        }

        public int DependencyHashCode => default;
        public bool HasDependencies => false;
        public object? Data { get; } = default;
        public string PrimaryKey => InternalId;
        public Type ResourceType { get; }

        public ResourcesLocation(string resourcesPath, Type resourceType)
        {
            InternalId = resourcesPath;
            ResourceType = resourceType;
        }
    }
}