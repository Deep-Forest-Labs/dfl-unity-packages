#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepForestLabs.BuildSystems.AddressablesBuildScripts;
using DeepForestLabs.Logger;
using Cysharp.Text;
using Unity.Android.Types;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using AndroidBuildSystem = UnityEditor.AndroidBuildSystem;

// ReSharper disable Unity.DuplicateShortcut

namespace DeepForestLabs.BuildSystems
{
    public static class BuildSystemEntryPoint
    {
	    // Command line argument keys
	    private const string EXECUTE_METHOD = "-executeMethod";
	    private const string BUILD_TARGET = "-buildTarget";
	    private const string ENVIRONMENT = "-environment";
	    private const string UNIQUE_ID = "-uniqueId";
	    private const string ASSET_ID = "-assetId";
	    private const string ENABLE_JSON_CATALOG = "-enableJsonCatalog";
	    private const string CONTENT_STATE_DATA_PATH = "-contentStateDataPath";
	    private const string BUILD_NUMBER = "-buildNumber";
	    private const string DEBUG_BUILD = "-debugBuild";
	    private const string RELEASE_BUILD = "-releaseBuild";
	    private const string TESTFLIGHT = "-testFlight";
	    private const string BUILD_APP_BUNDLE = "-buildAppBundle";
	    private const string SCRIPTING_DEFINES = "-scriptingDefines";
	    private const string OVERRIDE_ENVIRONMENT_FILE = "-overrideEnvironmentUrl";
	    private const string FAILED_BUILD_LOG_FORMAT = "Client player build failed with {0} error(s) after {1:hh\\:mm\\:ss}.";

	    [MenuItem("Build/Run _F5", false, 2)]
	    public static void RunFromCommandLine()
	    {
		    if (EditorSceneManager.GetActiveScene().isDirty)
		    {
			    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		    }

		    CommandLineArgs args = ReadArgs();
		    BuildSettingsEditor.Write(args);
		    SetupProjectSetting(args);

			EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
			EditorApplication.EnterPlaymode();
	    }

	    [MenuItem("Build/Run _F5", true, 2)]
	    private static bool CanRunFromCommandLine()
		    => !EditorApplication.isPlaying &&
		       !EditorApplication.isPaused &&
		       !EditorApplication.isCompiling &&
		       !EditorApplication.isUpdating;

	    [MenuItem("Build/Groups _F6", false, 1)]
        public static void BuildGroupsCommandLine()
        {
	        if (!BuildSystemSettings.SettingsExist)
	        {
		        BuilderUtils.ReturnErrorCode(new BuildException("Missing BuildSystemSettings."));
		        return;
	        }

	        try
	        {
		        CommandLineArgs args = ReadArgs();
		        BuildSettingsEditor.Write(args);
		        AddressableAssetSettings aas = AddressableAssetSettingsDefaultObject.Settings;
		        Log.Assert(aas.DataBuilders.FindIndex(b => b is GroupsBuildMode) == (int)BuilderIndex.GroupsBuildMode, "aas.DataBuilders.FindIndex(b => b is GroupsBuildMode) == (int)BuilderIndex.GroupsBuildMode");
		        aas.ActivePlayerDataBuilderIndex = (int)BuilderIndex.GroupsBuildMode;
		        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
		        if (!string.IsNullOrEmpty(result.Error))
		        {
			        throw BuildException.FromFormat("Groups build failed with error '{0}' after {1:hh\\:mm\\:ss}.", result.Error, TimeSpan.FromSeconds(result.Duration));
		        }
	        }
	        catch (Exception e)
	        {
		        BuilderUtils.ReturnErrorCode(e);
	        }
        }

