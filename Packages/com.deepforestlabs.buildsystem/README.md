# Deep Forest Labs Build System

Multi-platform build pipeline for Unity projects. Handles Addressables content builds, player builds, environment configuration, and platform-specific post-processing for Android, iOS, Windows, and WebGL.

## Supported Platforms

| Platform | Output | Key Features |
|----------|--------|-------------|
| **Android** | APK / AAB | Gradle, split APKs, debug symbols, keystore config |
| **iOS** | Xcode project | Bitcode, ATT, SKAN, export options plist, CocoaPods |
| **Windows** | `.exe` (IL2CPP) | Standalone naming, boot.config patching |
| **WebGL** | HTML folder | Configurable compression format via platform args |

## How It Works

### Build Entry Points

Builds are triggered either from Unity's menu or via CLI batch mode:

- **Menu**: `Build > Client` (content + player), `Build > Content Only`, `Build > Client No Bundles`
- **CLI**: `-executeMethod DeepForestLabs.BuildSystems.BuildSystemEntryPoint.BuildFromCommandLine`

### CLI Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| `-buildNumber` | Yes | Build number synced to platform settings |
| `-environment` | Yes | Environment name (must match a configured environment) |
| `-uniqueId` | Yes | Addressables unique id |
| `-assetId` | Yes | Addressables asset id |
| `-debugBuild` | No | Enables debug scripting defines |
| `-releaseBuild` | No | Sets `RELEASE_BUILD` define, strips debug logs |
| `-testFlight` | No | iOS: generates TestFlight export options |
| `-buildAppBundle` | No | Android: builds AAB instead of APK |
| `-scriptingDefines` | No | Additional semicolon-separated defines |
| `-platformArgs` | No | Platform-specific `key=value,key=value` pairs |

### Build Pipeline Sequence

1. **Pre-build**: backup files, create output dirs, set build number, set scripting defines, configure keystore (Android)
2. **Content build**: Addressables `BuildPlayerContent` or `ContentUpdateScript`
3. **Player build**: `BuildPipeline.BuildPlayer` with configured `BuildPlayerOptions`
4. **Post-build**: restore files, patch `boot.config`, rename split APKs (Android), generate export options (iOS), re-enable sprite atlases

## Configuration Assets

### `BuildSettings` (Runtime)

ScriptableObject at `Assets/Resources/BuildSettings.asset`. Holds the active build metadata at runtime:

- Build target, version numbers, build number
- Active environment (name, server URL, API keys)
- Addressables settings (unique id, asset id, load strategy)
- Target FPS, VSync, screen orientation

### `BuildSystemSettings` (Editor)

ScriptableObject at `Assets/Editor/BuildSystemSettings.asset`. Project-level editor configuration:

- Application name, analytics IDs
- Debug and release `BuildOptions` presets
- Environment list (fetched from remote JSON or configured locally)
- Addressable group sorting rules

### `AddressablesBuildSettings`

Serialized block inside `BuildSettings`:

| Field | Description |
|-------|-------------|
| `AssetLoadStrategy` | `RemoteCDN` (download from CDN) or `LocalBundles` (ship with build) |
| `UniqueId` | Unique build identifier for Addressables |
| `AssetId` | Asset version identifier |
| `EnableJsonCatalog` | Use JSON catalog format |
| `ActivePlayModeIndex` | Editor play mode (asset database, local bundles, remote bundles) |

## Addressables Play Modes

Custom data builders registered in Addressables settings (order must match `BuilderIndex` enum):

| Builder | Description |
|---------|-------------|
| `AssetDatabasePlayMode` | Fast iteration -- loads directly from AssetDatabase |
| `LocalAssetBundlesPlayMode` | Packed local bundles for testing bundle behavior |
| `RemoteAssetBundlesPlayMode` | Points at remote bundles, clears download cache |
| `GroupsBuildMode` | Rebuilds Addressable groups, validates group counts, prunes empty groups |
| `AssetBundlesBuildMode` | Full packed player content build with content-update support |

## Error Reporting

The framework provides `IErrorReporter` with two implementations:

- **`SentryErrorReporter`** -- wraps the Sentry SDK (default for production)
- **`NullErrorReporter`** -- no-op for platforms where Sentry is unavailable

Register your choice in `MainArgs.AddToBuilder()` or your app's container factory.

## Adding a New Platform

1. Implement `IPlatformBuildSetup` in `Editor/PlatformSetup/`
2. Register it in `PlatformBuildSetupResolver.Resolve()` for the new `BuildTarget`
3. The interface requires: `NamedBuildTarget`, `ConfigureProjectSettings()`, `GetOutputPath()`, build number get/set, default platform args

## Dependencies

- `com.unity.addressables` -- Addressable asset system
- `com.deepforestlabs.logger` -- build and runtime logging
