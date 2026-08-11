#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullAdService : IAdService
    {
        public bool IsAvailable => false;

        public bool IsRewardedReady(string placementId) => false;

        public UniTask<RewardedAdResult> ShowRewarded(string placementId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(
                nameof(NullAdService) + "." + nameof(ShowRewarded),
                "unavailable placement '{0}'",
                placementId);
            return UniTask.FromResult(RewardedAdResult.Unavailable);
        }

        public bool IsInterstitialReady(string placementId) => false;

        public UniTask<InterstitialAdResult> ShowInterstitial(string placementId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(
                nameof(NullAdService) + "." + nameof(ShowInterstitial),
                "unavailable placement '{0}'",
                placementId);
            return UniTask.FromResult(InterstitialAdResult.Unavailable);
        }
    }
}
#nullable disable
