#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Logger;
using UnityEngine;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    internal sealed class AudioService : IAudioService, IInitializable, IAsyncDisposable
    {
        [Dependency] private readonly IContainer _container = null!;
        [Dependency] private readonly IAudioMixerProvider _mixerProvider = null!;
        [Dependency] private readonly AudioAssetCache _assetCache = null!;
        [Dependency] private readonly DuckingController _duckingController = null!;
        [Dependency] private readonly SoundCatalog? _catalog = null;

        private AudioSourcePool _pool = null!;
        private GameObject _root = null!;
        private readonly List<SoundHandle> _activeHandles = new();
        private readonly Dictionary<SoundHandle, ActiveSound> _activeSounds = new();
        private readonly Dictionary<SoundGroupId, SoundHandle?> _bgmHandles = new();

        private readonly struct ActiveSound
        {
            public readonly PooledAudioSource Source;
            public readonly AudioClipAssetRef ClipRef;

            public ActiveSound(PooledAudioSource source, AudioClipAssetRef clipRef)
            {
                Source = source;
                ClipRef = clipRef;
            }
        }

        public bool IsMuted
        {
            get => _mixerProvider.GetMasterMute();
            set => _mixerProvider.SetMasterMute(value);
        }

        public float MasterVolume
        {
            get => _mixerProvider.GetMasterVolume();
            set => _mixerProvider.SetMasterVolume(value);
        }

        public async UniTask Initialize(CancellationToken token)
        {
            _root = new GameObject("[AudioService]");
            Object.DontDestroyOnLoad(_root);

            _pool = new AudioSourcePool(_root.transform);

            if (_catalog != null)
            {
                await _assetCache.PreloadFromCatalog(_catalog, token);

                foreach (SoundEntry entry in _catalog.Entries)
                {
                    if (entry.PoolPrewarm > 0)
                    {
                        _pool.Prewarm(entry.PoolPrewarm);
                    }
                }
            }
        }

        public UniTask DisposeAsync()
        {
            StopAll();
            _duckingController.ReleaseAll();
            _pool.ReturnAll();
            _assetCache.ReleaseAll();

            if (_root != null)
            {
                Object.Destroy(_root);
            }

            return UniTask.CompletedTask;
        }

        public async UniTask<ISoundHandle> PlaySfx(AudioClipAssetRef clip, SoundParams? options = null, CancellationToken token = default)
        {
            SoundParams p = ResolveParams(options, SoundGroupId.Sfx, false);
            return await PlayInternal(clip, p, token);
        }

        public async UniTask<ISoundHandle> PlayBgm(AudioClipAssetRef clip, SoundParams? options = null, CancellationToken token = default)
        {
            SoundParams p = ResolveParams(options, SoundGroupId.Bgm, true);
            return await PlayBgmInternal(clip, p, token);
        }

        public async UniTask<ISoundHandle> PlaySfx(string key, SoundParams? options = null, CancellationToken token = default)
        {
            SoundEntry entry = ResolveCatalogEntry(key);
            SoundParams p = MergeEntryParams(entry, options, false);
            return await PlayInternal(entry.Clip, p, token);
        }

        public async UniTask<ISoundHandle> PlayBgm(string key, SoundParams? options = null, CancellationToken token = default)
        {
            SoundEntry entry = ResolveCatalogEntry(key);
            SoundParams p = MergeEntryParams(entry, options, true);
            return await PlayBgmInternal(entry.Clip, p, token);
        }

        public async UniTask<ISoundHandle> PlaySfxAt(AudioClipAssetRef clip, Vector3 worldPosition, SoundParams? options = null, CancellationToken token = default)
        {
            SoundParams p = ResolveParams(options, SoundGroupId.Sfx, false);
            p = WithWorldPosition(p, worldPosition);
            return await PlayInternal(clip, p, token);
        }

        public async UniTask<ISoundHandle> PlaySfxAt(string key, Vector3 worldPosition, SoundParams? options = null, CancellationToken token = default)
        {
            SoundEntry entry = ResolveCatalogEntry(key);
            SoundParams p = MergeEntryParams(entry, options, false);
            p = WithWorldPosition(p, worldPosition);
            return await PlayInternal(entry.Clip, p, token);
        }

        public void StopGroup(SoundGroupId group)
        {
            for (int i = _activeHandles.Count - 1; i >= 0; i--)
            {
                if (_activeHandles[i].Group == group)
                {
                    _activeHandles[i].Stop();
                }
            }
        }

        public void StopAll()
        {
            for (int i = _activeHandles.Count - 1; i >= 0; i--)
            {
                _activeHandles[i].Stop();
            }
        }

        public float GetGroupVolume(SoundGroupId group) => _mixerProvider.GetVolume(group);
        public void SetGroupVolume(SoundGroupId group, float volume) => _mixerProvider.SetVolume(group, volume);
        public bool GetGroupMute(SoundGroupId group) => _mixerProvider.GetMute(group);
        public void SetGroupMute(SoundGroupId group, bool muted) => _mixerProvider.SetMute(group, muted);

        public async UniTask Preload(AudioClipAssetRef clip, CancellationToken token)
        {
            await _assetCache.Load(clip, token);
        }

        public async UniTask PreloadCatalog(CancellationToken token)
        {
            if (_catalog != null)
            {
                await _assetCache.PreloadFromCatalog(_catalog, token);
            }
        }

        public void Unload(AudioClipAssetRef clip)
        {
            _assetCache.Release(clip);
        }

        private async UniTask<ISoundHandle> PlayInternal(AudioClipAssetRef clipRef, SoundParams p, CancellationToken token)
        {
            AudioClip clip = await _assetCache.Load(clipRef, token);
            AudioMixerGroup? group = _mixerProvider.GetGroup(p.Group);

            PooledAudioSource? source = _pool.Rent(group, clip, p.MaxInstances);
            if (source == null)
            {
                Log.Warning("AudioSourcePool exhausted or max instances reached for clip");
                _assetCache.Release(clipRef);
                return CreateStoppedHandle(p.Group);
            }

            source.Group = p.Group;
            float startVolume = p.FadeInDuration > 0f ? 0f : p.Volume;
            if (p.WorldPosition.HasValue)
            {
                float blend = p.SpatialBlend > 0f ? p.SpatialBlend : 1f;
                float minDist = p.MinDistance > 0f ? p.MinDistance : 1f;
                float maxDist = p.MaxDistance > 0f ? p.MaxDistance : 40f;
                source.ConfigureSpatial(clip, group, startVolume, p.Loop, p.WorldPosition.Value,
                    blend, minDist, maxDist, p.Spatialize);
            }
            else
            {
                source.Configure(clip, group, startVolume, p.Pan, p.Loop);
            }
            source.Play();

            SoundHandle handle = new SoundHandle(source, p.Group, p.Volume, OnHandleStopped);
            _activeHandles.Add(handle);
            _activeSounds[handle] = new ActiveSound(source, clipRef);

            if (p.Ducking != null)
            {
                _duckingController.ApplyDucking(p.Ducking, handle);
            }

            if (p.FadeInDuration > 0f)
            {
                FadeInAsync(source, p.Volume, p.FadeInDuration, token).Forget();
            }

            if (!p.Loop)
            {
                MonitorClipCompletion(handle, clip.length, token).Forget();
            }

            return handle;
        }

        private async UniTask<ISoundHandle> PlayBgmInternal(AudioClipAssetRef clipRef, SoundParams p, CancellationToken token)
        {
            if (_bgmHandles.TryGetValue(p.Group, out SoundHandle? existingHandle) && existingHandle != null &&
                existingHandle.State != SoundState.Stopped)
            {
                if (p.CrossfadeDuration > 0f)
                {
                    existingHandle.FadeOut(p.CrossfadeDuration);
                }
                else
                {
                    existingHandle.Stop();
                }
            }

            ISoundHandle newHandle = await PlayInternal(clipRef, p, token);
            _bgmHandles[p.Group] = (SoundHandle)newHandle;
            return newHandle;
        }

        private void OnHandleStopped(SoundHandle handle)
        {
            _activeHandles.Remove(handle);
            _duckingController.ReleaseDucking(handle);

            if (_activeSounds.TryGetValue(handle, out ActiveSound active))
            {
                _activeSounds.Remove(handle);
                _pool.Return(active.Source);
                _assetCache.Release(active.ClipRef);
            }

            foreach (KeyValuePair<SoundGroupId, SoundHandle?> kvp in _bgmHandles)
            {
                if (kvp.Value == handle)
                {
                    _bgmHandles[kvp.Key] = null;
                    break;
                }
            }
        }

        private async UniTaskVoid MonitorClipCompletion(SoundHandle handle, float clipLength, CancellationToken token)
        {
            await UniTask.Delay((int)(clipLength * 1000f), ignoreTimeScale: true, cancellationToken: token)
                .SuppressCancellationThrow();

            if (handle.State == SoundState.Playing)
            {
                handle.MarkStopped();
                OnHandleStopped(handle);
            }
        }

        private async UniTaskVoid FadeInAsync(PooledAudioSource source, float targetVolume, float duration, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!source.IsActive) return;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.Source.volume = Mathf.Lerp(0f, targetVolume, t);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            if (source.IsActive)
            {
                source.Source.volume = targetVolume;
            }
        }

        private SoundEntry ResolveCatalogEntry(string key)
        {
            Log.Assert(_catalog != null, "SoundCatalog is null — cannot play by key '{0}'", key);
            bool found = _catalog!.TryGetEntry(key, out SoundEntry entry);
            Log.Assert(found, "SoundCatalog entry not found for key '{0}'", key);
            return entry;
        }

        private static SoundParams ResolveParams(SoundParams? options, SoundGroupId defaultGroup, bool defaultLoop)
        {
            if (options.HasValue)
            {
                SoundParams o = options.Value;
                return new SoundParams
                {
                    Group = o.Group.Name != null ? o.Group : defaultGroup,
                    Volume = o.Volume > 0f ? o.Volume : 1f,
                    Pan = o.Pan,
                    Loop = o.Loop || defaultLoop,
                    FadeInDuration = o.FadeInDuration,
                    CrossfadeDuration = o.CrossfadeDuration,
                    Ducking = o.Ducking,
                    MaxInstances = o.MaxInstances,
                    WorldPosition = o.WorldPosition,
                    SpatialBlend = o.SpatialBlend,
                    MinDistance = o.MinDistance,
                    MaxDistance = o.MaxDistance,
                    Spatialize = o.Spatialize
                };
            }

            return new SoundParams
            {
                Group = defaultGroup,
                Volume = 1f,
                Pan = 0f,
                Loop = defaultLoop,
                FadeInDuration = 0f,
                CrossfadeDuration = 0f,
                Ducking = null,
                MaxInstances = 0,
                WorldPosition = null,
                SpatialBlend = 0f,
                MinDistance = 0f,
                MaxDistance = 0f,
                Spatialize = false
            };
        }

        private static SoundParams MergeEntryParams(SoundEntry entry, SoundParams? options, bool defaultLoop)
        {
            SoundParams resolved = ResolveParams(options, entry.Group, defaultLoop);
            return new SoundParams
            {
                Group = resolved.Group.Name != null ? resolved.Group : entry.Group,
                Volume = options.HasValue && options.Value.Volume > 0f ? resolved.Volume : entry.DefaultVolume,
                Pan = options.HasValue ? resolved.Pan : entry.DefaultPan,
                Loop = resolved.Loop,
                FadeInDuration = resolved.FadeInDuration,
                CrossfadeDuration = resolved.CrossfadeDuration,
                Ducking = resolved.Ducking ?? entry.Ducking,
                MaxInstances = resolved.MaxInstances > 0 ? resolved.MaxInstances : entry.MaxInstances,
                WorldPosition = resolved.WorldPosition,
                SpatialBlend = resolved.SpatialBlend > 0f ? resolved.SpatialBlend : entry.SpatialBlend,
                MinDistance = resolved.MinDistance > 0f ? resolved.MinDistance : entry.MinDistance,
                MaxDistance = resolved.MaxDistance > 0f ? resolved.MaxDistance : entry.MaxDistance,
                Spatialize = resolved.Spatialize || entry.Spatialize
            };
        }

        private static SoundParams WithWorldPosition(SoundParams p, Vector3 position)
        {
            return new SoundParams
            {
                Group = p.Group,
                Volume = p.Volume,
                Pan = p.Pan,
                Loop = p.Loop,
                FadeInDuration = p.FadeInDuration,
                CrossfadeDuration = p.CrossfadeDuration,
                Ducking = p.Ducking,
                MaxInstances = p.MaxInstances,
                WorldPosition = position,
                SpatialBlend = p.SpatialBlend,
                MinDistance = p.MinDistance,
                MaxDistance = p.MaxDistance,
                Spatialize = p.Spatialize
            };
        }

        private static ISoundHandle CreateStoppedHandle(SoundGroupId group)
        {
            return new NullSoundHandle(group);
        }
    }
}
#nullable disable
