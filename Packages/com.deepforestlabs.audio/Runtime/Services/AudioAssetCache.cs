#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Logger;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    internal sealed class AudioAssetCache
    {
        [Dependency] private readonly IContainer _container = null!;

        private readonly Dictionary<AudioClipAssetRef, CacheEntry> _cache = new();
        private readonly Dictionary<AudioClipAssetRef, UniTaskCompletionSource<CacheEntry>> _pending = new();

        public async UniTask<AudioClip> Load(AudioClipAssetRef assetRef, CancellationToken token)
        {
            if (_cache.TryGetValue(assetRef, out CacheEntry entry))
            {
                entry.RefCount++;
                return entry.Clip;
            }

            if (_pending.TryGetValue(assetRef, out var pending))
            {
                entry = await pending.Task.AttachExternalCancellation(token);
                entry.RefCount++;
                return entry.Clip;
            }

            var tcs = new UniTaskCompletionSource<CacheEntry>();
            _pending[assetRef] = tcs;

            try
            {
                AudioClip clip = await _container.Checkout(assetRef, token);
                entry = new CacheEntry { Clip = clip, RefCount = 1 };
                _cache[assetRef] = entry;
                tcs.TrySetResult(entry);
                return clip;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _pending.Remove(assetRef);
            }
        }

        public void Release(AudioClipAssetRef assetRef)
        {
            if (!_cache.TryGetValue(assetRef, out CacheEntry entry))
                return;

            entry.RefCount--;
            if (entry.RefCount <= 0)
            {
                _cache.Remove(assetRef);
            }
        }

        public bool IsLoaded(AudioClipAssetRef assetRef) => _cache.ContainsKey(assetRef);

        public AudioClip? TryGetCached(AudioClipAssetRef assetRef)
        {
            return _cache.TryGetValue(assetRef, out CacheEntry entry) ? entry.Clip : null;
        }

        public async UniTask PreloadFromCatalog(SoundCatalog catalog, CancellationToken token)
        {
            List<UniTask> tasks = new();
            foreach (SoundEntry entry in catalog.Entries)
            {
                if (entry.Preload && !IsLoaded(entry.Clip))
                {
                    tasks.Add(LoadAndDiscard(entry.Clip, token));
                }
            }

            if (tasks.Count > 0)
            {
                Log.Info("Preloading {0} audio clips from catalog", tasks.Count);
                await UniTask.WhenAll(tasks);
            }
        }

        private async UniTask LoadAndDiscard(AudioClipAssetRef assetRef, CancellationToken token)
        {
            await Load(assetRef, token);
        }

        public void ReleaseAll()
        {
            _cache.Clear();
            _pending.Clear();
        }

        private sealed class CacheEntry
        {
            public AudioClip Clip = default!;
            public int RefCount;
        }
    }
}
#nullable disable
