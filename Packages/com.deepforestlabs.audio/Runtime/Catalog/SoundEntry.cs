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

        [Header("Voice Management")]
        [SerializeField] private StealPolicy _stealPolicy = StealPolicy.StealOldest;
        [Tooltip("Seconds of fade before the oldest instance is reclaimed (SoftSteal only).")]
        [SerializeField] private float _stealFadeDuration = 0.08f;
        [Tooltip("Minimum seconds between successive plays of this key. 0 = no cooldown.")]
        [SerializeField] private float _cooldown;

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
        public StealPolicy StealPolicy => _stealPolicy;
        public float StealFadeDuration => _stealFadeDuration;
        public float Cooldown => _cooldown;
    }
}
#nullable disable
