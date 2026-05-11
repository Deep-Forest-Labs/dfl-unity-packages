#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.Audio
{
    public interface IAudioService
    {
        UniTask<ISoundHandle> PlaySfx(AudioClipAssetRef clip, SoundParams? options = null, CancellationToken token = default);
        UniTask<ISoundHandle> PlayBgm(AudioClipAssetRef clip, SoundParams? options = null, CancellationToken token = default);

        UniTask<ISoundHandle> PlaySfx(string key, SoundParams? options = null, CancellationToken token = default);
        UniTask<ISoundHandle> PlayBgm(string key, SoundParams? options = null, CancellationToken token = default);

        void StopGroup(SoundGroupId group);
        void StopAll();

        bool IsMuted { get; set; }

        float MasterVolume { get; set; }
        float GetGroupVolume(SoundGroupId group);
        void SetGroupVolume(SoundGroupId group, float volume);
        bool GetGroupMute(SoundGroupId group);
        void SetGroupMute(SoundGroupId group, bool muted);

        UniTask Preload(AudioClipAssetRef clip, CancellationToken token);
        UniTask PreloadCatalog(CancellationToken token);
        void Unload(AudioClipAssetRef clip);
    }
}
#nullable disable
