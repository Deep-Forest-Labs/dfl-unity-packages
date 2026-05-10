#nullable enable
using System;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEngine;
using AddressableImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.BuildSystems
{
    // Must be static to work with Addressables ProfileValueReference
    public static class Variables
    {
        private static string? _assetId;
        private static string? _cdn;
        
        // Built variables
        public static string LocalBuildPath => GetLocalBuildPath();
        public static string LocalLoadPath=> GetLocalLoadPath();
        public static string RemoteBuildPath => GetRemoteBuildPath();
        
        // Runtime variables
        public static string RemoteLoadPath => GetRemoteLoadPath();
        public static string AssetId => _assetId ?? BuildSettings.Instance.Addressables.AssetId;
        
        public static bool IsRuntimeVariablesConfigured => _cdn != null || _assetId != null;

        public static void Configure(string cdn)
        {
            _cdn = cdn.Substring(0, cdn.LastIndexOf('/'));
            _assetId = cdn.Substring(cdn.LastIndexOf('/') + 1);
            if (Application.isEditor)
            {
                AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
                if (abs.AssetId != AddressablesBuildSettings.RELEASE_ASSET_ID &&  abs.AssetId != _assetId)
                {
                    Log.Warning("Overriding asset_id '{0}' with '{1}'.", _assetId, abs.AssetId);
                    _assetId = abs.AssetId;
                }
            }
        }
        
        private static string GetLocalBuildPath()
        {
#if !UNITY_EDITOR
            // Prevent accidental use in a built player
            throw new InvalidOperationException("GetLocalBuildPath is Editor-only. At runtime use GetLocalLoadPath instead.");
#else
            return AddressableImpl.BuildPath;
#endif
        }
        
        private static string GetLocalLoadPath()
        {
#if !UNITY_EDITOR
            if (BuildSettings.Instance.Addressables.LoadStrategy == AssetLoadStrategy.LocalBundles)
            {
                return UnityEngine.AddressableAssets.Addressables.RuntimePath;
            }
            return UnityEngine.AddressableAssets.Addressables.RuntimePath;
#else
            AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
            BuilderIndex playModeName = abs.ActivePlayModeIndex;
            switch (playModeName)
            {
                case BuilderIndex.RemoteAssetBundlePlayMode:
                    return GetRemoteLoadPath();

                case BuilderIndex.LocalAssetBundlePlayMode:
                case BuilderIndex.AssetDatabasePlayMode:
                default:
                    return GetLocalBuildPath();
            }
#endif
        }

        private static string GetRemoteBuildPath()
        {
#if !UNITY_EDITOR
            // Prevent accidental use in a built player
            throw new InvalidOperationException("GetRemoteBuildPath is Editor-only. At runtime use GetRemoteLoadPath instead.");
#else
            return new Uri(ZString.Format("{0}/../../AssetBundles/{1}/{2}", 
                Application.dataPath, 
                BuildSettings.Instance.Addressables.UniqueId, 
                BuildSettings.Instance.BuildTarget)
            ).AbsolutePath;
#endif
        }
        
        private static string GetRemoteLoadPath()
        {
#if !UNITY_EDITOR
            if (BuildSettings.Instance.Addressables.LoadStrategy == AssetLoadStrategy.LocalBundles)
            {
                return UnityEngine.AddressableAssets.Addressables.RuntimePath;
            }
            Log.Assert(_cdn != null, nameof(_cdn) + " != null");
            return _cdn;
#else
            AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
            BuilderIndex playModeName = abs.ActivePlayModeIndex;
            switch (playModeName)
            {
                case BuilderIndex.RemoteAssetBundlePlayMode:
                    Log.Assert(_cdn != null, nameof(_cdn) + " != null");
                    return _cdn;
                
                case BuilderIndex.LocalAssetBundlePlayMode:
                case BuilderIndex.AssetDatabasePlayMode:
                default:
                    return GetRemoteBuildPath();
            }
#endif
        }
        
        public static void Clear()
        {
            _cdn = null;
            _assetId = null;
        }
    }
}

#nullable disable