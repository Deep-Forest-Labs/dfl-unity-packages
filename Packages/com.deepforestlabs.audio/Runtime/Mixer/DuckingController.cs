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

        private readonly Dictionary<SoundGroupId, DuckingState> _states = new();

        public void ApplyDucking(DuckingProfile profile, ISoundHandle triggerHandle)
        {
            SoundGroupId target = profile.TargetGroup;

            if (!_states.TryGetValue(target, out DuckingState state))
            {
                state = new DuckingState();
                _states[target] = state;
            }

            state.ActiveRequests.Add(new DuckingRequest(profile, triggerHandle));
            UpdateDuckLevel(target, state);
        }

        public void ReleaseDucking(ISoundHandle triggerHandle)
        {
            foreach (KeyValuePair<SoundGroupId, DuckingState> kvp in _states)
            {
                DuckingState state = kvp.Value;
                for (int i = state.ActiveRequests.Count - 1; i >= 0; i--)
                {
                    if (state.ActiveRequests[i].TriggerHandle == triggerHandle)
                    {
                        state.ActiveRequests.RemoveAt(i);
                    }
                }
                UpdateDuckLevel(kvp.Key, state);
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

        private void UpdateDuckLevel(SoundGroupId target, DuckingState state)
        {
            if (state.ActiveRequests.Count == 0)
            {
                RestoreVolume(target, state);
                return;
            }

            float deepestDuck = 0f;
            float fastestAttack = float.MaxValue;
            foreach (DuckingRequest request in state.ActiveRequests)
            {
                if (request.Profile.VolumeReductionDb < deepestDuck)
                    deepestDuck = request.Profile.VolumeReductionDb;
                if (request.Profile.AttackTime < fastestAttack)
                    fastestAttack = request.Profile.AttackTime;
            }

            if (!_config.TryGetMapping(target, out AudioMixerConfig.GroupMapping mapping))
                return;

            if (!state.IsDucked)
            {
                _config.Mixer.GetFloat(mapping.VolumeParam, out float currentDb);
                state.OriginalDb = currentDb;
                state.IsDucked = true;
            }

            float targetDb = state.OriginalDb + deepestDuck;
            _config.Mixer.SetFloat(mapping.VolumeParam, targetDb);
        }

        private void RestoreVolume(SoundGroupId target, DuckingState state)
        {
            if (!state.IsDucked) return;

            if (_config.TryGetMapping(target, out AudioMixerConfig.GroupMapping mapping))
            {
                _config.Mixer.SetFloat(mapping.VolumeParam, state.OriginalDb);
            }

            state.IsDucked = false;
        }

        private sealed class DuckingState
        {
            public readonly List<DuckingRequest> ActiveRequests = new();
            public float OriginalDb;
            public bool IsDucked;
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
