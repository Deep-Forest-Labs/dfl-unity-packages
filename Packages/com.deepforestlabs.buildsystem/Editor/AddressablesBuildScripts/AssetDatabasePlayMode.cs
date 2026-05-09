#nullable enable
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace DeepForestLabs.BuildSystems.AddressablesBuildScripts
{
    [CreateAssetMenu(fileName = nameof(AssetDatabasePlayMode), menuName = "Addressables/Content Builders/Deep Forest Labs/Asset Database Play Mode")]
    public sealed class AssetDatabasePlayMode : BuildScriptFastMode
    {
        public override string Name => "Play with Asset Database";
    }
}