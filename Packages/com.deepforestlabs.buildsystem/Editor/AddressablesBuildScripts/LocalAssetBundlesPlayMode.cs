using System.IO;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.AddressablesBuildScripts
{
    [CreateAssetMenu(fileName = nameof(LocalAssetBundlesPlayMode), menuName = "Addressables/Content Builders/Deep Forest Labs/Local Asset Bundles Play Mode")]
    public sealed class LocalAssetBundlesPlayMode : BuildScriptPackedPlayMode
    {
        public override string Name => "Play with Local Asset Bundles";
        
        public override void ClearCachedData()
        {
            base.ClearCachedData();
            
            if (Directory.Exists(Variables.LocalBuildPath))
            {
                AssetBundlesBuildMode.TryDeleteDirectory(Variables.LocalBuildPath);
            }
            if (Directory.Exists(Variables.RemoteBuildPath))
            {
                AssetBundlesBuildMode.TryDeleteDirectory(Variables.RemoteBuildPath);
            }
        }
    }
}