# Deep Forest Labs Framework

The core application framework for DFL Unity projects. Provides dependency injection, async service lifecycle, Addressables-based asset loading, an MVC stack, error reporting, and connectivity services.

This is the foundational package -- every other DFL package depends on it.

## Quick Start

```csharp
// 1. Create a ContainerBuilderFactory ScriptableObject to register your services
public sealed class AppContainerFactory : ContainerBuilderFactory
{
    public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
    {
        return builder
            .AddScoped<IMyService, MyService>()
            .AddTransient<GameState>();
    }
}

// 2. Services declare dependencies via the [Dependency] attribute
public sealed class MyService : IMyService, IInitializable
{
    [Dependency] private readonly IContainer _container = null!;

    public async UniTask Initialize(CancellationToken token)
    {
        // Called automatically after injection
    }
}
```

See [Dependency Injection Guide](../../docs/dependency-injection.md) for the full API.

## Key Types

### Dependency Injection

| Type | Description |
|------|-------------|
| `IContainerBuilder` | Fluent API for registering singletons, scoped services, and transients |
| `IContainer` | Built container -- resolves dependencies, manages asset loading, owns child scopes |
| `IDiCollection` | Read-only service resolution: `Get<T>()`, `Find<T>()`, `TryGet<T>()` |
| `[Dependency]` | Attribute marking fields/properties for automatic injection |
| `IInitializable` | Async init hook called after all dependencies are injected |
| `IAsyncDisposable` | Async cleanup when the container scope disposes |
| `ContainerBuilderFactory` | ScriptableObject base class for wiring registrations via `[SerializeField]` config |

### Asset Loading

| Type | Description |
|------|-------------|
| `AssetRefT<T>` | Base serializable reference to a Unity asset (Resources or Addressables) |
| `AudioClipAssetRef` | Typed ref for `AudioClip` assets |
| `GameObjectAssetRefT<T>` | Typed ref for prefabs with a root component type |
| `ScriptableObjectAssetRefT<T>` | Typed ref for ScriptableObject assets |
| `SpriteAssetRef`, `MeshAssetRef`, `Texture2DAssetRef` | Typed refs for other asset types |

### Services

| Type | Description |
|------|-------------|
| `IErrorReporter` | Abstraction over crash reporting (default: Sentry) |
| `IConnectivityService` | Network connectivity monitoring and retry logic |
| `ILoggingService` | Unity log handler with filtering via `ILogFilter` |

### MVC

| Type | Description |
|------|-------------|
| `Controller<TModel, TView, TResult>` | Base controller with model/view/result generics |
| `Factory<TId, TModel, TView, TResult, TController>` | MVC factory for registering controller stacks |
| `IView` | View contract for MonoBehaviour-backed UI |

### Bootstrap

| Type | Description |
|------|-------------|
| `Main` | Static entry point -- loads `MainArgs` from Resources, builds root container, runs `MainState` |
| `MainArgs` | Abstract `ContainerBuilderFactory` that wires logging, the app factory, and core services |

## Dependencies

- `com.deepforestlabs.logger` -- structured logging
- `com.cysharp.unitask` -- async/await for Unity
- `com.cysharp.zstring` -- zero-allocation string formatting
- `com.unity.addressables` -- asset management
- `io.sentry.unity` -- error reporting
