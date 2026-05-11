#nullable enable
using UnityEngine;

namespace DeepForestLabs.Audio
{
    [CreateAssetMenu(fileName = "DuckingProfile", menuName = "Audio/Ducking Profile")]
    public sealed class DuckingProfile : ScriptableObject
    {
        [SerializeField] private SoundGroupId _targetGroup = SoundGroupId.Bgm;
        [SerializeField] [Range(-80f, 0f)] private float _volumeReductionDb = -12f;
        [SerializeField] [Range(0f, 2f)] private float _attackTime = 0.1f;
        [SerializeField] [Range(0f, 5f)] private float _releaseTime = 0.5f;

        public SoundGroupId TargetGroup => _targetGroup;
        public float VolumeReductionDb => _volumeReductionDb;
        public float AttackTime => _attackTime;
        public float ReleaseTime => _releaseTime;
    }
}
#nullable disable
