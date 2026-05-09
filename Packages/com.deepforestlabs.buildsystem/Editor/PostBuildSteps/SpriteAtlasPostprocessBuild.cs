#nullable enable
using DeepForestLabs.Logger;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PostBuildSteps
{
    public class SpriteAtlasPostprocessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder { get; } = (int)PostBuildOrder.SpriteAtlasV2;
 
        public void OnPostprocessBuild(BuildReport report)
        {
            BuildLog.Info("When loading the sprite atlases via Addressable, we should avoid they getting build into the app. Otherwise, the sprite atlases will get loaded into memory twice.");
            BuildLog.Info("Set the `IncludeInBuild` flag to `true` for all sprite atlases after finishing the App build.");
            SpriteAtlasUtils.SetAllIncludeInBuild(true);
        }
    }
}
#nullable disable