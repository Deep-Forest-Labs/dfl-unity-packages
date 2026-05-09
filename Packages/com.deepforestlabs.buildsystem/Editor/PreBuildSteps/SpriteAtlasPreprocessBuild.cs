#nullable enable
using DeepForestLabs.Logger;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
    public class SpriteAtlasPreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } =  (int)PreBuildOrder.SpriteAtlasV2;
 
        public void OnPreprocessBuild(BuildReport report)
        {
            BuildLog.Info("##### SpriteAtlas PreprocessBuild start ######");
            BuildLog.Info("When loading the sprite atlases via Addressable, we should avoid they getting build into the app. Otherwise, the sprite atlases will get loaded into memory twice.");
            BuildLog.Info("Set the `IncludeInBuild` flag to `false` for all sprite atlases before starting the App build.");

            SpriteAtlasUtils.SetAllIncludeInBuild(false);
 
            BuildLog.Info("##### SpriteAtlas PreprocessBuild end ######");
        }
    }
}
#nullable disable