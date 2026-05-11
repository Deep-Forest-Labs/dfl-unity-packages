# Dependency Injection

The DI container is the core of the DFL framework. It manages service registration, dependency resolution, async lifecycle, asset loading, and scope-based disposal.

## Registration

All registration happens on `IContainerBuilder` before `Build()` is called. The builder is fluent -- every method returns `IContainerBuilder` for chaining.

### Singletons

Shared across all scopes. One instance for the lifetime of the container.

```csharp
// Register an existing instance
builder.AddSingleton(myConfigObject);
builder.AddSingleton<MyConfig>(myConfigObject);

// Type alias: resolve IFoo as the registered Foo instance
builder.AddAlias<IFoo, Foo>();
```

### Scoped Services

Created once per container scope. Disposed when the scope ends.

```csharp
// Default constructor
builder.AddScoped<MyService>();

// Interface + implementation
builder.AddScoped<IMyService, MyService>();

// Custom factory
builder.AddScoped<IMyService>(collection => new MyService(collection.Get<IDependency>()));

// Async factory
builder.AddScoped<IMyService>(async (collection, token) =>
{
    var dep = collection.Get<IDependency>();
    return await MyService.CreateAsync(dep, token);
});
```

### Transients

Fresh instance created on every resolution. Not tracked by the container for disposal.

```csharp
builder.AddTransient<GameState>();
builder.AddTransient<IGameState, GameState>();
builder.AddTransient<IGameState>(collection => new GameState());
```

### ScriptableObject Assets

Load a `ScriptableObject` via `AssetRef` and register it as a singleton:

```csharp
builder.AddScriptableObject(_myConfigRef);  // ScriptableObjectAssetRefT<MyConfig>
```

### Unity Components

```csharp
builder.AddScopedComponent<AudioListener>(); // Creates a component on the container's GameObject
```

## Injection

### The `[Dependency]` Attribute

Mark fields or properties to be automatically filled by the container after construction:

```csharp
public sealed class PlayerController : IInitializable
{
    [Dependency] private readonly IContainer _container = null!;
    [Dependency] private readonly IAudioService _audio = null!;
    [Dependency] private readonly IGameService _game = null!;
}
```

**Rules:**
- Fields must have the `[Dependency]` attribute
- Non-nullable fields are **required** -- a `DiException` is thrown if the type isn't registered
- Nullable fields (`T?`) are **optional** -- silently skipped if not registered
- Injection happens once, after the container is built, before lifecycle methods

### Optional Dependencies

```csharp
public sealed class AnalyticsService
{
    [Dependency] private readonly IContainer _container = null!;
    [Dependency] private readonly IAnalyticsBackend? _backend = null; // won't fail if not registered
}
```

## Lifecycle Interfaces

Services can implement lifecycle interfaces to receive callbacks at specific phases:

### `IInitializable`

Called after all dependencies are injected. This is the primary place for async setup work (loading assets, connecting to servers, etc.).

```csharp
public interface IInitializable
{
    UniTask Initialize(CancellationToken token);
}
```

The `token` is tied to the container's scope -- if the scope disposes, initialization is cancelled.

### `IAsyncDisposable`

Called when the container scope disposes. Use for async cleanup (saving state, releasing network connections).

```csharp
public interface IAsyncDisposable
{
    UniTask DisposeAsync();
}
```

### `IDisposable`

Standard synchronous disposal, called after `IAsyncDisposable`.

### Lifecycle Order

```
Build() -> Inject [Dependency] fields -> PreInitialize -> Initialize -> [running] -> DisposeAsync -> Dispose
```

All `IInitializable.Initialize` calls can run concurrently -- the container does not guarantee ordering between services. If service A depends on service B being initialized first, A should access B through `[Dependency]` injection (which is available) and handle the timing in its own `Initialize`.

## Scopes and Child Containers

### Creating Child Scopes

Use `AddChild` to create nested container scopes. Each child has its own scoped services but inherits singletons from the parent:

```csharp
public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return builder
        .AddChild(_featureFactory)  // ContainerFactory asset ref
        .AddScoped<IGameService, GameService>();
}
```

### Resolution Order

When resolving a type, the container checks in order:
1. Scoped services in the current container
2. Singletons in the current container
3. Transient resolvers in the current container
4. Parent container (recursively up to root)

### Scope Disposal

When a scope disposes, it:
1. Cancels the scope's `CancellationToken`
2. Calls `IAsyncDisposable.DisposeAsync()` on scoped services
3. Calls `IDisposable.Dispose()` on scoped services
4. Releases any assets checked out through this scope

## ContainerBuilderFactory Pattern

The standard pattern for organizing DI registrations is a `ContainerBuilderFactory` -- a `ScriptableObject` that holds serialized configuration and registers services:

```csharp
[CreateAssetMenu(fileName = "GameFactory", menuName = "App/Game Factory")]
public sealed class GameFactory : ContainerBuilderFactory
{
    [SerializeField] private GameObjectAssetRefT<PlayerView> _playerPrefab = default!;
    [SerializeField] private ScriptableObjectAssetRefT<GameConfig> _gameConfig = default!;

    public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
    {
        return builder
            .AddScriptableObject(_gameConfig)
            .AddGameObjectManager(_playerPrefab, GameObjectManagerOptions.Required)
            .AddScoped<IGameService, GameService>();
    }
}
```

This keeps configuration in Unity's asset system (inspectable, version-controlled, environment-specific) while the actual service wiring stays in code.

### Composing Factories

`MainArgs` uses `AddFromBuilder` to chain factories:

```csharp
// In MainArgs -- loads a ContainerBuilderFactory from an AssetRef and runs its AddToBuilder
builder.AddFromBuilder(appContainerFactory);

// Or from an asset reference
builder.AddFromBuilder(_appFactoryRef);  // ScriptableObjectAssetRefT<ContainerBuilderFactory>
```

## IDiCollection (Read-Only Resolution)

`IDiCollection` is the read-only resolution interface, available on both `IContainerBuilder` and `IContainer`:

```csharp
public interface IDiCollection
{
    CancellationToken Scope { get; }
    T Get<T>();            // throws if not found
    T? Find<T>();          // returns null if not found
    bool TryGet<T>(out T? instance);
}
```

Use `Get<T>()` when you're certain the type is registered. Use `Find<T>()` or `TryGet<T>()` for optional dependencies at runtime (as opposed to using nullable `[Dependency]` fields for injection-time optionality).

## Extension Methods

Packages provide registration extensions on `IContainerBuilder`:

```csharp
// Framework -- connectivity service
builder.AddConnectivityService();

// Audio -- audio service with mixer config and optional catalog
builder.AddAudioService(mixerConfig, soundCatalog);
```

This is the recommended pattern for packages that need multiple related registrations.
