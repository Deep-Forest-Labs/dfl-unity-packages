#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Editor / non-ATT default: tracking not required; does not block ads/analytics gates.
    /// </summary>
    public sealed class NullConsentService : IConsentService
    {
        public ConsentStatus Status => ConsentStatus.NotRequired;

        public bool AllowsAnalytics => true;

        public bool AllowsPersonalizedAds => false;

        public UniTask<ConsentStatus> RequestTrackingAuthorization(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(
                nameof(NullConsentService) + "." + nameof(RequestTrackingAuthorization),
                "returning NotRequired");
            return UniTask.FromResult(ConsentStatus.NotRequired);
        }
    }
}
#nullable disable
