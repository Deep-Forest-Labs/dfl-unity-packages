#nullable enable
using UnityEditor;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public static class PlatformBuildSetupResolver
    {
        public static IPlatformBuildSetup Resolve(BuildTarget target) => target switch
        {
            BuildTarget.Android => new AndroidBuildSetup(),
            BuildTarget.iOS => new iOSBuildSetup(),
            BuildTarget.StandaloneWindows64 => new StandaloneBuildSetup(),
            BuildTarget.StandaloneWindows => new StandaloneBuildSetup(),
            BuildTarget.StandaloneOSX => new StandaloneBuildSetup(),
            BuildTarget.StandaloneLinux64 => new LinuxStandaloneBuildSetup(),
            BuildTarget.WebGL => new WebGLBuildSetup(),
            _ => throw new BuildException($"Unsupported build target: {target}")
        };

        public static IPlatformBuildSetup Resolve()
            => Resolve(EditorUserBuildSettings.activeBuildTarget);
    }
}
#nullable disable
