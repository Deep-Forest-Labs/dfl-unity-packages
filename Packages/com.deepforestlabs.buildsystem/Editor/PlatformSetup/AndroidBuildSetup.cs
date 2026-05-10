#nullable enable
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using Unity.Android.Types;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using AndroidBuildSystem = UnityEditor.AndroidBuildSystem;

namespace DeepForestLabs.BuildSystems.PlatformSetup
{
    public sealed class AndroidBuildSetup : IPlatformBuildSetup
    {
        public BuildTarget Target => BuildTarget.Android;
        public NamedBuildTarget NamedTarget => NamedBuildTarget.Android;

        public void ConfigureProjectSettings(CommandLineArgs args)
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize);
            EditorUserBuildSettings.buildAppBundle = args.BuildAppBundle;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            PlayerSettings.Android.splitApplicationBinary = args.BuildAppBundle;
            UnityEditor.Android.UserBuildSettings.DebugSymbols.level = args.IsReleaseBuild && !args.IsDebugBuild
                ? DebugSymbolLevel.SymbolTable
                : DebugSymbolLevel.Full;

            AssetDatabase.SaveAssets();
        }

        public string GetOutputPath(CommandLineArgs args, string baseBuildPath)
        {
            if (args.BuildAppBundle)
            {
                return Path.Combine(baseBuildPath, AndroidAABName(args.Environment, args.BuildNumber, args.UniqueId));
            }

            if (!PlayerSettings.Android.buildApkPerCpuArchitecture)
            {
                return Path.Combine(baseBuildPath, AndroidAPKName(args.Environment, args.BuildNumber, args.UniqueId));
            }

            return baseBuildPath;
        }

        public int GetBuildNumber() => PlayerSettings.Android.bundleVersionCode;

        public void SetBuildNumber(int buildNumber)
        {
            PlayerSettings.Android.bundleVersionCode = buildNumber;
        }

        public Dictionary<string, string> GetDefaultPlatformArgs()
        {
            return new Dictionary<string, string>
            {
                ["buildAppBundle"] = (EditorUserBuildSettings.buildAppBundle && PlayerSettings.Android.splitApplicationBinary).ToString()
            };
        }

        public static string AndroidAPKName(string environment, int buildNumber, string uniqueId)
        {
            string result = PlayerSettings.productName.Replace(" ", string.Empty);
            return ZString.Format("{0}_{1}_{2}_{3}.apk", result, uniqueId, buildNumber, environment.ToLower());
        }

        public static string AndroidAABName(string environment, int buildNumber, string uniqueId)
        {
            string result = PlayerSettings.productName.Replace(" ", string.Empty);
            return ZString.Format("{0}_{1}_{2}_{3}.aab", result, uniqueId, buildNumber, environment.ToLower());
        }
    }
}
#nullable disable
