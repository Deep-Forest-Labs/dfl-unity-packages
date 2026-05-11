# Deep Forest Labs Audio

Audio service package providing pooled `AudioSource` playback, Unity `AudioMixer` integration, sound catalogs, ducking, and DI-driven lifecycle management. Built on top of the DFL framework's dependency injection and `AudioClipAssetRef` asset loading.

## Quick Start

```csharp
// 1. Register in your ContainerBuilderFactory
public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return builder
        .AddAudioService(_audioMixerConfig, _soundCatalog)
        .AddTransient<GameState>();
}

// 2. Inject and use anywhere
public sealed class GameController
{
    [Dependency] private readonly IAudioService _audio = null!;

    public async UniTask OnSpin(CancellationToken token)
    {
        // Play by catalog key
        await _audio.PlaySfx("spin_click", token: token);

        // Play by asset ref with options
        ISoundHandle bgm = await _audio.PlayBgm(_bgmClip, new SoundParams
        {
            Volume = 0.8f,
            CrossfadeDuration = 1.5f
        }, token);

        // Per-instance control
        bgm.Volume = 0.5f;
        bgm.FadeOut(2f);
    }
}
```

## Setup

1. **Create an AudioMixer** in Unity with groups: Master > BGM, SFX, UI. Expose volume parameters (`MasterVolume`, `BGMVolume`, `SFXVolume`, `UIVolume`).
2. **Create an AudioMixerConfig** asset (Assets > Create > Audio > Mixer Config). Assign the mixer and map group names to mixer groups.
3. **Optionally create a SoundCatalog** asset (Assets > Create > Audio > Sound Catalog). Add entries with keys, clip refs, and per-sound defaults.
4. **Register** via `builder.AddAudioService(config, catalog)` in your `ContainerBuilderFactory`.

## Key Types

### Public API

| Type | Description |
|------|-------------|
| `IAudioService` | Main service interface: play SFX/BGM, stop, volume/mute, preload/unload |
| `ISoundHandle` | Per-instance control: volume, pan, mute, pause, resume, stop, fade |
| `SoundParams` | Value object for play options (volume, pan, loop, fade, ducking, max instances) |
| `SoundGroupId` | Strongly-typed group identifier. Built-ins: `Bgm`, `Sfx`, `Ui` |
| `SoundState` | Enum: `Playing`, `Paused`, `Stopped` |
| `ContainerExtensions` | `AddAudioService(IContainerBuilder, AudioMixerConfig, SoundCatalog?)` |

### ScriptableObject Assets

| Type | Description |
|------|-------------|
| `AudioMixerConfig` | Maps `SoundGroupId` names to `AudioMixerGroup` references and exposed volume parameters |
| `SoundCatalog` | Keyed collection of `SoundEntry` items with `ISerializationCallbackReceiver` for O(1) lookup |
| `DuckingProfile` | Defines ducking: target group, dB reduction, attack/release times |

### Catalog

| Type | Description |
|------|-------------|
| `SoundEntry` | One catalog row: key, `AudioClipAssetRef`, group, default volume/pan, max instances, pool prewarm, preload flag, optional ducking |

## Features

- **On-demand + optional preload**: clips load via `IContainer.Checkout` when first played; catalog entries can be marked for eager preload during `Initialize`
- **AudioSource pooling**: pooled instances with configurable capacity, per-clip max instances (steals oldest), prewarm hints
- **Mixer routing**: all playback routed through Unity `AudioMixerGroup`; volume controlled via exposed parameters with linear-to-dB conversion
- **Ducking**: `DuckingProfile` assets define which group to duck and by how much; multiple requests stack (deepest duck wins)
- **BGM crossfade**: `PlayBgm` with `CrossfadeDuration` fades out the previous track while fading in the new one
- **Catalog string keys**: play by name (`PlaySfx("spin_click")`) with per-entry defaults so callers don't repeat configuration

## Dependencies

- `com.deepforestlabs.framework` -- DI container, `AudioClipAssetRef`, lifecycle interfaces
- `com.cysharp.unitask` -- async playback and asset loading
