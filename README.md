# Deep Forest Labs Unity Packages

A monorepo hosting the core UPM packages for Deep Forest Labs Unity projects.

## Packages

| Package | Path | Description |
|---------|------|-------------|
| `com.deepforestlabs.logger` | `Packages/com.deepforestlabs.logger` | Structured logging with filtering and build-time log stripping |
| `com.deepforestlabs.buildsystem` | `Packages/com.deepforestlabs.buildsystem` | Build pipeline, environment configuration, and Addressables integration |
| `com.deepforestlabs.framework` | `Packages/com.deepforestlabs.framework` | Core application framework — DI container, MVC, async lifecycle, Addressables management, Sentry integration |

## Installation

Add these packages to your Unity project's `Packages/manifest.json` using Git URLs with the `?path=` parameter:

```json
{
  "dependencies": {
    "com.deepforestlabs.framework": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.framework#v1.0.0",
    "com.deepforestlabs.logger": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.logger#v1.0.0",
    "com.deepforestlabs.buildsystem": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.buildsystem#v1.0.0"
  }
}
```

Pin to a specific tag (e.g. `#v1.0.0`) for reproducible builds.

## Dependencies

These packages require the following UPM dependencies (resolved automatically):

- `com.unity.addressables`
- `com.unity.nuget.newtonsoft-json`
- `io.sentry.unity`
- `com.cysharp.unitask`

## License

Copyright © 2024 Deep Forest Labs. All rights reserved.
