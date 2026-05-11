#nullable enable
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

        public async UniTask<AudioClip> Load(AudioClipAssetRef assetRef, CancellationToken token)
        {
            if (_cache.TryGetValue(assetRef, out CacheEntry entry))
            {
                entry.RefCount++;
                return entry.Clip;
            }

            AudioClip clip = await _container.Checkout(assetRef, token);
            _cache[assetRef] = new CacheEntry { Clip = clip, RefCount = 1 };
            return clip;
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
        }

        private sealed class CacheEntry
        {
            public AudioClip Clip = default!;
            public int RefCount;
        }
    }
}
#nullable disable
