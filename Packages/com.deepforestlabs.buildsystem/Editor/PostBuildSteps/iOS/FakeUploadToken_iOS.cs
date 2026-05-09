#if UNITY_IOS

using System;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class FakeUploadToken_iOS : IPostprocessBuildWithReport
{
	public int callbackOrder => (int)PostBuildOrder.FakeUploadToken;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string pathToBuiltProject = report.summary.outputPath;

			string pbxFilename = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
			BuildLog.Info("PostBuildFakeUploadToken - Postprocessing \"{0}\" for target iOS", pbxFilename);

			PBXProject project = new PBXProject();
			project.ReadFromFile(pbxFilename);

			string targetGUID = project.GetUnityMainTargetGuid();

			string token = project.GetBuildPropertyForAnyConfig(targetGUID, "USYM_UPLOAD_AUTH_TOKEN");
			if (string.IsNullOrEmpty(token))
			{
				token = "FakeToken";
			}
			project.SetBuildProperty(targetGUID, "USYM_UPLOAD_AUTH_TOKEN", token);

			project.WriteToFile(pbxFilename);
		}
	}
}
#endif
