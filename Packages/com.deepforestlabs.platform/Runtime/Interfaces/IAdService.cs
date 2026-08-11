#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IAdService
    {
        bool IsAvailable { get; }

        bool IsRewardedReady(string placementId);

        UniTask<RewardedAdResult> ShowRewarded(string placementId, CancellationToken token);

        bool IsInterstitialReady(string placementId);

        UniTask<InterstitialAdResult> ShowInterstitial(string placementId, CancellationToken token);
    }
}
#nullable disable
