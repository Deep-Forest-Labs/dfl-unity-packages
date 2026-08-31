#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Thin MAX SDK façade. Platform owns orchestration; the game (or a future package) supplies
    /// a real implementation when AppLovin MAX is installed (<c>DFL_MAX_SDK</c>).
    /// </summary>
    [Preserve]
    [RequireImplementors]
    public interface IMaxSdkClient
    {
        bool IsPresent { get; }

        void SetSdkKey(string sdkKey);

        void SetHasUserConsent(bool hasConsent);

        void SetDoNotSell(bool doNotSell);

        UniTask Initialize(CancellationToken token);

        void LoadRewarded(string adUnitId);

        bool IsRewardedReady(string adUnitId);

        void ShowRewarded(string adUnitId);

        event Action<string>? RewardedLoaded;
        event Action<string>? RewardedLoadFailed;
        event Action<string>? RewardedDisplayed;
        event Action<string>? RewardedDisplayFailed;
        event Action<string>? RewardedHidden;
        event Action<string>? RewardedReceivedReward;
    }
}
#nullable disable
