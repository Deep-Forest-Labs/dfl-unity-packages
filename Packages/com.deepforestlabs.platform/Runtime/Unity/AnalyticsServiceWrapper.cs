#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs;
using UnityEngine.UnityConsent;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// UGS analytics bootstrap. Collection stays disabled until E2 enables consent + StartDataCollection.
    /// </summary>
    public sealed class AnalyticsServiceWrapper : IInitializable
    {
        [Dependency] private readonly UnityServicesWrapper _unityServicesWrapper = default!;

        private static bool _isInitialized;

        private readonly bool _analyticsEnabled;

        public AnalyticsServiceWrapper()
        {
            //TODO E2 — wire to BuildSettings / consent; currently always off
            _analyticsEnabled = false;
        }

        public async UniTask Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_isInitialized)
            {
                return;
            }

            await UniTask.WaitUntil(() => _unityServicesWrapper.IsInitialized, cancellationToken: token);

            if (_analyticsEnabled)
            {
                EndUserConsent.SetConsentState(new ConsentState());
            }

            _isInitialized = true;
        }
    }
}
#nullable disable
