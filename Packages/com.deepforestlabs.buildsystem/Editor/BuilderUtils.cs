#nullable enable
using System;
using System.IO;
using System.Linq;
using DeepForestLabs.BuildSystems.PlatformSetup;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
	[InitializeOnLoad]
	public static class BuilderUtils
	{
		static BuilderUtils()
		{
			if (!Application.isBatchMode && BuildSystemSettings.SettingsExist)
			{
				AddressableAssetSettingsDefaultObject.Settings.OnModification = OnAddressableAssetSettingsModification;
			}

			BuildSettings? buildSettings = AssetDatabase.LoadAssetAtPath<BuildSettings>(BuildSettingsEditor.ASSET_DATABASE_PATH);
			if (buildSettings == null)
			{
				BuildSettingsEditor.Create();
			}

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
		{
			if (stateChange == PlayModeStateChange.ExitingEditMode)
			{
				BuildSettingsEditor.Write(BuildSystemEntryPoint.ReadArgs());
			}
		}

		private static void OnAddressableAssetSettingsModification(AddressableAssetSettings aas, AddressableAssetSettings.ModificationEvent evt, object args)
		{
			switch (evt)
			{
				case AddressableAssetSettings.ModificationEvent.ActivePlayModeScriptChanged:
					BuildSettingsEditor.Write((BuilderIndex)ProjectConfigData.ActivePlayModeIndex);
					break;
			}
		}

		public static BuildPlayerOptions GetBuildPlayerOptions(CommandLineArgs args)
		{
			return new BuildPlayerOptions
			{
				scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
				target = EditorUserBuildSettings.activeBuildTarget,
				options = args.BuildOptions,
				locationPathName = GetOutputPath(args)
			};
		}

		public static string GetOutputPath() => GetOutputPath(BuildSystemEntryPoint.ReadArgs());

		public static void ReturnErrorCode(Exception e)
		{
			BuildLog.Exception(e);
			if (e.InnerException != null)
			{
				BuildLog.Exception(e.InnerException);
			}
			if (BuildSystemEntryPoint.ReadArgs().IsCommandLineBuild)
			{
				EditorApplication.Exit(-2);
			}
			else
			{
				DisplayError(e).Forget();
			}
		}

		private static async UniTask DisplayError(Exception e)
		{
			await UniTask.NextFrame();
			EditorUtility.DisplayDialog("Build Failed", ZString.Format("{0}", e.Message), "ok");
		}

		public static string GetBuildPath()
		{
			CommandLineArgs args = BuildSystemEntryPoint.ReadArgs();
			string buildPath = ZString.Format("{0}/../../Builds/{1}/{2}/", Application.dataPath, args.BuildTarget,
				args.UniqueId);
			if (!Directory.Exists(buildPath))
			{
				Directory.CreateDirectory(buildPath);
			}

			return buildPath;
		}

		public static string GetBackupPath()
		{
			CommandLineArgs args = BuildSystemEntryPoint.ReadArgs();
			string buildPath = ZString.Format("{0}/../../Backups/{1}/{2}/", Application.dataPath, args.BuildTarget,
				args.UniqueId);
			if (!Directory.Exists(buildPath))
			{
				Directory.CreateDirectory(buildPath);
			}

			return buildPath;
		}

		private static string GetOutputPath(CommandLineArgs args)
		{
			string basePath = GetBuildPath();
			if (!Directory.Exists(basePath))
			{
				Directory.CreateDirectory(basePath);
			}

			return PlatformBuildSetupResolver.Resolve().GetOutputPath(args, basePath);
		}

		public static string AndroidAPKName(string environment, int buildNumber, string uniqueId)
			=> AndroidBuildSetup.AndroidAPKName(environment, buildNumber, uniqueId);
	}
}
#nullable disable