# Deep Forest Labs Unity Packages

A monorepo hosting the core UPM packages for Deep Forest Labs Unity projects.

## Packages

| Package | Path | Description |
|---------|------|-------------|
| `com.deepforestlabs.logger` | `Packages/com.deepforestlabs.logger` | Structured logging with filtering and build-time log stripping |
| `com.deepforestlabs.buildsystem` | `Packages/com.deepforestlabs.buildsystem` | Multi-platform build pipeline (Android, iOS, Windows, WebGL), environment configuration, and Addressables integration |
| `com.deepforestlabs.framework` | `Packages/com.deepforestlabs.framework` | Core application framework — DI container, MVC, async lifecycle, Addressables management, error reporting abstraction |

## Supported Platforms

- **Android** — full build pipeline with APK/AAB support
- **iOS** — full build pipeline with Xcode project generation
- **Windows Standalone** — IL2CPP builds with `.exe` output
- **WebGL** — IL2CPP builds with configurable compression

### Adding a new platform

Implement `IPlatformBuildSetup` in `Packages/com.deepforestlabs.buildsystem/Editor/PlatformSetup/` and register it in `PlatformBuildSetupResolver.Resolve()`. The interface handles:
- Project settings configuration
- Output path generation
- Build number management
- Default platform arguments

## Installation

Add these packages to your Unity project's `Packages/manifest.json`. For local development, use `file:` paths:

```json
{
  "dependencies": {
    "com.deepforestlabs.framework": "file:../../dfl-unity-packages/Packages/com.deepforestlabs.framework",
    "com.deepforestlabs.logger": "file:../../dfl-unity-packages/Packages/com.deepforestlabs.logger",
    "com.deepforestlabs.buildsystem": "file:../../dfl-unity-packages/Packages/com.deepforestlabs.buildsystem"
  }
}
```

For versioned releases, use Git URLs pinned to a tag:

```json
{
  "dependencies": {
    "com.deepforestlabs.framework": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.framework#v1.0.0"
  }
}
```

## Key Abstractions

### IPlatformBuildSetup

Strategy interface for platform-specific build configuration. Each supported platform has an implementation in `Editor/PlatformSetup/`. The resolver (`PlatformBuildSetupResolver`) selects the correct implementation based on the active build target.

### IErrorReporter

Abstraction over crash/error reporting backends. The default `SentryErrorReporter` wraps the Sentry SDK. Projects can register `NullErrorReporter` for platforms where Sentry is unavailable or undesired. Register the desired implementation in your project's `MainArgs.AddToBuilder()`.

### AssetLoadStrategy

Per-project configuration on `AddressablesBuildSettings` that controls whether content bundles are loaded from a remote CDN (`RemoteCDN`) or shipped locally with the build (`LocalBundles`). WebGL projects may prefer `LocalBundles` to avoid re-downloading each session.

## Dependencies

These packages depend on third-party UPM packages that are **not** on the Unity registry. Consuming projects must add the OpenUPM scoped registry and the package references to their `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp"
      ]
    }
  ],
  "dependencies": {
    "com.cysharp.unitask": "2.5.10",
    "com.cysharp.zstring": "2.6.0"
  }
}
```

The following dependencies resolve automatically from the Unity registry:

- `com.unity.addressables`
- `com.unity.nuget.newtonsoft-json`
- `io.sentry.unity`

## License

Copyright © 2024 Deep Forest Labs. All rights reserved.
