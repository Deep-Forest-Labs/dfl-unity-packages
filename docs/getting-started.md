# Getting Started

This guide walks you through setting up a new Unity project with the DFL packages and wiring your first service through the dependency injection container.

## Prerequisites

- **Unity 2022.3 LTS** or later
- **GitHub access** to the [dfl-unity-packages](https://github.com/Deep-Forest-Labs/dfl-unity-packages) repo (private)
- **Git credentials configured** -- run `gh auth login` or ensure Git Credential Manager is set up
- **OpenUPM scoped registry** for UniTask and ZString (see [Installation](#1-add-packages))

## The Fast Path: dfl-unity-template

The quickest way to start a new project is to clone [dfl-unity-template](https://github.com/Deep-Forest-Labs/dfl-unity-template). It comes pre-configured with all DFL packages, a `MainArgs` asset, an `AppContainerFactory`, and the OpenUPM registry.

If you're adding packages to an existing project, follow the manual steps below.

## Manual Setup

### 1. Add Packages

Add the OpenUPM registry and DFL packages to your `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.cysharp"]
    }
  ],
  "dependencies": {
    "com.cysharp.unitask": "2.5.10",
    "com.cysharp.zstring": "2.6.0",
    "com.deepforestlabs.audio": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.audio#v1.0.0",
    "com.deepforestlabs.buildsystem": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.buildsystem#v1.0.0",
    "com.deepforestlabs.framework": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.framework#v1.0.0",
    "com.deepforestlabs.logger": "https://github.com/Deep-Forest-Labs/dfl-unity-packages.git?path=Packages/com.deepforestlabs.logger#v1.0.0"
  }
}
```

> **Local package development:** If you need to edit the packages themselves, temporarily replace a Git URL with a `file:` path pointing to your local clone (e.g. `"file:../../dfl-unity-packages/Packages/com.deepforestlabs.framework"`). Do not commit `file:` paths to shared branches.

### 2. Create Build Settings

The framework expects a `BuildSettings` asset at `Assets/Resources/BuildSettings.asset` and a `BuildSystemSettings` asset at `Assets/Editor/BuildSystemSettings.asset`. These are typically auto-created by the build system editor scripts on first load.

### 3. Create MainArgs

Create a subclass of `MainArgs` (a `ContainerBuilderFactory` ScriptableObject) and place it at `Assets/Resources/MainArgs.asset`. This is the root of your DI configuration:

```csharp
using DeepForestLabs;
using DeepForestLabs.Factories;
using DeepForestLabs.Services;
using UnityEngine;

[CreateAssetMenu(fileName = "MainArgs", menuName = "App/MainArgs")]
public sealed class AppMainArgs : MainArgs
{
    [SerializeField] private ScriptableObjectAssetRefT<ContainerBuilderFactory> _appFactory = default!;

    public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
    {
        return base.AddToBuilder(builder)
            .AddScoped<IErrorReporter, NullErrorReporter>();
    }
}
```

`MainArgs.AddToBuilder` in the base class already wires:
- `LogFilter` (via a `[SerializeField]` asset reference)
- Your app's `ContainerBuilderFactory` (via a `[SerializeField]` asset reference)
- `AudioListener` component
- `LoggingService`
- `UnobservedExceptionState`

### 4. Create Your App Container Factory

This is where you register your game's services. Create a `ContainerBuilderFactory` ScriptableObject:

```csharp
using DeepForestLabs;
using DeepForestLabs.Factories;

[CreateAssetMenu(fileName = "AppContainerFactory", menuName = "App/Container Factory")]
public sealed class AppContainerFactory : ContainerBuilderFactory
{
    public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
    {
        return builder
            .AddScoped<IGameService, GameService>()
            .AddTransient<GameState>();
    }
}
```

Create the asset in `Assets/Resources/` and assign it in the MainArgs inspector.

### 5. Write Your First Service

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs;
using DeepForestLabs.Logger;

public interface IGameService
{
    string PlayerName { get; }
}

public sealed class GameService : IGameService, IInitializable
{
    // Automatically injected by the container
    [Dependency] private readonly IContainer _container = null!;

    public string PlayerName { get; private set; } = "Unknown";

    public async UniTask Initialize(CancellationToken token)
    {
        Log.Info("GameService initializing...");
        PlayerName = "Player 1";
        Log.Info("GameService ready: {0}", PlayerName);
    }
}
```

### 6. Run

Press Play in Unity. The framework's `Main` class automatically:

1. Loads `MainArgs` from `Resources/MainArgs`
2. Builds the root DI container
3. Injects all `[Dependency]` fields
4. Calls `Initialize` on every `IInitializable` service
5. Runs `MainState` (which drives your app lifecycle)

You should see log output confirming your service initialized.

## What's Next

- [Architecture Overview](architecture.md) -- understand the package structure and lifecycle
- [Dependency Injection](dependency-injection.md) -- full registration API and patterns
- [Asset Loading](asset-loading.md) -- loading prefabs, audio clips, and other assets through the container
- [Audio Service](audio-service.md) -- setting up BGM, SFX, and sound catalogs
