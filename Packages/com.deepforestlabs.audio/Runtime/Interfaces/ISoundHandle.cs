#nullable enable
namespace DeepForestLabs.Audio
{
    public interface ISoundHandle
    {
        SoundGroupId Group { get; }
        SoundState State { get; }
        float Volume { get; set; }
        float Pan { get; set; }
        bool IsMuted { get; set; }
        void Pause();
        void Resume();
        void Stop();
        void FadeOut(float duration);
    }
}
#nullable disable
