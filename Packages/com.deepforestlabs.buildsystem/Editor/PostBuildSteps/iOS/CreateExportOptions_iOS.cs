#if UNITY_IOS

using System;
using System.IO;
using DeepForestLabs.BuildSystems;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class CreateExportOptions_iOS : IPostprocessBuildWithReport
{
	public int callbackOrder => (int)PostBuildOrder.CreateExportOptions;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			bool forAppStore = BuildSettings.Instance.IsTestFlightBuild;

			string plistPath = ZString.Format("{0}/export_options.plist", report.summary.outputPath);
			BuildLog.Info("PostBuildCreateExportOptions - Postprocessing {0}. TestFlight: {1} for target iOS", plistPath, forAppStore);

			PlistDocument plist = new PlistDocument();
			PlistElementDict rootDict = plist.root;

			rootDict.SetString("teamID", PlayerSettings.iOS.appleDeveloperTeamID);
			rootDict.SetString("method", forAppStore ? "app-store-connect" : "development");
			rootDict.SetString("destination", forAppStore ? "upload" : "export");

			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif
