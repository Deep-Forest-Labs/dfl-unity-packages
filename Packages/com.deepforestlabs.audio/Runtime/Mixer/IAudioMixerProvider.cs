#nullable enable
using UnityEngine.Audio;

namespace DeepForestLabs.Audio
{
    public interface IAudioMixerProvider
    {
        AudioMixerGroup? GetGroup(SoundGroupId groupId);
        float GetVolume(SoundGroupId groupId);
        void SetVolume(SoundGroupId groupId, float linear);
        bool GetMute(SoundGroupId groupId);
        void SetMute(SoundGroupId groupId, bool muted);
        float GetMasterVolume();
        void SetMasterVolume(float linear);
        bool GetMasterMute();
        void SetMasterMute(bool muted);
    }
}
#nullable disable
