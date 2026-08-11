#nullable enable
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// iOS App Tracking Transparency. Editor / Android / pre-iOS-14 → <see cref="ConsentStatus.NotRequired"/>.
    /// Analytics remains allowed when ATT is denied; personalized ads only when authorized.
    /// </summary>
    [Preserve]
    public sealed class AttConsentService : IConsentService
    {
        private ConsentStatus _status = ResolveInitialStatus();

        public ConsentStatus Status => _status;

        public bool AllowsAnalytics => true;

        public bool AllowsPersonalizedAds => _status == ConsentStatus.Authorized;

        public async UniTask<ConsentStatus> RequestTrackingAuthorization(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

#if UNITY_IOS && !UNITY_EDITOR
            if (_status is ConsentStatus.Authorized or ConsentStatus.Denied or ConsentStatus.Restricted)
            {
                return _status;
            }

            var tcs = new UniTaskCompletionSource<ConsentStatus>();
            using (token.Register(() => tcs.TrySetCanceled(token)))
            {
                s_pending = tcs;
                DFL_RequestTrackingAuthorization(OnNativeAttResult);
                _status = await tcs.Task;
                s_pending = null;
            }

            return _status;
#else
            _status = ConsentStatus.NotRequired;
            await UniTask.CompletedTask;
            return _status;
#endif
        }

        private static ConsentStatus ResolveInitialStatus()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return MapNativeStatus(DFL_GetTrackingAuthorizationStatus());
#else
            return ConsentStatus.NotRequired;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private static UniTaskCompletionSource<ConsentStatus>? s_pending;

        private delegate void AttCallback(int status);

        [DllImport("__Internal")]
        private static extern void DFL_RequestTrackingAuthorization(AttCallback callback);

        [DllImport("__Internal")]
        private static extern int DFL_GetTrackingAuthorizationStatus();

        [MonoPInvokeCallback(typeof(AttCallback))]
        private static void OnNativeAttResult(int status)
        {
            s_pending?.TrySetResult(MapNativeStatus(status));
        }

        private static ConsentStatus MapNativeStatus(int status) =>
            status switch
            {
                0 => ConsentStatus.Unknown,      // NotDetermined
                1 => ConsentStatus.Restricted,   // Restricted
                2 => ConsentStatus.Denied,       // Denied
                3 => ConsentStatus.Authorized,   // Authorized
                _ => ConsentStatus.Unknown
            };
#endif
    }
}
#nullable disable
