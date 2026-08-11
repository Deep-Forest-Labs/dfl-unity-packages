#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IIapService
    {
        bool IsAvailable { get; }

        bool HasEntitlement(string productId);

        UniTask<PurchaseResult> Purchase(string productId, CancellationToken token);

        UniTask<RestorePurchasesResult> RestorePurchases(CancellationToken token);
    }
}
#nullable disable
