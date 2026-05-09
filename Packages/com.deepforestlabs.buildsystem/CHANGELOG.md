# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
 - Added support for content update builds
 - Move PlayerPref build args to be scriptable object that are created on build.
 - Cleanup fo various build paths to ensure local and jenkins builds work the same. 

## [1.0.41] - 2023-01-16
 - Added a DisableBitcode iOS post process build step.
 - Removed `compileBitcode` from the `CreateExportOptions_iOS` post process build step.

## [1.0.40] - 2022-11-04
 - Change Max Concurrent WebRequests to 10.

## [1.0.39] - 2022-10-26
 - Change the way version number is written to INI file

## [1.0.38] - 2022-09-30
 - Enabled Android create symbols file.

## [1.0.37] - 2022-09-29
 - Add Post Build Step to setup iOS App Tracking Transparency

## [1.0.36] - 2022-08-15
 - Set flag to output the build layout report.
 - Set flag to not build addressables with player build.
 - Added code to copy the build layout report to the Builds dir.

## [1.0.35] - 2022-08-11
 - Clear out the static cached Values before starting a build

## [1.0.34] - 2022-08-11
 - Add more logging during the build

## [1.0.33] - 2022-07-20
 - Rebuilding asset groups is now optional during a build.
 - Updated addressables Unity Editor menu options.

## [1.0.32] - 2022-07-08
 - Added support for PAD.

## [1.0.31] - 2022-06-01
 - Added support to send postback copies to AppsFlyer

## [1.0.30] - 2022-05-19
 - Removed `cdnUrl` since it's no longer used

## [1.0.29] - 2022-05-03
 - Fixed save android manifest logic.

## [1.0.28] - 2022-05-03
 - Fixed some logs and split APK rename logic.

## [1.0.27] - 2022-05-03
 - Fixed build path error when building a Android app bundle.

## [1.0.26] - 2022-05-02
 - Fixed build error when building a Android app bundle.

## [1.0.25] - 2022-04-15
 - Removed Firebase and Teak copy prebuild steps.

## [1.0.24] - 2022-04-12
 - Don't write out null or empty strings to INI file or PlayerPrefs
 - Return default value if INI or PlayerPrefs "value" is null or empty string

## [1.0.23] - 2022-04-12
 - Added `ServerComsKey` and `UserServiceKey` to build settings
 - Cleaned up the `AbstractBuildSettings` class

## [1.0.22] - 2022-03-29
 - Accidentally left a !UNITY_IOS instead of UNITY_IOS

## [1.0.21] - 2022-02-11
 - Added `compileBitcode` to the `CreateExportOptions_iOS` post build step

## [1.0.20] - 2022-01-31
 - Removed check for existing addressables settings.

## [1.0.19] - 2022-01-26
 - Increased the size of the Runtime Selector dialog

## [1.0.18] - 2021-11-15
 - Initialize Addressable Settings earlier in the build steps so that the scenes are set before being read by the build system.

## [1.0.17] - 2021-11-12
 - Added progress bars to the addressables processes.

## [1.0.16] - 2021-11-12
 - Added more automation of the setup of addressable settings and dependency settings.

## [1.0.15] - 2021-11-10
 - Changed how the addressables settings are setup so that it work for a builds and editor work.

## [1.0.14] - 2021-11-09
 - Removed the on start Unity auto-generate addressables settings code since it cause issues with the command line build system.

## [1.0.13] - 2021-11-09
 - Added better error handling for the addressables builder.
 - Added more AssetDatabase Refresh and SaveAssets calls to make sure the AssetDatabase is up-to-date.

## [1.0.12] - 2021-11-08
 - Added scenes in build list to the addressables builder settings.

## [1.0.11] - 2021-11-08
 - Auto-generate all needed settings for addressables.

## [1.0.10] - 2021-11-01
 - Fixed log message.

## [1.0.9] - 2021-11-01
 - Removed addressables load path.

## [1.0.8] - 2021-10-26
 - Better Addressables build clean up.
 - Addressables groups tools.
 - Addressables groups build pipeline.

## [1.0.7] - 2021-09-15
 - Added IGAPI URL and IPAPI URL

## [1.0.6] - 2021-05-03
 - Fixed a bug with `BuilderUtils.AndroidAPKName` in `ClientBuilder.cs`

## [1.0.5] - 2021-05-13
 - changed the baseAPKName pattern from `{result}_{environment.ToLower()}_{buildNumber}.apk` to `{result}_{uniqueId}_{buildNumber}_{environment.ToLower()}.apk`

## [1.0.4] - 2021-05-06
 - Added API URL

## [1.0.3] - 2021-04-19
 - Added support for split apk files.

## [1.0.2] - 2021-04-05
 - Fixes to the .ASMDEF file for `Editor` folder

## [1.0.1] - 2021-04-05
 - Fix missing `CdnUrl` from the Environment Selector

## [1.0.0] - 2021-03-29
 - First Version
