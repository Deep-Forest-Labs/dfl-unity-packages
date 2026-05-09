#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
	public sealed class BuildSystemSettings : ScriptableObject, IComparer<AddressableAssetGroup>
	{
		private const string ASSET_DATABASE_PATH = "Assets/Editor/BuildSystemSettings.asset";
		
		public static bool SettingsExist => AssetDatabase.LoadAssetAtPath<BuildSystemSettings>(ASSET_DATABASE_PATH);

		public static BuildSystemSettings GetSettings()
		{
			BuildSystemSettings settings;
			if (AssetDatabase.LoadAssetAtPath<BuildSystemSettings>(ASSET_DATABASE_PATH) != null)
			{
				settings = AssetDatabase.LoadAssetAtPath<BuildSystemSettings>(ASSET_DATABASE_PATH);
			}
			else
			{
				throw new BuildException(ZString.Format("{0} not configured yet.", nameof(BuildSystemSettings)));
			}

			return settings;
		}

		[SerializeField] internal string? _environmentsUrl = EnvironmentsDownloader.DEFAULT_URL;
		[SerializeField] internal string _serverCommsKey = string.Empty;
		[SerializeField] internal string _userServiceKey = string.Empty;
		[SerializeField] internal string _appName = string.Empty;
		[SerializeField] internal int _targetFps = 30;
		[SerializeField] internal int _vscyncCount = 0;
		[SerializeField] internal string _analyticsAppid = string.Empty;
		[SerializeField] internal string _analyticsSubversion = "1.6.0";
		[SerializeField] internal BuildOptions _buildOptions;
		[SerializeField] internal BuildOptions _debugBuildOptions;
		[SerializeField] internal BuildOptions _releaseBuildOptions;

		public string EnvironmentsUrl
		{
			get
			{
				CommandLineArgs args = BuildSystemEntryPoint.ReadArgs();
				return string.IsNullOrWhiteSpace(args.OverrideEnvironmentUri)
					? _environmentsUrl ?? EnvironmentsDownloader.DEFAULT_URL
					: args.OverrideEnvironmentUri;
			}
		}

		public string ServerCommsKey => _serverCommsKey;
		public string UserServiceKey => _userServiceKey;
		public string AppName => _appName;
		public int TargetFps => _targetFps;
		public int VscyncCount => _vscyncCount;
		public string AnalyticsAppid => _analyticsAppid;
		public string AnalyticsSubversion => _analyticsSubversion;
		public BuildOptions DebugBuildOptions => _debugBuildOptions = BuildOptions.CompressWithLz4HC | BuildOptions.CleanBuildCache | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
		public BuildOptions ReleaseBuildOptions => _releaseBuildOptions = BuildOptions.CompressWithLz4HC | BuildOptions.CleanBuildCache;
		

		public int Compare(AddressableAssetGroup x, AddressableAssetGroup y)
		{
			return String.Compare(x.name, y.name, StringComparison.Ordinal);
		}
	}
}
#nullable disable