using System;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Set the build number X.Y.Z (build)
namespace DeepForestLabs.BuildSystems.PreBuildSteps
{
	public class SetBuildNumber : IPreprocessBuildWithReport
	{
		public int callbackOrder => (int)PreBuildOrder.SetBuildNumber;

		public void OnPreprocessBuild(BuildReport report)
		{
			BuildSettings buildSettings = BuildSettings.Instance;
			BuildLog.Info("PreBuild:SetBuildNumber - Setting build number to {0} for target {1}", buildSettings.BuildNumber, report.summary.platform);

			switch (report.summary.platform)
			{
				case BuildTarget.iOS:
					PlayerSettings.iOS.buildNumber = buildSettings.BuildNumber.ToString();
					break;

				case BuildTarget.Android:
					PlayerSettings.Android.bundleVersionCode = buildSettings.BuildNumber;
					break;
			}
		}
	}
}
