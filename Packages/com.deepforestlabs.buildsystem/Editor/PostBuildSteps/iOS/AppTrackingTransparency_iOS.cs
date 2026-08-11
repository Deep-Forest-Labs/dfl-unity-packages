#if UNITY_IOS

using System;
using System.IO;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class AppTrackingTransparency_iOS : IPostprocessBuildWithReport
{
	// Set the IDFA request description:
    const string TRACKING_DESCRIPTION = "Your data will be used to provide you a personalized AD experience.";

    public int callbackOrder => (int)PostBuildOrder.AppTrackingTransparency;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string plistPath = report.summary.outputPath + "/Info.plist";
			BuildLog.Info("AppTrackingTransparency - Postprocessing \"{0}\" for target iOS", plistPath);

			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			PlistElementDict rootDict = plist.root;
			rootDict.SetString("NSUserTrackingUsageDescription", TRACKING_DESCRIPTION);

			File.WriteAllText(plistPath, plist.WriteToString());

			string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject project = new PBXProject();
			project.ReadFromFile(projectPath);
#if UNITY_2019_3_OR_NEWER
			string target = project.GetUnityMainTargetGuid();
			string frameworkTarget = project.GetUnityFrameworkTargetGuid();
#else
			string target = project.TargetGuidByName("Unity-iPhone");
			string frameworkTarget = target;
#endif
			project.AddFrameworkToProject(target, "AppTrackingTransparency.framework", false);
			project.AddFrameworkToProject(frameworkTarget, "AppTrackingTransparency.framework", false);
			project.WriteToFile(projectPath);
		}
	}
}
#endif