        /// <summary>
       /// This function is called by the CI build job
       /// </summary>
       [MenuItem("Build/Client _F7", false, 3)]
        public static void BuildFromCommandLine()
        {
	        if (!BuildSystemSettings.SettingsExist)
	        {
		        BuilderUtils.ReturnErrorCode(new BuildException("Missing BuildSystemSettings."));
		        return;
	        }
	        CommandLineArgs args = ReadArgs();
	        BuildSettingsEditor.Write(args);
	        SetupProjectSetting(args);
	        AddressableAssetSettings aas = AddressableAssetSettingsDefaultObject.Settings;

	        try
	        {
		        // Build Content
		        Log.Assert(aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode, "aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode");
		        aas.ActivePlayerDataBuilderIndex = (int)BuilderIndex.AssetBundlesBuildMode;
		        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult? result);
		        if (!string.IsNullOrEmpty(result.Error))
		        {
			        throw BuildException.FromFormat("Client content build failed with error '{0}' after {1:hh\\:mm\\:ss}.",
				        result.Error, TimeSpan.FromSeconds(result.Duration));
		        }

		        // Build Player
		        BuildReport? report = BuildPipeline.BuildPlayer(BuilderUtils.GetBuildPlayerOptions(args));
		        Validate(report);
	        }
	        catch (Exception e)
	        {
		        BuilderUtils.ReturnErrorCode(e);
	        }
        }

	    [MenuItem("Build/Client _F7", true, 3)]
	    public static bool CanBuildFromCommandLine() => CanBuildContentFromCommandLine();
	    
        /// <summary>
		/// Use only for test build will compile and builds.  Faster but wont run. 
		/// </summary>
		[MenuItem("Build/Client (No Bundles) #_F7", false, 3)]
        public static void BuildNoAssetsFromCommandLine()
        {
	        if (!Application.isBatchMode)
	        {
		        bool result = EditorUtility.DisplayDialog("Build",
			        "Are you sure you want to build client without assets?  The resulting binary will not run.", "Build",
			        "Cancel");

		        if (!result)
		        {
			        return;
		        }
	        }
	        
            if (!BuildSystemSettings.SettingsExist)
            {
                BuilderUtils.ReturnErrorCode(new BuildException("Missing BuildSystemSettings."));
                return;
            }
            
            CommandLineArgs args = ReadArgs();
            BuildSettingsEditor.Write(args);
            SetupProjectSetting(args);

            try
            {
	            // Build Player
                BuildReport? report = BuildPipeline.BuildPlayer(BuilderUtils.GetBuildPlayerOptions(args));
                Validate(report, new AssetOnlyBuildLogFilter());
            }
            catch (Exception e)
            {
                BuilderUtils.ReturnErrorCode(e);
            }
        }

        /// <summary>
        /// This function is called by the CI build job
        /// </summary>
        [MenuItem("Build/Content _F8", false, 4)]
        public static void BuildContentFromCommandLine()
        {
	        if (!BuildSystemSettings.SettingsExist)
	        {
		        BuilderUtils.ReturnErrorCode(new BuildException("Missing BuildSystemSettings."));
		        return;
	        }

	        CommandLineArgs args = ReadArgs();
	        BuildSettingsEditor.Write(args);
	        AddressableAssetSettings aas = AddressableAssetSettingsDefaultObject.Settings;

	        try
	        {
                // Build Content
                Log.Assert(aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode, "aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode");
                aas.ActivePlayerDataBuilderIndex = (int)BuilderIndex.AssetBundlesBuildMode;
		        AddressableAssetSettings.BuildPlayerContent(out var result);
		        if (!string.IsNullOrEmpty(result.Error))
		        {
			        throw BuildException.FromFormat("Content build failed with error '{0}' after {1:hh\\:mm\\:ss}.", result.Error, TimeSpan.FromSeconds(result.Duration));
		        }
	        }
	        catch (Exception e)
	        {
		        BuilderUtils.ReturnErrorCode(e);
	        }
        }

	    [MenuItem("Build/Content _F8", true, 4)]
	    public static bool CanBuildContentFromCommandLine() =>
		    ReadArgs().AssetId == AddressablesBuildSettings.RELEASE_ASSET_ID;

