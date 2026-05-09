#if UNITY_IOS

using System;
using DeepForestLabs.BuildSystems.PostBuildSteps;
using DeepForestLabs.Logger;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class LaunchStoryboard_iOS : IPostprocessBuildWithReport
{
	private const string XCODE_IMAGES_FOLDER = "Unity-iPhone/Images.xcassets";
	// TODO: Replace with your project's launch image asset name
	private const string SOURCE_FOLDER_NAME = "AppLogo.imageset";
	private const string SOURCE_FOLDER_ROOT = "Assets/LaunchImages/iOS/LaunchStoryboard/Editor";

	public int callbackOrder => (int)PostBuildOrder.LaunchStoryBoard;

	public void OnPostprocessBuild(BuildReport report)
	{
		if (report.summary.platform == BuildTarget.iOS)
		{
			string pathToBuiltProject = report.summary.outputPath;

			string srcPath = ZString.Format("{0}/{1}", SOURCE_FOLDER_ROOT, SOURCE_FOLDER_NAME);
			string dstPath = ZString.Format("{0}/{1}/{2}", pathToBuiltProject, XCODE_IMAGES_FOLDER, SOURCE_FOLDER_NAME);

			BuildLog.Info("PostBuildLaunchStoryboard - Postprocessing {0} for target iOS", srcPath);

			FileUtil.DeleteFileOrDirectory(dstPath);
			FileUtil.CopyFileOrDirectory(srcPath, dstPath);
		}
	}
}
#endif
