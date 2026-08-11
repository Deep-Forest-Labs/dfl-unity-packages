#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullPushNotificationService : IPushNotificationService
    {
        public bool IsAvailable => false;

        public UniTask<PushPermissionResult> RequestPermission(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullPushNotificationService) + "." + nameof(RequestPermission), "unavailable");
            return UniTask.FromResult(PushPermissionResult.Unavailable);
        }

        public UniTask Register(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullPushNotificationService) + "." + nameof(Register), "no-op");
            return UniTask.CompletedTask;
        }
    }
}
#nullable disable
