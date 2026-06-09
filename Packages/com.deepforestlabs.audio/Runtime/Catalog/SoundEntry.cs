#nullable enable
using System;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private string _key = string.Empty;
        [SerializeField] private AudioClipAssetRef _clip = default!;
        [SerializeField] private SoundGroupId _group = SoundGroupId.Sfx;
        [SerializeField] [Range(0f, 1f)] private float _defaultVolume = 1f;
        [SerializeField] [Range(-1f, 1f)] private float _defaultPan;
        [SerializeField] private int _maxInstances;
        [SerializeField] private int _poolPrewarm;
        [SerializeField] private bool _preload;
        [SerializeField] private DuckingProfile? _ducking;

        [Header("Spatial")]
        [SerializeField] [Range(0f, 1f)] private float _spatialBlend;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 40f;
        [SerializeField] private bool _spatialize;

        public string Key => _key;
        public AudioClipAssetRef Clip => _clip;
        public SoundGroupId Group => _group;
        public float DefaultVolume => _defaultVolume;
        public float DefaultPan => _defaultPan;
        public int MaxInstances => _maxInstances;
        public int PoolPrewarm => _poolPrewarm;
        public bool Preload => _preload;
        public DuckingProfile? Ducking => _ducking;
        public float SpatialBlend => _spatialBlend;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public bool Spatialize => _spatialize;
    }
}
#nullable disable