        /// <summary>
        /// This function is called by the CI build job
        /// </summary>
        [MenuItem("Build/Content Update _F8", false, 5)]
        public static void BuildContentUpdateFromCommandLine()
        {
	        if (!BuildSystemSettings.SettingsExist)
	        {
		        BuilderUtils.ReturnErrorCode(new BuildException("Missing BuildSystemSettings."));
		        return;
	        }

	        CommandLineArgs args = ReadArgs();
	        BuildSettingsEditor.Write(args);
	        AddressableAssetSettings aas = AddressableAssetSettingsDefaultObject.Settings;

	        try
	        {
		        // Build Content
		        Log.Assert(aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode, "aas.DataBuilders.FindIndex(b => b is AssetBundlesBuildMode) == (int)BuilderIndex.AssetBundlesBuildMode");
		        aas.ActivePlayerDataBuilderIndex = (int)BuilderIndex.AssetBundlesBuildMode;
		        AddressablesPlayerBuildResult? result = ContentUpdateScript.BuildContentUpdate(aas, args.ContentStateDataPath);
		        if (!string.IsNullOrEmpty(result.Error))
		        {
			        throw BuildException.FromFormat("Content update build failed with error '{0}' after {1:hh\\:mm\\:ss}.", result.Error, TimeSpan.FromSeconds(result.Duration));
		        }
	        }
	        catch (Exception e)
	        {
		        BuilderUtils.ReturnErrorCode(e);
	        }
        }

        [MenuItem("Build/Content Update _F8", true, 5)]
        public static bool CanBuildContentUpdateFromCommandLine() =>
	        ReadArgs().AssetId != AddressablesBuildSettings.RELEASE_ASSET_ID;

        [MenuItem("Build/Clean/Cached Bundles _F9", false, 6)]
        private static void CleanCache()
        {
	        List<string> cachePaths = new();
	        Caching.GetAllCachePaths(cachePaths);
	        foreach (string? path in cachePaths)
	        {
		        Debug.Log(ZString.Format("Cleaning cache at path '{0}'", path));
	        }
	        Caching.ClearCache();
        }

        [MenuItem("Build/Clean/Builders _F10", false, 7)]
        private static void CleanBuilders()
        {
	        if (!BuildSystemSettings.SettingsExist)
	        {
		        BuilderUtils.ReturnErrorCode(new BuildFailedException("Missing BuildSystemSettings."));
	        }

	        try
	        {
		        Debug.Log(ZString.Format("{0}.{1} Started.", nameof(BuildSystemEntryPoint), nameof(CleanBuilders)));
		        AddressableAssetSettings aas = AddressableAssetSettingsDefaultObject.Settings;
		        foreach (IDataBuilder builder in aas.DataBuilders.OfType<IDataBuilder>())
		        {
			        builder.ClearCachedData();
		        }

		        BuildCache.PurgeCache(false);

		        string path = BuilderUtils.GetBuildPath();
		        if (Directory.Exists(path))
		        {
			        Directory.Delete(path, true);
		        }

		        path = Variables.LocalBuildPath;
		        if (Directory.Exists(path))
		        {
			        Directory.Delete(path, true);
		        }
		        
		        path = Variables.RemoteBuildPath;
		        if (Directory.Exists(path))
		        {
			        Directory.Delete(path, true);
		        }

		        Debug.Log(ZString.Format("{0}.{1} Finished.", nameof(BuildSystemEntryPoint), nameof(CleanBuilders)));
	        }
	        catch (Exception e)
	        {
		        BuilderUtils.ReturnErrorCode(e);
	        }
        }

        [MenuItem("Build/Clean/Build Pipeline _F11", false, 8)]
        private static void CleanBuildPipeline()
        {
	        BuildCache.PurgeCache(false);
        }

        [MenuItem("Build/Settings _F12", false, 9)]
        public static void EditBuildSystemSettings()
        {
	        Selection.activeObject = BuildSystemSettings.GetSettings();
        }

