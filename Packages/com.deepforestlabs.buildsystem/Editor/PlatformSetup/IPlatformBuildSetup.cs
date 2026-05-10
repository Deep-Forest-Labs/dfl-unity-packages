#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public interface IPlatformBuildSetup
    {
        BuildTarget Target { get; }
        NamedBuildTarget NamedTarget { get; }
        void ConfigureProjectSettings(CommandLineArgs args);
        string GetOutputPath(CommandLineArgs args, string baseBuildPath);
        int GetBuildNumber();
        void SetBuildNumber(int buildNumber);
        Dictionary<string, string> GetDefaultPlatformArgs();
    }
}
#nullable disable
