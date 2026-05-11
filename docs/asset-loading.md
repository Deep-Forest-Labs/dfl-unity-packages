# Asset Loading

The framework provides a unified asset loading system that works with both Unity `Resources` and `Addressables`. Assets are loaded through the DI container, which manages their lifecycle and releases them when the owning scope disposes.

## AssetRef Types

All asset references extend `AssetRefT<T>`, a serializable class that stores either a Resources path or an Addressables GUID:

| Type | Asset Type | Use Case |
|------|-----------|----------|
| `AudioClipAssetRef` | `AudioClip` | Sound effects, music |
| `GameObjectAssetRefT<T>` | `GameObject` with root `T` component | Prefabs (views, UI, world objects) |
| `GameObjectAssetRef` | `GameObject` (untyped) | Prefabs without a specific root component |
| `ScriptableObjectAssetRefT<T>` | `ScriptableObject` of type `T` | Configuration assets, data catalogs |
| `ScriptableObjectAssetRef` | `ScriptableObject` (untyped) | Generic SO references |
| `SpriteAssetRef` | `Sprite` | UI images |
| `AtlasedSpriteAssetRef` | `Sprite` (from atlas) | Atlased UI sprites |
| `SpriteAtlasAssetRef` | `SpriteAtlas` | Full atlas loading |
| `Texture2DAssetRef` | `Texture2D` | Textures |
| `MeshAssetRef` | `Mesh` | 3D meshes |
| `SceneAssetRef` | Scene | Addressable scenes |

### Loading Modes

```csharp
public enum AssetRefMode
{
    Resources,     // Loaded via Resources.Load
    Addressables   // Loaded via Addressables (GUID-based)
}
```

Each `AssetRef` type has factory methods:

```csharp
AudioClipAssetRef.FromResources("Audio/bgm_main");
AudioClipAssetRef.FromAddressables("a1b2c3d4e5f6...");
```

In practice, you rarely create these in code -- they're serialized as `[SerializeField]` fields on ScriptableObjects and assigned in the inspector.

## Loading Assets at Build Time (Registration)

Register assets during `IContainerBuilder` setup to ensure they're downloaded and available:

### Download (Prefetch)

Ensures the Addressable bundle is downloaded but doesn't load the asset into memory:

```csharp
builder.AddDownload(_myPrefabRef);     // GameObjectAssetRef
builder.AddDownload(_mySpriteRef);     // SpriteAssetRef
builder.AddDownload(_myClipRef);       // AudioClipAssetRef
```

### Register as Scoped

Loads the asset and registers it for resolution via the container:

```csharp
// ScriptableObject -- loaded and available via Get<MyConfig>()
builder.AddScriptableObject(_configRef);

// AudioClip -- loaded and available via Checkout
builder.AddAudioClip(_clipRef);

// Scene -- loaded additively
builder.AddScene(_sceneRef);

// Sprite / Texture / Mesh
builder.AddSprite(_spriteRef);
builder.AddTextures2D(_textureRef);
builder.AddMesh(_meshRef);
```

### GameObject Managers

For prefabs, use `AddGameObjectManager` which sets up pooled instantiation:

```csharp
builder.AddGameObjectManager(_prefabRef, GameObjectManagerOptions.Required);
builder.AddGameObjectManager<PlayerView>(_playerRef, GameObjectManagerOptions.Required);
```

`GameObjectManagerOptions` controls loading behavior:
- `Required` -- must load successfully or container build fails
- `LegacyInstancePool` -- uses the legacy pooling path
- `LegacyManagedInstancePool` -- managed instance pool with lifecycle tracking

### Scoped GameObjects

Instantiate a prefab that lives for the scope's lifetime:

```csharp
builder.AddScopedGameObject(_prefabRef, parentTransform);
builder.AddScopedGameObject<MyView>(_viewRef, null); // no parent
```

## Loading Assets at Runtime

### `IContainer.Checkout`

Load an asset on demand and hold a reference:

```csharp
public sealed class MyService : IInitializable
{
    [Dependency] private readonly IContainer _container = null!;

    public async UniTask Initialize(CancellationToken token)
    {
        // Load an AudioClip
        AudioClip clip = await _container.Checkout(_clipRef, token);

        // Load a ScriptableObject
        MyConfig config = await _container.Checkout(_configRef, token);

        // Load a Sprite and assign to an Image
        await _container.Checkout(_spriteRef, myImage, token);
    }
}
```

Checked-out assets are tied to the container's scope. When the scope disposes, the framework handles releasing the underlying Addressables handles.

### `IContainer.Download`

Ensure an Addressable bundle is downloaded without loading the asset:

```csharp
await _container.Download(_assetRef, _container.Scope);
```

### `IContainer.CanLocate`

Check whether an asset can be resolved (useful before attempting a load):

```csharp
if (_container.CanLocate(_assetRef))
{
    var clip = await _container.Checkout(_assetRef, token);
}
```

## Asset Lifecycle

Assets follow the container scope lifecycle:

1. **Registration** -- `AddDownload` / `AddGameObjectManager` / `AddScriptableObject` during builder phase
2. **Build** -- framework downloads registered assets, loads required ones
3. **Runtime** -- `Checkout` for on-demand loading
4. **Disposal** -- scope disposes, all checked-out assets are released

You do not need to manually release assets checked out through the container. The scope handles cleanup automatically.

## Addressables Configuration

`AddressablesBuildSettings` (part of `BuildSettings`) controls how assets are loaded:

| Setting | Description |
|---------|-------------|
| `AssetLoadStrategy.RemoteCDN` | Download bundles from a CDN at runtime |
| `AssetLoadStrategy.LocalBundles` | Ship bundles with the build (good for WebGL) |
| `UniqueId` | Unique build identifier for Addressables catalogs |
| `AssetId` | Asset version identifier |

In the editor, `ActivePlayModeIndex` switches between fast mode (AssetDatabase), local bundles, and remote bundles for testing.

## Practical Examples

### Loading a Prefab Pool

```csharp
public sealed class EnemyFactory : ContainerBuilderFactory
{
    [SerializeField] private GameObjectAssetRefT<EnemyView> _enemyPrefab = default!;

    public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
    {
        return builder
            .AddGameObjectManager(_enemyPrefab, GameObjectManagerOptions.Required);
    }
}

// Then in a service:
[Dependency] private readonly IGameObjectManagerT<EnemyView> _enemyPool = null!;

public async UniTask<EnemyView> SpawnEnemy(Transform parent, CancellationToken token)
{
    return await _enemyPool.Checkout(parent, false, token);
}
```

### Preloading Audio Clips

```csharp
// Register for download at build time
builder.AddDownload(_sfxClipRef);

// Load on demand later
AudioClip clip = await _container.Checkout(_sfxClipRef, token);
```

Or use the audio package's `IAudioService.Preload()` which handles ref-counting internally. See [Audio Service](audio-service.md).