        public static CommandLineArgs ReadArgs()
        {
	        BuildSystemSettings bss = BuildSystemSettings.GetSettings();

		    CommandLineReader reader = new();
		    bool isCommandLineBuild = reader.IsValid(EXECUTE_METHOD);

		    CommandLineArgs args;
		    if (isCommandLineBuild)
		    {
			    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			    Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
			    Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
			    Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
			    Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);

			    // Validate we got args we need
		        Log.Assert(reader.IsValid(BUILD_NUMBER), "Missing required command line arg '{0}'.", BUILD_NUMBER);
		        Log.Assert(reader.IsValid(ENVIRONMENT), "Missing required command line arg '{0}'.", ENVIRONMENT);
		        Log.Assert(reader.IsValid(UNIQUE_ID), "Missing required command line arg '{0}'.", UNIQUE_ID);
		        Log.Assert(reader.IsValid(ASSET_ID), "Missing required command line arg '{0}'.", ASSET_ID);

		        string buildTarget = reader
			        .StringArgument(BUILD_TARGET, EditorUserBuildSettings.activeBuildTarget.ToString());
		        int buildNumber = reader.IntArgument(BUILD_NUMBER, -1);
		        string[] versionParts = PlayerSettings.bundleVersion.Split('.');
		        string version = ZString.Format("{0}.{1}.{2} ({3})", versionParts[0], versionParts[1], versionParts[2],
			        buildNumber);
		        string shortVersion = ZString.Format("{0}.{1}.{2}", versionParts[0], versionParts[1], versionParts[2]);
		        string environment = reader.StringArgument(ENVIRONMENT, "local");
		        string uniqueId = reader.StringArgument(UNIQUE_ID, AddressablesBuildSettings.DEFAULT_UNIQUE_VALUE);
		        string assetId = reader.StringArgument(ASSET_ID, AddressablesBuildSettings.RELEASE_ASSET_ID);
		        bool enableJsonCatalog = reader.BooleanArgument(ENABLE_JSON_CATALOG, false);
		        string contentStatePath = reader.StringArgument(CONTENT_STATE_DATA_PATH,
			        ContentUpdateScript.GetContentStateDataPath(false));
		        bool isDebugBuild = reader.BooleanArgument(DEBUG_BUILD, false);
		        bool isReleaseBuild = reader.BooleanArgument(RELEASE_BUILD, false);
		        bool isTestFlightBuild = reader.BooleanArgument(TESTFLIGHT, false);
		        bool buildAppBundle = reader.BooleanArgument(BUILD_APP_BUNDLE, false);
		        string scriptingDefines = reader.StringArgument(SCRIPTING_DEFINES, string.Empty);
		        string overrideEnvironmentFile = reader.StringArgument(OVERRIDE_ENVIRONMENT_FILE, string.Empty);
		        BuildOptions buildOptions = isDebugBuild ? bss.DebugBuildOptions : bss.ReleaseBuildOptions;

		        args = new CommandLineArgs(buildTarget, isCommandLineBuild, buildNumber, version, shortVersion,
			        environment, uniqueId, assetId, enableJsonCatalog, contentStatePath, isDebugBuild, isReleaseBuild, 
			        isTestFlightBuild, buildAppBundle, scriptingDefines, overrideEnvironmentFile, buildOptions);

		        if (!string.IsNullOrEmpty(args.OverrideEnvironmentUri))
		        {
			        string uri = new Uri(args.OverrideEnvironmentUri).LocalPath;
			        if (!string.IsNullOrEmpty(uri) && !File.Exists(uri))
			        {
				        throw new BuildFailedException(ZString.Format("File not found at path {0}.", uri));
			        }
		        }

		        // If content update, validate we got a content update file path
		        if (args.AssetId != AddressablesBuildSettings.RELEASE_ASSET_ID &&
		            !File.Exists(args.ContentStateDataPath))
		        {
			        throw new BuildFailedException(ZString.Format("Required content update file not found at path {0}.",
				        args.ContentStateDataPath));
		        }
	        }
	        else
	        {
		        BuildSettings bs = BuildSettings.Instance;
		        string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
		        int buildNumber = PlayerSettings.Android.bundleVersionCode;
		        string[] versionParts = PlayerSettings.bundleVersion.Split('.');
		        string version = ZString.Format("{0}.{1}.{2} ({3})", versionParts[0], versionParts[1], versionParts[2], buildNumber);
		        string shortVersion = ZString.Format("{0}.{1}.{2}", versionParts[0], versionParts[1], versionParts[2]);
		        string environment = bs.Environment.Name;
		        string uniqueId = bs.Addressables.UniqueId;
		        bool enableJsonCatalog = AddressableAssetSettingsDefaultObject.Settings.EnableJsonCatalog;
		        uniqueId = string.IsNullOrEmpty(uniqueId) ? AddressablesBuildSettings.DEFAULT_UNIQUE_VALUE : uniqueId;
		        string assetId =  bs.Addressables.AssetId;
		        assetId = string.IsNullOrEmpty(assetId) ? AddressablesBuildSettings.RELEASE_ASSET_ID : assetId;
		        string contentStatePath = ContentUpdateScript.GetContentStateDataPath(false);
		        bool isDebugBuild = Debug.isDebugBuild;
		        bool isReleaseBuild = !Debug.isDebugBuild;
		        bool isTestFlightBuild = false;
		        bool buildAppBundle = EditorUserBuildSettings.buildAppBundle && PlayerSettings.Android.splitApplicationBinary;
		        string scriptingDefines = string.Empty;
		        string overrideEnvironmentFile = EnvironmentsDownloader.DEFAULT_URL;
		        BuildOptions buildOptions = isDebugBuild ? bss.DebugBuildOptions : bss.ReleaseBuildOptions;

		        args = new CommandLineArgs(buildTarget, isCommandLineBuild, buildNumber, version, shortVersion,
			        environment, uniqueId, assetId, enableJsonCatalog, contentStatePath, isDebugBuild, isReleaseBuild, 
			        isTestFlightBuild, buildAppBundle, scriptingDefines, overrideEnvironmentFile, buildOptions);
	        }

