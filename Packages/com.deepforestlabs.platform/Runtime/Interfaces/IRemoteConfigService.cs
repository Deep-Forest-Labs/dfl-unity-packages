#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    public enum RemoteConfigRefreshStatus
    {
        Succeeded = 0,
        Failed = 1,
        Skipped = 2
    }

    [Preserve]
    [RequireImplementors]
    public interface IRemoteConfigService
    {
        /// <summary>
        /// Performs a fresh remote fetch when available. Cache-only activate is not success for Firebase.
        /// </summary>
        UniTask<RemoteConfigRefreshStatus> Refresh(CancellationToken token);

        bool TryGetString(string key, out string value);

        bool TryGetInt(string key, out int value);

        bool TryGetFloat(string key, out float value);

        bool TryGetBool(string key, out bool value);
    }
}
#nullable disable
