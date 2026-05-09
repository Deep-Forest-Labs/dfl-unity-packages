#nullable enable
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
    public class PodFilePostProcess : MonoBehaviour
    {
#if UNITY_IOS
        [PostProcessBuild(45)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
        private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
        {
            if (target == BuildTarget.iOS)
            {

                using (StreamWriter sw = File.AppendText(buildPath + "/Podfile"))
                {
                    sw.WriteLine("use_frameworks!");
                }
            }
        }
#endif
    }
}

#nullable disable