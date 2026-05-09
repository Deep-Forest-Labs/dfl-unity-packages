#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DeepForestLabs.BuildSystems.AddressablesBuildScripts
{
	[CreateAssetMenu(fileName = nameof(AssetBundlesBuildMode), menuName = "Addressables/Content Builders/Deep Forest Labs/Asset Bundles Build Mode")]
	public sealed class AssetBundlesBuildMode : BuildScriptPackedMode
    {
        public override string Name => "Build Asset Bundles";

        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
	        BuildScript.buildCompleted += OnBuildComplete;
	        
	        AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
	        bool isContentUpdate = abs.AssetId != AddressablesBuildSettings.RELEASE_ASSET_ID;
	        if (isContentUpdate && builderInput.PreviousContentState == null)
	        {
		        return AddressableAssetBuildResult.CreateResult<TResult>(null, 0,
			        "Content update missing previous content state.");
	        }
	        string label = isContentUpdate ? "Content Update Build" : "Content Build";
	        BuildLog.Info("Addressables {0} {1} Started", Name, label);
	        
	        ProjectConfigData.GenerateBuildLayout = true;
	        Stopwatch timer = new();
	        timer.Start();
	        
	        TResult result = base.BuildDataImplementation<TResult>(builderInput);

	        CopyBuildArtifacts();
	        
	        TimeSpan delta = timer.Elapsed;
	        string buildTime = ZString.Format("{0:00}:{1:00}:{2:00}", delta.Hours, delta.Minutes, delta.Seconds);
	        result.Duration = timer.Elapsed.TotalSeconds;
	        if (!string.IsNullOrEmpty(result.Error))
	        {
		        BuildLog.Error("Addressables {0} {1} failed with error '{2}'. Time to Failure: {3}", Name, label, result.Error, buildTime);
	        }
	        else
	        {
		        BuildLog.Info("Addressables {0} {1} Finished. Time to Build: {2}", Name, label, buildTime);    
	        }
            
            return result;
        }

        private void OnBuildComplete(AddressableAssetBuildResult report)
        {
	        BuildScript.buildCompleted -= OnBuildComplete;
	        
	        CopyPostBuildArtifacts();
        }

        public override void ClearCachedData()
        {
	        base.ClearCachedData();
	        
	        if (Directory.Exists(Addressables.LibraryPath))
	        {
		        TryDeleteDirectory(Addressables.LibraryPath);
	        }
	        if (Directory.Exists(Addressables.BuildPath))
	        {
		        TryDeleteDirectory(Addressables.BuildPath);
	        }
	        if (Directory.Exists(Variables.LocalBuildPath))
	        {
		        TryDeleteDirectory(Variables.LocalBuildPath);
	        }
	        if (Directory.Exists(Variables.RemoteBuildPath))
	        {
		        TryDeleteDirectory(Variables.RemoteBuildPath);
	        }
        }

        public static void TryDeleteDirectory(string path)
        {
	        foreach (string directory in Directory.GetDirectories(path))
	        {
		        TryDeleteDirectory(directory);
	        }

	        try
	        {
		        Directory.Delete(path, true);
	        }
	        catch (IOException)
	        {
		        Directory.Delete(path, true);
	        }
	        catch (UnauthorizedAccessException)
	        {
		        Directory.Delete(path, true);
	        }
        }
        
        private static void CopyBuildArtifacts()
        {
	        AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
	        string catalogExt = abs.EnableJsonCatalog ? "json" : "bin";

	        // Copy catalog.bin/json to a unique id
	        string fromPath = Path.Combine(Variables.RemoteBuildPath, ZString.Format("catalog_{0}.{1}", AddressablesBuildSettings.RELEASE_ASSET_ID, catalogExt));
	        string toPath = ZString.Format("{0}/catalog_{1}.{2}", Variables.RemoteBuildPath, abs.AssetId, catalogExt);
	        if (fromPath != toPath)
	        {
		        if (File.Exists(fromPath))
		        {
			        BuildLog.Info("Copying '{0}' to '{1}'.", fromPath, toPath);
			        File.Copy(fromPath, toPath, true);
		        }
		        else
		        {
			        BuildLog.Error("Copying from '{0}' to '{1}' failed. Invalid path.", fromPath,
				        toPath);
			        return;
		        }
	        }

	        // Copy catalog.hash to a unique id
	        fromPath = Path.Combine(Variables.RemoteBuildPath, ZString.Format("catalog_{0}.hash", AddressablesBuildSettings.RELEASE_ASSET_ID));
	        toPath = ZString.Format("{0}/catalog_{1}.hash", Variables.RemoteBuildPath, abs.AssetId);
	        if (fromPath != toPath)
	        {
		        if (File.Exists(fromPath))
		        {
			        BuildLog.Info("Copying '{0}' to '{1}'.", fromPath, toPath);
			        File.Copy(fromPath, toPath, true);
		        }
		        else
		        {
			        BuildLog.Error("Copying from '{0}' to '{1}' failed. Invalid path.", fromPath,
				        toPath);
			        return;
		        }
	        }

	        // Copy Content State
	        fromPath = ContentUpdateScript.GetContentStateDataPath(false);
	        toPath = ZString.Format("{0}/addressables_content_state_{1}.bin", Variables.RemoteBuildPath, abs.AssetId);
	        if (File.Exists(fromPath))
	        {
		        BuildLog.Info("Copying '{0}' to '{1}'.", fromPath, toPath);
		        File.Copy(fromPath, toPath, true);
	        }
	        else
	        {
		        BuildLog.Error("Copying from '{0}' to '{1}' failed. Invalid path.", fromPath,
			        toPath);
		        return;
	        }
        }
        
        private static void CopyPostBuildArtifacts()
        {
	        AddressablesBuildSettings abs = BuildSettings.Instance.Addressables;
	        string fromPath;
	        string toPath;
	        
			// Copy AddressablesBuildTEP.json
	        fromPath = ZString.Concat(Addressables.LibraryPath, "AddressablesBuildTEP.json");
	        if (File.Exists(fromPath))
	        {
		        toPath = ZString.Format("{0}/AddressablesBuildTEP_{1}.json", Variables.RemoteBuildPath, abs.AssetId);
		        BuildLog.Info("Copying '{0}' to '{1}'.", fromPath, toPath);
		        File.Copy(fromPath, toPath, true);
	        }
	        else
	        {
		        BuildLog.Error("Failed to find AddressablesBuildTEP.json.");
		        return;
	        }
			 
	        // Copy build layout
	        fromPath = ZString.Concat(Addressables.LibraryPath, "buildlayout.json");
	        if (File.Exists(fromPath))
	        {
		        toPath = ZString.Format("{0}/buildlayout_{1}.json", Variables.RemoteBuildPath, abs.AssetId);
		        BuildLog.Info("Copying '{0}' to '{1}'.", fromPath, toPath);
		        File.Copy(fromPath, toPath, true);
	        }
	        else
	        {
		        BuildLog.Error("Failed to find build report.");
		        return;
	        }
        }
    }
}
#nullable disable