		    return args;
        }

        private static void SetupProjectSetting(CommandLineArgs args)
        {
            // Running in OptimizeSpeed can cause issues with Generic JsonConverter AOT
            // Leave as OptimizeSize until addressed. Must be set on all builds as it's a Unity setting,
            // not a project setting.
            /*EditorUserBuildSettings.il2CppCodeGeneration = args.IsReleaseBuild
				? Il2CppCodeGeneration.OptimizeSpeed
				: Il2CppCodeGeneration.OptimizeSize;*/

            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.iOS, Il2CppCodeGeneration.OptimizeSize);
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
        
        private static void Validate(BuildReport report, ILogFilter? filter = null)
        {
	        int errorCount = 0;
	        foreach (BuildStep step in report.steps)
	        {
		        foreach (BuildStepMessage message in step.messages)
		        {
			        switch (message.type)
			        {
				        case LogType.Assert when filter == null || !filter.IsIgnoredAssert(message.content):
				        case LogType.Error when filter == null || !filter.IsIgnoredError(message.content):
				        case LogType.Exception when filter == null || !filter.IsIgnoredError(message.content):
					        errorCount++;
					        Debug.unityLogger.Log(message.type, null, message.content);
					        break;
			        }
		        }
	        }

	        if (errorCount > 0)
	        {
		        throw BuildException.FromFormat(FAILED_BUILD_LOG_FORMAT, errorCount, report.summary.totalTime);
	        }

            if (report.summary.result == BuildResult.Failed)
            {
                throw BuildException.FromFormat(FAILED_BUILD_LOG_FORMAT,
                    report.summary.totalErrors, report.summary.totalTime);
            }
        }

        private sealed class AssetOnlyBuildLogFilter : ILogFilter
        {
	        public bool IsIgnoredInfo(string? message) => false;

	        public bool IsIgnoredWarning(string? message) => false;

	        public bool IsIgnoredAssert(string? message) => false;

	        public bool IsIgnoredError(string? message)
	        {
		        return message == null ||
		               message.Contains("Player content has not been built. Aborting build until content is built. This can be done from the Addressables window in the Build->Build Player Content menu command."); //ContentBuiltCheck.ERROR_MESSAGE;
	        }

	        public bool IsIgnoredException(Exception? e)
	        {
		        return false;
	        }

	        public bool IsWarningError(string formatted)
	        {
		        return false;
	        }
        }
    }
}
#nullable disable