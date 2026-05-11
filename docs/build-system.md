# Build System

The `com.deepforestlabs.buildsystem` package provides a multi-platform build pipeline that orchestrates Addressables content builds, Unity player builds, environment configuration, and platform-specific post-processing.

## Overview

The build system is **editor-only** -- it provides no runtime services beyond `BuildSettings` (a `ScriptableObject` loaded from Resources that exposes build metadata at runtime).

Builds can be triggered from Unity's menu or via CLI for CI/CD:

| Entry Point | Method | Description |
|-------------|--------|-------------|
| Menu: `Build > Client` | `BuildSystemEntryPoint` | Full build: content + player |
| Menu: `Build > Content Only` | `BuildSystemEntryPoint` | Addressables content only |
| Menu: `Build > Client No Bundles` | `BuildSystemEntryPoint` | Player build without content |
| CLI | `-executeMethod ...BuildFromCommandLine` | CI/CD batch mode |

## Configuration Assets

### BuildSettings (Runtime)

`Assets/Resources/BuildSettings.asset` -- available at runtime via `BuildSettings.Instance`.

| Field | Description |
|-------|-------------|
| `BuildTarget` | Active platform |
| `FullVersionNumber` | Full version string |
| `ShortVersion` | Short version (e.g. `1.0.0`) |
| `BuildNumber` | Integer build number |
| `Environment` | Active `EnvironmentBuildSettings` (name, server URL, API keys) |
| `Addressables` | `AddressablesBuildSettings` block |
| `TargetFps` | Target frame rate |
| `VSyncCount` | VSync setting |
| `Orientation` | Screen orientation |

### BuildSystemSettings (Editor)

`Assets/Editor/BuildSystemSettings.asset` -- project-level configuration for the build pipeline.

| Field | Description |
|-------|-------------|
| Application name | Displayed in the inspector header |
| Debug/Release `BuildOptions` | Presets toggled by `-debugBuild` / `-releaseBuild` |
| Environment list | Configured locally or fetched from a remote JSON URL |
| Addressable group sort rules | Controls group ordering during builds |
| Analytics IDs | App-specific analytics identifiers |

### AddressablesBuildSettings

Nested inside `BuildSettings`:

| Field | Description |
|-------|-------------|
| `AssetLoadStrategy` | `RemoteCDN` -- download from CDN; `LocalBundles` -- ship with build |
| `UniqueId` | Unique Addressables build identifier |
| `AssetId` | Asset version identifier |
| `EnableJsonCatalog` | Use JSON catalog format |
| `ActivePlayModeIndex` | Editor play mode index (asset database, local, remote) |

## Environments

Each environment is an `EnvironmentBuildSettings` with:

- Environment name
- Server URL
- Analytics key
- API endpoint
- Communications URL
- User service URL

Environments can be fetched from a remote JSON endpoint (via `EnvironmentsDownloader`) or configured manually in `BuildSystemSettings`.

## Platform Setup

Each platform implements `IPlatformBuildSetup`:

```csharp
public interface IPlatformBuildSetup
{
    NamedBuildTarget NamedBuildTarget { get; }
    void ConfigureProjectSettings(CommandLineArgs args);
    string GetOutputPath(CommandLineArgs args);
    int GetBuildNumber();
    void SetBuildNumber(int number);
    PlatformArgs GetDefaultPlatformArgs();
}
```

`PlatformBuildSetupResolver.Resolve()` maps `BuildTarget` to the correct implementation.

### Platform Details

**Android** (`AndroidBuildSetup`):
- Gradle backend, IL2CPP
- APK or AAB (`-buildAppBundle`)
- Split APKs with CPU-specific naming
- Debug symbols and keystore configuration

**iOS** (`iOSBuildSetup`):
- IL2CPP with size optimization
- Xcode project output
- Post-build: disable bitcode, encryption exemption, ATT description, SKAN endpoint, export options plist, launch storyboard, CocoaPods `use_frameworks!`

**Windows** (`StandaloneBuildSetup`):
- IL2CPP with `.exe` output
- Boot config patching (worker thread count)

**WebGL** (`WebGLBuildSetup`):
- IL2CPP
- Configurable compression format via `-platformArgs compression=gzip`

## Build Pipeline Steps

### Pre-Build (`IPreprocessBuildWithReport`)

Executed in `callbackOrder` sequence:

1. **ClientPreBuilder** -- log build info, backup files, create output directories, disable splash screen
2. **SpriteAtlasPreprocessBuild** -- disable sprite atlas "include in build"
3. **SetBuildNumber** -- sync build number to `PlayerSettings`
4. **SetScriptingDefines** -- merge scripting defines, swap `RELEASE_BUILD` / `NOT_RELEASE_BUILD`
5. **SetKeystoreInfo_Android** -- configure Android keystore
6. **SetBuildSubtarget_Android** -- set texture subtarget to Generic

### Post-Build (`IPostprocessBuildWithReport`)

1. **ClientPostBuilder** -- restore/delete files, validate `BuildReport`
2. **SpriteAtlasPostprocessBuild** -- re-enable sprite atlas "include in build"
3. **BootConfigModifier** -- patch `boot.config` (worker thread count)
4. **RenameSplitAPKFiles_Android** -- rename per-CPU APK and mapping files
5. **SaveAndroidManifest_Android** -- copy merged `AndroidManifest.xml`
6. iOS-specific: bitcode, encryption, ATT, SKAN, export options, launch storyboard, upload tokens

## Addressables Play Modes

Custom data builders for different editor workflows:

| Mode | Builder Class | Description |
|------|--------------|-------------|
| Fast | `AssetDatabasePlayMode` | Load directly from AssetDatabase (no bundles) |
| Local | `LocalAssetBundlesPlayMode` | Build and load local bundles |
| Remote | `RemoteAssetBundlesPlayMode` | Load from remote URL, clears download cache |
| Groups | `GroupsBuildMode` | Rebuild groups, run folder importer, validate counts |
| Bundles | `AssetBundlesBuildMode` | Full player content build with content-update detection |

The order of these builders in Addressables settings must match the `BuilderIndex` enum.

## CLI Arguments Reference

| Argument | Required | Description |
|----------|----------|-------------|
| `-buildNumber` | Yes | Integer build number |
| `-environment` | Yes | Environment name |
| `-uniqueId` | Yes | Addressables unique build id |
| `-assetId` | Yes | Addressables asset version id |
| `-debugBuild` | No | Enable debug defines and options |
| `-releaseBuild` | No | Enable `RELEASE_BUILD`, strip debug logs |
| `-testFlight` | No | iOS: TestFlight export options |
| `-buildAppBundle` | No | Android: AAB instead of APK |
| `-scriptingDefines` | No | Additional defines (semicolon-separated) |
| `-overrideEnvironmentUrl` | No | Override the base environment URL |
| `-platformArgs` | No | Platform-specific key=value pairs (comma-separated) |

## Error Reporting

`IErrorReporter` is the framework's abstraction over crash/error reporting:

- **`SentryErrorReporter`** -- wraps the Sentry Unity SDK for production crash reporting
- **`NullErrorReporter`** -- no-op implementation for development or unsupported platforms

Register your choice in your `MainArgs` or `ContainerBuilderFactory`:

```csharp
builder.AddScoped<IErrorReporter, SentryErrorReporter>();
// or
builder.AddScoped<IErrorReporter, NullErrorReporter>();
```

The build system's `SetScriptingDefines` step handles `RELEASE_BUILD` / `NOT_RELEASE_BUILD` defines that control log stripping in the logger package.
