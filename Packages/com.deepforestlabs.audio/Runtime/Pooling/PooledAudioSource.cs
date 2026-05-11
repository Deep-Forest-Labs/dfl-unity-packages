#nullable enable
using UnityEngine;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    internal sealed class PooledAudioSource
    {
        public AudioSource Source { get; }
        public GameObject GameObject { get; }
        public bool IsActive { get; set; }
        public AudioClip? AssignedClip { get; set; }
        public SoundGroupId Group { get; set; }

        public PooledAudioSource(Transform parent)
        {
            GameObject = new GameObject("PooledAudioSource");
            GameObject.transform.SetParent(parent, false);
            Source = GameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            Source.spatialBlend = 0f;
            IsActive = false;
        }

        public void Configure(AudioClip clip, AudioMixerGroup? mixerGroup, float volume, float pan, bool loop)
        {
            AssignedClip = clip;
            Source.clip = clip;
            Source.outputAudioMixerGroup = mixerGroup;
            Source.volume = volume;
            Source.panStereo = pan;
            Source.loop = loop;
        }

        public void Play()
        {
            IsActive = true;
            Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
            Reset();
        }

        public void Pause() => Source.Pause();
        public void UnPause() => Source.UnPause();

        public void Reset()
        {
            IsActive = false;
            Source.Stop();
            Source.clip = null;
            Source.outputAudioMixerGroup = null;
            Source.volume = 1f;
            Source.panStereo = 0f;
            Source.loop = false;
            AssignedClip = null;
            Group = default;
        }
    }
}
#nullable disable
