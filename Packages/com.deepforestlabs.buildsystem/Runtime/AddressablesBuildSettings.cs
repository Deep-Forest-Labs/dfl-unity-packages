#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
    [Serializable]
    public sealed class AddressablesBuildSettings
    {
        [SerializeField] internal string _uniqueId = DEFAULT_UNIQUE_VALUE;
        [Tooltip("Unique identifier for a content updates.  Otherwise set to 'release'")]
        [SerializeField] internal string _assetId = RELEASE_ASSET_ID;
        [SerializeField] internal bool _enableJsonCatalog = false;
        [SerializeField] [HideInInspector] internal BuilderIndex _activePlayModeIndex;

        public string UniqueId => _uniqueId;
        public string AssetId => _assetId;
        public bool EnableJsonCatalog => _enableJsonCatalog;
#if UNITY_EDITOR
        public BuilderIndex ActivePlayModeIndex => _activePlayModeIndex;
#endif
        
        public const string RELEASE_ASSET_ID = "release";
        public const string DEFAULT_UNIQUE_VALUE = "default";
    }
}
#nullable disable