# Deep Forest Labs Unity Packages

A monorepo hosting the core UPM packages for Deep Forest Labs Unity projects.

## Packages

| Package | Path | Description |
|---------|------|-------------|
| `com.deepforestlabs.logger` | `Packages/com.deepforestlabs.logger` | Structured logging with filtering and build-time log stripping |
| `com.deepforestlabs.buildsystem` | `Packages/com.deepforestlabs.buildsystem` | Multi-platform build pipeline (Android, iOS, Windows, WebGL), environment configuration, and Addressables integration |
| `com.deepforestlabs.framework` | `Packages/com.deepforestlabs.framework` | Core application framework -- DI container, MVC, async lifecycle, Addressables management, error reporting abstraction |
| `com.deepforestlabs.audio` | `Packages/com.deepforestlabs.audio` | Audio service -- pooled AudioSource instances, AudioMixer integration, sound catalogs, ducking, DI lifecycle |

## Documentation

Detailed guides live in the [`docs/`](docs/) folder:

| Guide | What it covers |
|-------|----------------|
| [Getting Started](docs/getting-started.md) | First-time setup, creating a project, wiring your first service |
| [Architecture](docs/architecture.md) | Package dependency graph, design philosophy, lifecycle sequence |
| [Dependency Injection](docs/dependency-injection.md) | Registration API, `[Dependency]` injection, scopes, factories, lifecycle |
| [Asset Loading](docs/asset-loading.md) | `AssetRef` types, `Checkout`/`Download`, Addressables configuration |
| [Audio Service](docs/audio-service.md) | Playing sounds, mixer routing, ducking, catalogs, preloading |
| [Build System](docs/build-system.md) | Build pipeline, platform setup, environments, CI integration |
| [Logging](docs/logging.md) | `Log` API, levels, build-time stripping, filtering |
| [Testing](docs/testing.md) | Test structure, NUnit patterns, mocking the DI container |

Each package also has its own README with a quick-reference summary.

## Supported Platforms

- **Android** -- full build pipeline with APK/AAB support
- **iOS** -- full build pipeline with Xcode project generation
- **Windows Standalone** -- IL2CPP builds with `.exe` output
- **WebGL** -- IL2CPP builds with configurable compression

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
    "com.deepforestlabs.buildsystem": "file:../../dfl-unity-packages/Packages/com.deepforestlabs.buildsystem",
    "com.deepforestlabs.audio": "file:../../dfl-unity-packages/Packages/com.deepforestlabs.audio"
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

Or use [dfl-unity-template](https://github.com/Deep-Forest-Labs/dfl-unity-template) which comes pre-configured with all packages.

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
    "com.cysharp.zstring": "2.6.0",
    "com.cysharp.zlinq": "1.5.5"
  }
}
```

### ZLinq Drop-in Generator (recommended for game code)

The framework provides `com.cysharp.zlinq` as a transitive dependency. To automatically optimize all LINQ operations on arrays and lists in your **game code** to zero-allocation:

1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) (available on OpenUPM as `com.github-glitchenzo.nugetforunity`)
2. Open **NuGet > Manage NuGet Packages** and install `ZLinq` and `ZLinq.DropInGenerator`
3. Add an `AssemblyInfo.cs` to your game assembly with:

```csharp
using ZLinq;
[assembly: ZLinqDropIn("", DropInGenerateTypes.Array | DropInGenerateTypes.List)]
```

The framework packages themselves use standard System.Linq internally. Game code written with the drop-in active should use `using ZLinq;` in files with chained LINQ operators and avoid returning `ValueEnumerable` where `IEnumerable<T>` is expected.

### ZLinq.Unity (optional)

For zero-allocation GameObject/Transform traversal (`Descendants()`, `Children()`, `Ancestors()`, etc.), add the Unity extensions package:

```json
{
  "dependencies": {
    "com.cysharp.zlinq.unity": "https://github.com/Cysharp/ZLinq.git?path=src/ZLinq.Unity/Assets/ZLinq.Unity"
  }
}
```

The following dependencies resolve automatically from the Unity registry:

- `com.unity.addressables`
- `com.unity.nuget.newtonsoft-json`
- `io.sentry.unity`

## License

Copyright (c) 2024 Deep Forest Labs. All rights reserved.
