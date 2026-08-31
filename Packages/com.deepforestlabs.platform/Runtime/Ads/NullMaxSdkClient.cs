#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullMaxSdkClient : IMaxSdkClient
    {
        public bool IsPresent => false;

        public event Action<string>? RewardedLoaded
        {
            add { }
            remove { }
        }

        public event Action<string>? RewardedLoadFailed
        {
            add { }
            remove { }
        }

        public event Action<string>? RewardedDisplayed
        {
            add { }
            remove { }
        }

        public event Action<string>? RewardedDisplayFailed
        {
            add { }
            remove { }
        }

        public event Action<string>? RewardedHidden
        {
            add { }
            remove { }
        }

        public event Action<string>? RewardedReceivedReward
        {
            add { }
            remove { }
        }

        public void SetSdkKey(string sdkKey) =>
            NullPlatformLog.Once(nameof(NullMaxSdkClient) + "." + nameof(SetSdkKey), "no MAX SDK");

        public void SetHasUserConsent(bool hasConsent) { }

        public void SetDoNotSell(bool doNotSell) { }

        public UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return UniTask.CompletedTask;
        }

        public void LoadRewarded(string adUnitId) { }

        public bool IsRewardedReady(string adUnitId) => false;

        public void ShowRewarded(string adUnitId) { }
    }
}
#nullable disable
