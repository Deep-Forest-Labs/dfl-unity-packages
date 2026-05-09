#nullable enable
using System.Diagnostics;
using System.IO;
using Cysharp.Text;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepForestLabs.BuildSystems.AddressablesBuildScripts
{
    [CreateAssetMenu(fileName = nameof(RemoteAssetBundlesPlayMode), menuName = "Addressables/Content Builders/Deep Forest Labs/Remote Asset Bundles Play Mode")]
	public sealed class RemoteAssetBundlesPlayMode : BuildScriptBase
	{
        /// <inheritdoc />
		public override string Name => ZString.Format("Play using Remote Bundles ({0})", BuildSettings.Instance.Addressables.UniqueId);
        
        /// <inheritdoc />
        public override bool CanBuildData<T>() => BuildSystemSettings.SettingsExist &&
                                                  typeof(T).IsAssignableFrom(typeof(AddressablesPlayModeBuildResult));

        /// <inheritdoc />
        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            PlayerPrefs.DeleteKey(Addressables.kAddressablesRuntimeDataPath);
            PlayerPrefs.DeleteKey(Addressables.kAddressablesRuntimeBuildLogPath);

            IDataBuilderResult res = new AddressablesPlayModeBuildResult()
            {
                OutputPath = string.Empty,
                Duration = timer.Elapsed.TotalSeconds
            };
            return (TResult)res;
        }

        public override void ClearCachedData()
        {
            base.ClearCachedData();

            Caching.ClearCache();
        }
    }
}
#nullable disable