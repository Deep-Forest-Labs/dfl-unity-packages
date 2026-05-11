#nullable enable
using System.Collections.Generic;
using DeepForestLabs.Logger;
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    internal sealed class AudioMixerProvider : IAudioMixerProvider
    {
        [Dependency] private readonly AudioMixerConfig _config = default!;

        private readonly Dictionary<string, bool> _muteStates = new();
        private bool _masterMuted;

        public AudioMixerGroup? GetGroup(SoundGroupId groupId)
        {
            if (_config.TryGetMapping(groupId, out AudioMixerConfig.GroupMapping mapping))
            {
                return mapping.MixerGroup;
            }
            Log.Warning("No mixer group mapping for '{0}'", groupId.Name);
            return null;
        }

        public float GetVolume(SoundGroupId groupId)
        {
            if (!_config.TryGetMapping(groupId, out AudioMixerConfig.GroupMapping mapping))
                return 1f;

            if (_config.Mixer.GetFloat(mapping.VolumeParam, out float db))
                return AudioMixerUtils.DecibelsToLinear(db);

            return 1f;
        }

        public void SetVolume(SoundGroupId groupId, float linear)
        {
            if (!_config.TryGetMapping(groupId, out AudioMixerConfig.GroupMapping mapping))
                return;

            float db = AudioMixerUtils.LinearToDecibels(linear);
            _config.Mixer.SetFloat(mapping.VolumeParam, db);
        }

        public bool GetMute(SoundGroupId groupId)
        {
            return _muteStates.TryGetValue(groupId.Name, out bool muted) && muted;
        }

        public void SetMute(SoundGroupId groupId, bool muted)
        {
            _muteStates[groupId.Name] = muted;

            if (!_config.TryGetMapping(groupId, out AudioMixerConfig.GroupMapping mapping))
                return;

            if (muted)
            {
                _config.Mixer.SetFloat(mapping.VolumeParam, -80f);
            }
            else
            {
                float current = GetVolume(groupId);
                SetVolume(groupId, current > 0f ? current : 1f);
            }
        }

        public float GetMasterVolume()
        {
            if (_config.Mixer.GetFloat(_config.MasterVolumeParam, out float db))
                return AudioMixerUtils.DecibelsToLinear(db);
            return 1f;
        }

        public void SetMasterVolume(float linear)
        {
            float db = AudioMixerUtils.LinearToDecibels(linear);
            _config.Mixer.SetFloat(_config.MasterVolumeParam, db);
        }

        public bool GetMasterMute() => _masterMuted;

        public void SetMasterMute(bool muted)
        {
            _masterMuted = muted;
            if (muted)
            {
                _config.Mixer.SetFloat(_config.MasterVolumeParam, -80f);
            }
            else
            {
                float current = GetMasterVolume();
                SetMasterVolume(current > 0f ? current : 1f);
            }
        }
    }
}
#nullable disable
