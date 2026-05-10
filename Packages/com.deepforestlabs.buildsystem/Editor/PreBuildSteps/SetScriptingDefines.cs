using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
	public class SetScriptingDefines : IPreprocessBuildWithReport
	{
		public int callbackOrder => (int)PreBuildOrder.SetScriptingDefines;

		public void OnPreprocessBuild(BuildReport report)
		{
			NamedBuildTarget buildTarget = report.summary.platform switch
			{
				BuildTarget.iOS => NamedBuildTarget.iOS,
				BuildTarget.Android => NamedBuildTarget.Android,
				BuildTarget.StandaloneWindows64 => NamedBuildTarget.Standalone,
				BuildTarget.StandaloneWindows => NamedBuildTarget.Standalone,
				BuildTarget.StandaloneOSX => NamedBuildTarget.Standalone,
				BuildTarget.StandaloneLinux64 => NamedBuildTarget.Standalone,
				BuildTarget.WebGL => NamedBuildTarget.WebGL,
				_ => throw new BuildFailedException($"Unsupported platform: {report.summary.platform}")
			};

			BuildSettings buildSettings = BuildSettings.Instance;
			string currentDefines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
			if (!string.IsNullOrEmpty(buildSettings.ScriptingDefines))
			{
				currentDefines = string.Format("{0}; {1}", currentDefines, buildSettings.ScriptingDefines);
			}

			if (buildSettings.IsReleaseBuild)
			{
				if (currentDefines.Contains("NOT_RELEASE_BUILD"))
				{
					currentDefines = currentDefines.Replace("NOT_RELEASE_BUILD", "RELEASE_BUILD");
				}
				else
				{
					currentDefines += "; RELEASE_BUILD";	
				}
			}
			BuildLog.Info("PreBuild:ScriptingDefines - for target {0} : '{1}'", report.summary.platform, currentDefines);
			PlayerSettings.SetScriptingDefineSymbols(buildTarget, currentDefines);
		}
	}
}
