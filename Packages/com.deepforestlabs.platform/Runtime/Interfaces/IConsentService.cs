#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IConsentService
    {
        ConsentStatus Status { get; }

        bool AllowsAnalytics { get; }

        bool AllowsPersonalizedAds { get; }

        UniTask<ConsentStatus> RequestTrackingAuthorization(CancellationToken token);
    }
}
#nullable disable
