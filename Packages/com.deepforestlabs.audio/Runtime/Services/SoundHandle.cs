#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    internal sealed class SoundHandle : ISoundHandle
    {
        private readonly PooledAudioSource _source;
        private readonly Action<SoundHandle> _onStop;
        private float _volume;
        private bool _isMuted;

        public SoundGroupId Group { get; }
        public SoundState State { get; private set; }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp01(value);
                if (!_isMuted) _source.Source.volume = _volume;
            }
        }

        public float Pan
        {
            get => _source.Source.panStereo;
            set => _source.Source.panStereo = Mathf.Clamp(value, -1f, 1f);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                _source.Source.volume = _isMuted ? 0f : _volume;
            }
        }

        public SoundHandle(PooledAudioSource source, SoundGroupId group, float volume, Action<SoundHandle> onStop)
        {
            _source = source;
            _onStop = onStop;
            _volume = volume;
            Group = group;
            State = SoundState.Playing;
        }

        public void Pause()
        {
            if (State != SoundState.Playing) return;
            _source.Pause();
            State = SoundState.Paused;
        }

        public void Resume()
        {
            if (State != SoundState.Paused) return;
            _source.UnPause();
            State = SoundState.Playing;
        }

        public void Stop()
        {
            if (State == SoundState.Stopped) return;
            State = SoundState.Stopped;
            _source.Stop();
            _onStop(this);
        }

        public void FadeOut(float duration)
        {
            if (State == SoundState.Stopped) return;
            FadeOutAsync(duration, default).Forget();
        }

        public void SetWorldPosition(Vector3 position)
        {
            _source.SetPosition(position);
        }

        internal void MarkStopped()
        {
            State = SoundState.Stopped;
        }

        private async UniTaskVoid FadeOutAsync(float duration, CancellationToken token)
        {
            float startVolume = _source.Source.volume;
            float elapsed = 0f;

            while (elapsed < duration && State != SoundState.Stopped)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _source.Source.volume = Mathf.Lerp(startVolume, 0f, t);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (State != SoundState.Stopped)
            {
                Stop();
            }
        }
    }
}
#nullable disable
