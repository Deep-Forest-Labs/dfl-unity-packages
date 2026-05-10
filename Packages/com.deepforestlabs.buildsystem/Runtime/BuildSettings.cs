#nullable enable
using System;
using DeepForestLabs.Logger;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
	public class BuildSettings : ScriptableObject
	{
		public const string RESOURCES_PATH = nameof(BuildSettings);

		protected static BuildSettings? _instance = null;
		public static BuildSettings Instance => _instance ??= Load();
		
		[SerializeField] public string _appName = string.Empty;
		[SerializeField] public string _buildTarget = string.Empty;
		[SerializeField] internal bool _isCommandLineBuild;
		[SerializeField] internal int _buildNumber = -1;
		[SerializeField] internal string _versionNumber = string.Empty;
		[SerializeField] internal string _shortVersion = string.Empty;
		[SerializeField] internal bool _isDebugBuild = false;
		[SerializeField] internal bool _isReleaseBuild = false;
		[SerializeField] internal bool _isTestFlightBuild = false;
		[SerializeField] internal string _scriptingDefines = string.Empty;
		[SerializeField] internal int _targetFps = 30;
		[SerializeField] internal int _vscyncCount = 0;
		[SerializeField] internal string _analyticsAppid = String.Empty;
		[SerializeField] internal string _analyticsSubversion = "1.6.0";
		[SerializeField] internal ContainerLogFlag _containerLogFlag;
		[SerializeField] internal EnvironmentBuildSettings _environment;
		[SerializeField] internal ScreenOrientation _orientation = ScreenOrientation.Portrait;
		[SerializeField] internal AddressablesBuildSettings _addressables = null!;

		public string BuildTarget => _buildTarget;
		public string AppName => _appName;
		public bool IsCommandLineBuild => _isCommandLineBuild;
		public int BuildNumber => _buildNumber;
		public string VersionNumber => _versionNumber;
		public string FullVersionNumber => _versionNumber;
		public string ShortVersion => _shortVersion;
		public bool IsDebugBuild => _isDebugBuild;
		public bool IsReleaseBuild => _isReleaseBuild;
		public bool IsTestFlightBuild => _isTestFlightBuild;
		public string ScriptingDefines => _scriptingDefines;
		public int TargetFps => _targetFps;
		public int VSyncCount => _vscyncCount;
		public string AnalyticsAppid => _analyticsAppid;
		public string AnalyticsSubversion => _analyticsSubversion;
		public ContainerLogFlag ContainerLogFlag => _containerLogFlag;
		public EnvironmentBuildSettings Environment => _environment;
		public ScreenOrientation Orientation => _orientation;
		public AddressablesBuildSettings Addressables => _addressables;

		private static BuildSettings Load()
		{
			if (_instance == null)
			{
				_instance = Resources.Load(RESOURCES_PATH) as BuildSettings 
				            ?? throw new GameException("Failed to load BuildConfiguration.");
			}

			return _instance;
		}
	}
}

#nullable disable
