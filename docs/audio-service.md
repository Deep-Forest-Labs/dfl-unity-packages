# Audio Service

The `com.deepforestlabs.audio` package provides a DI-integrated audio service with pooled `AudioSource` playback, Unity `AudioMixer` routing, sound catalogs, and ducking.

## Setup

### 1. Create an AudioMixer

In Unity, create an AudioMixer (Assets > Create > Audio Mixer) with this group hierarchy:

- **Master** (expose parameter: `MasterVolume`)
  - **BGM** (expose parameter: `BGMVolume`)
  - **SFX** (expose parameter: `SFXVolume`)
  - **UI** (expose parameter: `UIVolume`)

To expose a parameter: select the group in the AudioMixer window, right-click the volume slider, choose "Expose to script", and rename it in the Exposed Parameters list.

### 2. Create an AudioMixerConfig

Assets > Create > Audio > Mixer Config. In the inspector:
- Assign your AudioMixer
- Set the master volume parameter name (default: `MasterVolume`)
- Add group mappings: for each `SoundGroupId` name (`BGM`, `SFX`, `UI`), assign the corresponding `AudioMixerGroup` and its exposed volume parameter name

### 3. Create a SoundCatalog (Optional)

Assets > Create > Audio > Sound Catalog. Add entries for sounds you want to play by string key. Each entry has:
- **Key** -- lookup name (e.g. `"spin_click"`, `"bgm_lobby"`)
- **Clip** -- `AudioClipAssetRef` pointing to the audio asset
- **Group** -- which mixer group to route to
- **Default Volume / Pan** -- per-sound defaults
- **Max Instances** -- concurrent play limit (0 = unlimited)
- **Pool Prewarm** -- pre-allocate AudioSource instances for frequently-played sounds
- **Preload** -- load the clip into memory during service initialization
- **Ducking** -- optional `DuckingProfile` reference

### 4. Register the Service

In your `ContainerBuilderFactory`:

```csharp
[SerializeField] private AudioMixerConfig _audioMixerConfig = default!;
[SerializeField] private SoundCatalog? _soundCatalog;

public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
{
    return builder
        .AddAudioService(_audioMixerConfig, _soundCatalog);
}
```

## Playing Sounds

### By Asset Reference

```csharp
[Dependency] private readonly IAudioService _audio = null!;

// Play SFX (fire and forget)
ISoundHandle handle = await _audio.PlaySfx(_clickClip, token: token);

// Play BGM (looping by default)
ISoundHandle bgm = await _audio.PlayBgm(_menuMusic, token: token);
```

### By Catalog Key

```csharp
// Requires a SoundCatalog with a "spin_click" entry
ISoundHandle handle = await _audio.PlaySfx("spin_click", token: token);
ISoundHandle bgm = await _audio.PlayBgm("bgm_lobby", token: token);
```

Catalog lookups merge the entry's defaults (group, volume, pan, ducking, max instances) with any `SoundParams` you provide. Caller-specified values override catalog defaults.

### With Options

```csharp
ISoundHandle handle = await _audio.PlaySfx(_clickClip, new SoundParams
{
    Volume = 0.7f,
    Pan = -0.5f,           // slightly left
    MaxInstances = 3,       // at most 3 concurrent plays of this clip
    FadeInDuration = 0.2f,
}, token);
```

### BGM Crossfade

```csharp
ISoundHandle newBgm = await _audio.PlayBgm(_battleMusic, new SoundParams
{
    CrossfadeDuration = 2f,  // fade out current BGM over 2 seconds while fading in new
}, token);
```

## Sound Handle

Every `Play` call returns an `ISoundHandle` for per-instance control:

```csharp
ISoundHandle handle = await _audio.PlaySfx(_clip, token: token);

handle.Volume = 0.5f;        // adjust volume (0..1)
handle.Pan = 0.3f;           // stereo pan (-1..1)
handle.IsMuted = true;       // mute without stopping

handle.Pause();               // pause playback
handle.Resume();              // resume from pause

handle.FadeOut(1.5f);        // fade to silence over 1.5s, then stop
handle.Stop();                // immediate stop

SoundState state = handle.State;  // Playing, Paused, or Stopped
SoundGroupId group = handle.Group; // which group this sound belongs to
```

Non-looping sounds automatically stop and clean up when the clip finishes.

## Volume and Mute

### Master Volume

```csharp
_audio.MasterVolume = 0.8f;  // 0..1 linear, converted to dB internally
_audio.IsMuted = true;        // mutes everything via -80dB on master
```

### Per-Group Volume

```csharp
_audio.SetGroupVolume(SoundGroupId.Bgm, 0.5f);
_audio.SetGroupVolume(SoundGroupId.Sfx, 1.0f);
_audio.SetGroupMute(SoundGroupId.Bgm, true);

float bgmVol = _audio.GetGroupVolume(SoundGroupId.Bgm);
bool sfxMuted = _audio.GetGroupMute(SoundGroupId.Sfx);
```

### Custom Groups

