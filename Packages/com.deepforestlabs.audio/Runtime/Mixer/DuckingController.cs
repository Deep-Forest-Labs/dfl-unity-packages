#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs.Audio
{
    internal sealed class DuckingController
    {
        [Dependency] private readonly IAudioMixerProvider _mixerProvider = null!;
        [Dependency] private readonly AudioMixerConfig _config = default!;
        [Dependency] private readonly CancellationToken _scope = default!;

        private readonly Dictionary<SoundGroupId, DuckingState> _states = new();

        public void ApplyDucking(DuckingProfile profile, ISoundHandle triggerHandle)
        {
            foreach (SoundGroupId target in profile.TargetGroups)
            {
                if (!_states.TryGetValue(target, out DuckingState state))
                {
                    state = new DuckingState();
                    _states[target] = state;
                }

                state.ActiveRequests.Add(new DuckingRequest(profile, triggerHandle));
                UpdateDuckLevel(target, state, 0f);
            }
        }

        public void ReleaseDucking(ISoundHandle triggerHandle)
        {
            foreach (KeyValuePair<SoundGroupId, DuckingState> kvp in _states)
            {
                DuckingState state = kvp.Value;
                float release = -1f;
                for (int i = state.ActiveRequests.Count - 1; i >= 0; i--)
                {
                    DuckingRequest request = state.ActiveRequests[i];
                    if (request.TriggerHandle == triggerHandle)
                    {
                        release = Mathf.Max(release, request.Profile.ReleaseTime);
                        state.ActiveRequests.RemoveAt(i);
                    }
                }

                if (release >= 0f)
                    UpdateDuckLevel(kvp.Key, state, release);
            }
        }

        public void ReleaseAll()
        {
            foreach (KeyValuePair<SoundGroupId, DuckingState> kvp in _states)
            {
                kvp.Value.ActiveRequests.Clear();
                RestoreVolume(kvp.Key, kvp.Value);
            }
            _states.Clear();
        }

        public async UniTask FadeToTarget(SoundGroupId group, float targetDb, float duration, CancellationToken token)
        {
            if (!_config.TryGetMapping(group, out AudioMixerConfig.GroupMapping mapping))
                return;

            _config.Mixer.GetFloat(mapping.VolumeParam, out float currentDb);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float db = Mathf.Lerp(currentDb, targetDb, t);
                _config.Mixer.SetFloat(mapping.VolumeParam, db);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _config.Mixer.SetFloat(mapping.VolumeParam, targetDb);
        }

        private void UpdateDuckLevel(SoundGroupId target, DuckingState state, float releaseSeconds)
        {
            if (!_config.TryGetMapping(target, out AudioMixerConfig.GroupMapping mapping))
                return;

            if (!state.IsDucked)
            {
                if (state.ActiveRequests.Count == 0)
                    return;
                _config.Mixer.GetFloat(mapping.VolumeParam, out float currentDb);
                state.OriginalDb = currentDb;
                state.OffsetDb = 0f;
                state.IsDucked = true;
            }

            state.Profiles.Clear();
            foreach (DuckingRequest request in state.ActiveRequests)
                state.Profiles.Add(request.Profile);

            DuckLevel level = DuckLevel.Resolve(state.Profiles, state.OffsetDb, releaseSeconds);
            state.OffsetDb = level.OffsetDb;
            StartFade(mapping.VolumeParam, state, level.FadeSeconds);
        }

        private void StartFade(string param, DuckingState state, float seconds)
        {
            CancelFade(state);

            float targetDb = state.OriginalDb + state.OffsetDb;
            if (seconds <= 0f)
            {
                SetDuckedVolume(param, state, targetDb);
                return;
            }

            state.Fade = CancellationTokenSource.CreateLinkedTokenSource(_scope);
            Fade(param, state, targetDb, seconds, state.Fade.Token).Forget();
        }

        private async UniTaskVoid Fade(string param, DuckingState state, float targetDb, float seconds, CancellationToken token)
        {
            _config.Mixer.GetFloat(param, out float startDb);
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow())
                    return;
                elapsed += Time.unscaledDeltaTime;
                _config.Mixer.SetFloat(param, Mathf.Lerp(startDb, targetDb, Mathf.Clamp01(elapsed / seconds)));
            }

            SetDuckedVolume(param, state, targetDb);
        }

        private void SetDuckedVolume(string param, DuckingState state, float db)
        {
            _config.Mixer.SetFloat(param, db);
            if (state.ActiveRequests.Count == 0)
                state.IsDucked = false;
        }

        private void RestoreVolume(SoundGroupId target, DuckingState state)
        {
            CancelFade(state);
            if (!state.IsDucked) return;

            if (_config.TryGetMapping(target, out AudioMixerConfig.GroupMapping mapping))
            {
                _config.Mixer.SetFloat(mapping.VolumeParam, state.OriginalDb);
            }

            state.IsDucked = false;
        }

        private static void CancelFade(DuckingState state)
        {
            if (state.Fade == null) return;
            state.Fade.Cancel();
            state.Fade.Dispose();
            state.Fade = null;
        }

        private sealed class DuckingState
        {
            public readonly List<DuckingRequest> ActiveRequests = new();
            public readonly List<DuckingProfile> Profiles = new();
            public float OriginalDb;
            public float OffsetDb;
            public bool IsDucked;
            public CancellationTokenSource? Fade;
        }

        private readonly struct DuckingRequest
        {
            public readonly DuckingProfile Profile;
            public readonly ISoundHandle TriggerHandle;

            public DuckingRequest(DuckingProfile profile, ISoundHandle triggerHandle)
            {
                Profile = profile;
                TriggerHandle = triggerHandle;
            }
        }
    }
}
#nullable disable
