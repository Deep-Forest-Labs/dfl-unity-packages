#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs
{
    [Serializable]
    public struct GameObjectManagerOptions
    {
        [SerializeField] private GameObjectManagerDownloadOptions _downloadOptions;
        [SerializeField] private GameObjectManagerLoadOptions _loadOptions;
        [SerializeField] private bool _prewarm;
        [SerializeField] private bool _useCreate;
        [SerializeField] private bool _autoUnload;
        [SerializeField] private int _prewarmCount;
        [SerializeField] private int _maxPoolSize;

        public GameObjectManagerDownloadOptions DownloadOptions => _downloadOptions;

        public GameObjectManagerLoadOptions LoadOptions => _loadOptions;

        public bool Prewarm => _prewarm;
        
        public bool UseCreate => _useCreate;
        public bool AutoUnload => _autoUnload;

        /// <summary>
        /// Number of idle instances to pre-instantiate after prefab load.
        /// When 0 and Prewarm is true, defaults to 1 for backward compatibility.
        /// </summary>
        public int PrewarmCount => _prewarmCount;

        /// <summary>
        /// Maximum idle instances retained in the pool. 0 means unlimited.
        /// When a check-in would exceed this, the instance is destroyed instead of pooled.
        /// </summary>
        public int MaxPoolSize => _maxPoolSize;

        public GameObjectManagerOptions(
            GameObjectManagerDownloadOptions downloadOptions,
            GameObjectManagerLoadOptions loadOptions,
            bool prewarm,
            bool useCreate,
            bool autoUnload,
            int prewarmCount = 0,
            int maxPoolSize = 0)
        {
            _downloadOptions = downloadOptions;
            _loadOptions = loadOptions;
            _prewarm = prewarm;
            _useCreate = useCreate;
            _autoUnload = autoUnload;
            _prewarmCount = prewarmCount;
            _maxPoolSize = maxPoolSize;
        }

        public int ResolvePrewarmCount()
        {
            if (_prewarmCount > 0) return _prewarmCount;
            return _prewarm ? 1 : 0;
        }

        public static readonly GameObjectManagerOptions Required = new(GameObjectManagerDownloadOptions.Required, GameObjectManagerLoadOptions.Required, true, false, false);
        public static readonly GameObjectManagerOptions Background = new(GameObjectManagerDownloadOptions.Background, GameObjectManagerLoadOptions.Background, true, false, false);
        public static readonly GameObjectManagerOptions OnDemand = new(GameObjectManagerDownloadOptions.OnDemand, GameObjectManagerLoadOptions.OnDemand, false, false, false);
        public static readonly GameObjectManagerOptions OnDemandWithAutoUnload = new(GameObjectManagerDownloadOptions.OnDemand, GameObjectManagerLoadOptions.OnDemand, false, false, true);

        // Create mode version of Required
        public static readonly GameObjectManagerOptions LegacyInstancePool = new(GameObjectManagerDownloadOptions.Required, GameObjectManagerLoadOptions.Required, false, true, false);
        // Create mode version of OnDemandWithAutoUnload
        public static readonly GameObjectManagerOptions LegacyManagedInstancePool = new(GameObjectManagerDownloadOptions.OnDemand, GameObjectManagerLoadOptions.OnDemand, false, true, true);
        public static readonly GameObjectManagerOptions LegacyManagedInstancePoolWithBackgroundDownload = new(GameObjectManagerDownloadOptions.Background, GameObjectManagerLoadOptions.OnDemand, false, true, true);

        public static GameObjectManagerOptions RequiredWithPool(int prewarmCount, int maxPoolSize = 0) =>
            new(GameObjectManagerDownloadOptions.Required, GameObjectManagerLoadOptions.Required,
                false, false, false, prewarmCount, maxPoolSize);
    }
}
#nullable disable