Create additional groups beyond the built-in `Bgm`, `Sfx`, and `Ui`:

```csharp
public static readonly SoundGroupId Ambience = new("Ambience");
public static readonly SoundGroupId Voice = new("Voice");
```

Add matching mixer groups in your AudioMixer and map them in `AudioMixerConfig`.

## Ducking

Ducking temporarily lowers one or more groups' volume when a specific sound plays (e.g. duck BGM when a win fanfare plays).

### Setup

1. Create a `DuckingProfile` asset (Assets > Create > Audio > Ducking Profile)
2. Configure: target group to duck, any groups it also ducks, dB reduction, attack/release times
3. Assign it to a `SoundEntry` in your catalog, or pass it via `SoundParams`

### Usage

```csharp
// Via SoundParams
ISoundHandle fanfare = await _audio.PlaySfx(_fanfareClip, new SoundParams
{
    Ducking = _bgmDuckProfile,  // DuckingProfile ScriptableObject
}, token);
// BGM volume drops while fanfare plays, restores when it stops

// Via catalog -- assign the DuckingProfile on the SoundEntry in the inspector
ISoundHandle fanfare = await _audio.PlaySfx("win_fanfare", token: token);
```

A profile ducks its target group and every group in **Also Ducks** by the same amount, on the same attack and release.

Each group fades to its new level: going deeper takes the fastest attack among its active requests, and coming back up takes the release time of the profile that stopped. Times are unscaled, so ducks fade while the game is paused. An attack or release of 0 changes the volume at once.

Multiple simultaneous duck requests on the same group stack -- the deepest reduction wins. When all ducking triggers stop, the group fades back to its original volume.

## Preloading

### Individual Clips

```csharp
await _audio.Preload(_importantClip, token);
// Later plays are instant (no async load delay)
```

### Catalog Preload

```csharp
await _audio.PreloadCatalog(token);
// Loads all entries with Preload = true in parallel
```

Entries marked `Preload` in the catalog are also automatically loaded during the service's `Initialize` phase.

### Unloading

```csharp
_audio.Unload(_clipRef);
// Decrements the internal ref count; clip is released when count hits zero
```

## Stopping Sounds

```csharp
_audio.StopGroup(SoundGroupId.Sfx);  // stop all SFX
_audio.StopAll();                      // stop everything
```

## Pooling

The service maintains a pool of `AudioSource` components on hidden `GameObjects`. Pool behavior:

- **Initial capacity**: 8 sources (grows on demand up to max 64)
- **Max instances per clip**: set via `SoundParams.MaxInstances` or `SoundEntry.MaxInstances`; when the limit is reached, the oldest instance of that clip is stolen
- **Prewarm**: catalog entries with `PoolPrewarm > 0` pre-allocate idle sources during initialization
- **Auto-return**: non-looping sources are returned to the pool when the clip finishes

## Voice Budget

Unity plays at most **Max Real Voices** (Project Settings > Audio) at once. Past that, Unity virtualizes the sources with the worst `AudioSource.priority`. The voice budget lets the service decide what plays instead.

### Pool size

`AudioMixerConfig.SizePoolToRealVoices` (off by default) sizes the pool to `AudioSettings.GetConfiguration().numRealVoices`, read once when the service initializes. Off, the pool stays at 64.

### Per-group settings

Each `AudioMixerConfig` group mapping has:

| Field | Default | Meaning |
|---|---|---|
| `Max Voices` | 0 (no cap) | Most voices this group plays at once |
| `Reserved Voices` | 0 | Pool slots only this group may use |
| `When Full` | `RejectNew` | At `Max Voices`: `RejectNew` drops the new sound, `StealOldest` stops the group's oldest voice |
| `Priority` | 128 | `AudioSource.priority` for the group's sounds. 0 is most important, 256 least |

Add a mapping per group your game routes to. Groups beyond the built-ins (`BGM`, `SFX`, `UI`) are listed in a `SoundGroupRegistry` so **Auto-Map Groups from Mixer** and the catalog's group picker see them.

### Per-sound priority

A `SoundEntry` with **Override Priority** on uses its own `Priority` instead of the group's. `SoundParams.Priority` overrides both for one play. The resolved value is set on the pooled `AudioSource` when the sound plays.

### When the pool is full

A new sound takes the voice of the least important playing sound that is strictly less important than itself (oldest first on ties), and is rejected if there is none. Voices inside another group's reservation are never taken. With every priority at the default 128, nothing is taken and a full pool rejects the new sound, as before.

## Architecture

See [Dependency Injection](dependency-injection.md) for how `AddAudioService` works under the hood. The service registers:

- `AudioMixerConfig` as singleton
- `SoundCatalog` as optional singleton
- `IAudioMixerProvider` (scoped) -- mixer group resolution and volume control
- `AudioAssetCache` (scoped) -- ref-counted clip loading via `IContainer.Checkout`
- `DuckingController` (scoped) -- stacking ducking requests and fading each group's duck
- `IAudioService` -> `AudioService` (scoped) -- orchestrates everything
