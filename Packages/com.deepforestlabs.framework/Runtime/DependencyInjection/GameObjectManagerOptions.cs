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

        public GameObjectManagerDownloadOptions DownloadOptions => _downloadOptions;

        public GameObjectManagerLoadOptions LoadOptions => _loadOptions;

        public bool Prewarm => _prewarm;
        
        public bool UseCreate => _useCreate;
        public bool AutoUnload => _autoUnload;

        public GameObjectManagerOptions(GameObjectManagerDownloadOptions downloadOptions, GameObjectManagerLoadOptions loadOptions, bool prewarm, bool useCreate, bool autoUnload)
        {
            _downloadOptions = downloadOptions;
            _loadOptions = loadOptions;
            _prewarm = prewarm;
            _useCreate = useCreate;
            _autoUnload = autoUnload;
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
    }
}
#nullable disable