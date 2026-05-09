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

public class DisableBitcode_iOS : IPostprocessBuildWithReport
{
	public int callbackOrder => (int)PostBuildOrder.DisableBitCode;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
			BuildLog.Info("PostBuildDisableBitcode - Postprocessing \"{0}\" for target iOS", projectPath);

			PBXProject pbxProject = new PBXProject();
			pbxProject.ReadFromFile(projectPath);

			//Disabling Bitcode on all targets

			//Main
			string target = pbxProject.GetUnityMainTargetGuid();
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

			//Unity Tests
			target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

			//Unity Framework
			target = pbxProject.GetUnityFrameworkTargetGuid();
			pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

			pbxProject.WriteToFile(projectPath);
		}
	}
}
#endif
