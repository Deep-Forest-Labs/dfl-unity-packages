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
            BuildLog.Info("Clear IncludeInBuild only on atlases that are Addressable, so they are not embedded and then loaded from a bundle. Atlases that are not in a group stay in the player.");

            SpriteAtlasUtils.SetAllIncludeInBuild(false);
 
            BuildLog.Info("##### SpriteAtlas PreprocessBuild end ######");
        }
    }
}
#nullable disable