#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IPushNotificationService
    {
        bool IsAvailable { get; }

        UniTask<PushPermissionResult> RequestPermission(CancellationToken token);

        UniTask Register(CancellationToken token);
    }
}
#nullable disable
