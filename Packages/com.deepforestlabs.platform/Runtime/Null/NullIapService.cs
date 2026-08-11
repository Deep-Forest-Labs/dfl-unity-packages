#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullIapService : IIapService
    {
        public bool IsAvailable => false;

        public bool HasEntitlement(string productId) => false;

        public UniTask<PurchaseResult> Purchase(string productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(
                nameof(NullIapService) + "." + nameof(Purchase),
                "unavailable product '{0}'",
                productId);
            return UniTask.FromResult(new PurchaseResult(PurchaseResultStatus.Unavailable, productId));
        }

        public UniTask<RestorePurchasesResult> RestorePurchases(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullIapService) + "." + nameof(RestorePurchases), "unavailable");
            return UniTask.FromResult(RestorePurchasesResult.Unavailable);
        }
    }
}
#nullable disable
