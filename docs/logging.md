# Logging

The `com.deepforestlabs.logger` package provides a static logging API built on [ZString](https://github.com/Cysharp/ZString) for zero-allocation string formatting. It wraps Unity's `Debug` API with frame-tagged output, compile-time log stripping for release builds, and color-coded prefixes in the Editor.

## API Reference

### Always Available

These methods are never stripped -- they exist in all build configurations:

```csharp
using DeepForestLabs.Logger;

Log.Info("Server connected");
Log.Info("Player {0} joined room {1}", playerName, roomId);

Log.Warning("Retry attempt {0} of {1}", attempt, maxRetries);

Log.Error("Failed to load config: {0}", path);

Log.Exception(caughtException);
Log.Exception("Context message", caughtException);
```

### Debug (Stripped in Release)

Removed entirely from release builds via `[Conditional]` attributes -- zero runtime cost:

```csharp
Log.Debug("Frame delta: {0}ms", deltaMs);
Log.DebugWarning("Shader fallback active for {0}", materialName);
Log.DebugError("Unexpected state in {0}", stateMachine);
Log.DebugException(ex);
```

In the Editor, debug messages display with a colored `DEBUG` prefix for visual distinction.

### Editor-Only

Compiled only under `UNITY_EDITOR`:

```csharp
Log.Editor("Inspector refreshed for {0}", target);
Log.EditorWarning("Missing reference on {0}", gameObject);
Log.EditorError("Validation failed: {0}", message);
Log.EditorException(ex);
```

### DevError / DevException

Full error in development builds, downgraded to warnings in release:

```csharp
Log.DevError("Unexpected state: {0}", state);
Log.DevException(ex);
```

This is useful for errors that shouldn't crash release builds but should be loud during development.

### Assert

Checks a condition and throws `AssertionException` if it fails:

```csharp
Log.Assert(health > 0, "Health must be positive, got {0}", health);
Log.Assert(config != null, "Config is null");
```

Stripped in release unless `RELEASE_WITH_ASSERTS_ENABLED` is defined.

### Validate

Asset validation checks (stripped outside Editor and release):

```csharp
Log.Validate(sprite != null, "[Asset Validation Failure] Missing sprite on {0}", name);
```

## Format Strings

All logging methods use ZString for zero-allocation formatting. Use `{0}`, `{1}`, etc. placeholders with up to 5 generic arguments:

```csharp
Log.Info("Score: {0}", score);
Log.Info("Player {0} at position ({1}, {2})", name, x, y);
Log.Debug("{0} loaded in {1}ms with {2} assets", bundle, elapsed, count);
```

Each overload is generic (`T1`, `T2`, etc.) so value types are not boxed.

## Frame Tagging

Every log line is automatically prefixed with `[frameCount]`:

```
[142] Server connected
[142] DEBUG: Frame delta: 16ms
[143] Retry attempt 2 of 5
```

This makes it easy to correlate log messages with specific frames when debugging timing issues.

## Context Object

All methods have an overload that accepts a `UnityEngine.Object` context parameter. Clicking the log in the console will ping the object:

```csharp
Log.Info(gameObject, "Spawned enemy at {0}", position);
Log.Warning(this, "Health below threshold: {0}", health);
```

## Scripting Symbols

The build system manages these defines automatically, but you can set them manually for testing:

| Symbol | What It Does |
|--------|-------------|
| `RELEASE_BUILD` | Strips `Debug*`, `Assert`, `Validate`, `Editor*` methods. `DevError`/`DevException` downgrade to warnings. |
| `NOT_RELEASE_BUILD` | All methods active (default for development) |
| `RELEASE_WITH_DEBUG_LOGS` | Keeps `Debug*` methods in a release build |
| `RELEASE_WITH_ASSERTS_ENABLED` | Keeps `Assert` in a release build |

The build system's `SetScriptingDefines` pre-build step swaps `RELEASE_BUILD` / `NOT_RELEASE_BUILD` based on whether `-releaseBuild` is passed.

## Filtering

### ILogFilter

The `ILogFilter` interface allows projects to ignore or downgrade specific log categories:

```csharp
public interface ILogFilter
{
    bool IsIgnoredInfo { get; }
    bool IsIgnoredWarning { get; }
    bool IsIgnoredAssert { get; }
    bool IsIgnoredError { get; }
    bool IsIgnoredException(Exception exception);
    bool IsWarningError { get; }
}
```

### LogFilter ScriptableObject

The default implementation (`LogFilter`) passes everything through. Projects can subclass it or create custom `ILogFilter` implementations for:
- Suppressing noisy log categories in production
- Downgrading errors to warnings for known benign issues
- Filtering specific exception types from error reporting

### How Filtering Connects

Filtering is **not** applied inside the `Log` class itself. Instead, the framework's `LoggingService` (registered via `MainArgs`) replaces Unity's default `ILogHandler` and consults the registered `ILogFilter` before forwarding logs. This means filtering only activates after the DI container is built and `LoggingService` initializes.

## GameException

A custom `Exception` subclass with ZString-based format factories:

```csharp
throw GameException.FromFormat("Invalid state: {0} for player {1}", state, playerId);
throw GameException.FromFormat(innerException, "Load failed for {0}", assetPath);
```

## BuildLog (Editor)

`BuildLog` provides prefixed logging for build pipeline scripts. All methods are `[Conditional("UNITY_EDITOR")]`:

```csharp
BuildLog.Info("Starting content build");      // :: INFO :: Starting content build
BuildLog.Warning("Skipping empty group");     // :: WARNING :: Skipping empty group
BuildLog.Error("Build failed: {0}", reason);  // :: ERROR :: Build failed: ...
```
