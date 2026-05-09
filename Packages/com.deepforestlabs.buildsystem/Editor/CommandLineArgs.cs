#nullable enable
using System;
using UnityEditor;

namespace DeepForestLabs.BuildSystems
{
    [Serializable]
    public readonly struct CommandLineArgs
    {
        public string BuildTarget { get; }
        public bool IsCommandLineBuild { get; }
        public int BuildNumber { get; }
        public string Version { get; }
        public string ShortVersion { get; }
        public string Environment { get; }
        public string UniqueId { get; }
        public string AssetId { get; }
        public bool EnableJsonCatalog { get; }
        public string ContentStateDataPath { get; } 
        public bool IsDebugBuild { get; }
        public bool IsReleaseBuild { get; }
        public bool IsTestFlightBuild { get; }
        public bool BuildAppBundle { get; }
        public string ScriptingDefines { get; }
        public string OverrideEnvironmentUri { get; }
        public BuildOptions BuildOptions { get; }

        public CommandLineArgs(string buildTarget, bool isCommandLineBuild, int buildNumber, string version, string shortVersion,
            string environment, string uniqueId, string assetId, bool enableJsonCatalog, string contentStateDataPath, bool isDebugBuild,
            bool isReleaseBuild, bool isTestFlightBuild, bool buildAppBundle, string scriptingDefines, 
            string overrideEnvironmentUri, BuildOptions buildOptions)
        {
            BuildTarget = buildTarget;
            IsCommandLineBuild = isCommandLineBuild;
            BuildNumber = buildNumber;
            Version = version;
            ShortVersion = shortVersion;
            Environment = environment;
            UniqueId = uniqueId;
            AssetId = assetId;
            EnableJsonCatalog = enableJsonCatalog;
            ContentStateDataPath = contentStateDataPath;
            IsDebugBuild = isDebugBuild;
            IsReleaseBuild = isReleaseBuild;
            IsTestFlightBuild = isTestFlightBuild;
            BuildAppBundle = buildAppBundle;
            ScriptingDefines = scriptingDefines;
            OverrideEnvironmentUri = overrideEnvironmentUri;
            BuildOptions = buildOptions;
        }
    }
}
#nullable disable