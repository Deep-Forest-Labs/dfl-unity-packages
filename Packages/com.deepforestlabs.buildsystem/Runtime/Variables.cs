#nullable enable
using System;
using System.IO;
using DeepForestLabs.Logger;
using UnityEngine;
using AddressableImpl = UnityEngine.AddressableAssets.Addressables;

namespace DeepForestLabs.BuildSystems
{
    // Must be static to work with Addressables ProfileValueReference
    public static class Variables
    {
        private static string? _assetId;
        private static string? _cdn;
        private static string? _outputRoot;
        
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

        /// <summary>
        /// Absolute directory that owns AssetBundles/ (and, via BuilderUtils, Builds/
        /// and Backups/). Empty or null restores the project-root default.
        /// Editor entry points push the resolved -outputRoot here because this
        /// type lives in Runtime and cannot see CommandLineArgs.
        /// </summary>
        public static void ConfigureOutputRoot(string? root)
        {
            _outputRoot = string.IsNullOrEmpty(root) ? null : root;
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

        private static string GetOutputRoot()
        {
#if !UNITY_EDITOR
            throw new InvalidOperationException("GetOutputRoot is Editor-only.");
#else
            if (!string.IsNullOrEmpty(_outputRoot))
            {
                return Path.GetFullPath(_outputRoot);
            }

            // Parent of Assets/ — the Unity project root.
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
#endif
        }

        private static string GetRemoteBuildPath()
        {
#if !UNITY_EDITOR
            // Prevent accidental use in a built player
            throw new InvalidOperationException("GetRemoteBuildPath is Editor-only. At runtime use GetRemoteLoadPath instead.");
#else
            return Path.Combine(
                GetOutputRoot(),
                "AssetBundles",
                BuildSettings.Instance.Addressables.UniqueId,
                BuildSettings.Instance.BuildTarget);
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
            _outputRoot = null;
        }
    }
}

#nullable disable
