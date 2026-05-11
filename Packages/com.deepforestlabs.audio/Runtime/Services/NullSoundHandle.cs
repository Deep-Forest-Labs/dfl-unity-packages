#nullable enable
namespace DeepForestLabs.Audio
{
    internal sealed class NullSoundHandle : ISoundHandle
    {
        public SoundGroupId Group { get; }
        public SoundState State => SoundState.Stopped;
        public float Volume { get; set; }
        public float Pan { get; set; }
        public bool IsMuted { get; set; }

        public NullSoundHandle(SoundGroupId group)
        {
            Group = group;
        }

        public void Pause() { }
        public void Resume() { }
        public void Stop() { }
        public void FadeOut(float duration) { }
    }
}
#nullable disable
