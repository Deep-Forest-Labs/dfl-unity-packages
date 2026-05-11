# Deep Forest Labs Logger

A zero-allocation logging facade built on [ZString](https://github.com/Cysharp/ZString) that wraps Unity's `Debug` API with frame-tagged output, compile-time log stripping, and color-coded Editor prefixes.

## Usage

```csharp
using DeepForestLabs.Logger;

// Always available
Log.Info("Player connected");
Log.Info("Score: {0}, Level: {1}", score, level);
Log.Warning("Retry attempt {0}", attempt);
Log.Error("Failed to load asset: {0}", assetPath);
Log.Exception(ex);
Log.Assert(health > 0, "Health must be positive, got {0}", health);

// Stripped from release builds (zero cost)
Log.Debug("Frame timing: {0}ms", deltaMs);
Log.DebugWarning("Shader fallback active");

// Editor-only
Log.Editor("Inspector refreshed");

// Errors in dev, downgraded to warnings in release
Log.DevError("Unexpected state: {0}", state);
Log.DevException(ex);
```

## Log Categories

| Method | Output | Stripped in release? |
|--------|--------|---------------------|
| `Log.Info` | `Debug.Log` | No |
| `Log.Warning` | `Debug.LogWarning` | No |
| `Log.Error` | `Debug.LogError` | No |
| `Log.Exception` | `Debug.LogException` | No |
| `Log.Assert` | `Debug.Assert` + throws `AssertionException` | Yes (unless `RELEASE_WITH_ASSERTS_ENABLED`) |
| `Log.Debug` | `Debug.Log` with colored `DEBUG` prefix | Yes (unless `RELEASE_WITH_DEBUG_LOGS`) |
| `Log.DebugWarning` | `Debug.LogWarning` with prefix | Yes |
| `Log.DebugError` | `Debug.LogError` with prefix | Yes |
| `Log.Editor` | `Debug.Log` | Yes (Editor-only via `UNITY_EDITOR`) |
| `Log.DevError` | `Debug.LogError` in dev, `Debug.LogWarning` in release | Downgraded, not stripped |
| `Log.Validate` | `Debug.Assert` with `[Asset Validation Failure]` prefix | Yes |

## Scripting Symbols

The build system (`com.deepforestlabs.buildsystem`) manages these defines automatically:

| Symbol | Effect |
|--------|--------|
| `RELEASE_BUILD` | Strips `Debug`, `Assert`, `Validate`, `Editor` methods |
| `NOT_RELEASE_BUILD` | Enables all log methods (default for development) |
| `RELEASE_WITH_DEBUG_LOGS` | Keeps `Debug` methods in a release build |
| `RELEASE_WITH_ASSERTS_ENABLED` | Keeps `Assert` in a release build |

## Frame Tagging

Every log line is prefixed with `[frameCount]` for correlating logs with Unity's frame timeline:

```
[142] Player connected
[142] DEBUG: Frame timing: 16ms
[143] Retry attempt 2
```

## Filtering

`ILogFilter` and its default `LogFilter` ScriptableObject let projects ignore or downgrade specific log categories. Filtering is applied by `LoggingService` in the framework package (which replaces Unity's default log handler), not by the `Log` class itself.

## Key Types

| Type | Description |
|------|-------------|
| `Log` | Static partial class -- the main logging API |
| `ILogFilter` | Interface for filtering/downgrading log categories |
| `LogFilter` | Default ScriptableObject filter (all pass-through) |
| `GameException` | `Exception` subclass with ZString format factories |
| `BuildLog` | Editor-only build-pipeline logging (stripped outside Editor) |

## Dependencies

- `com.cysharp.zstring` -- zero-allocation string formatting
