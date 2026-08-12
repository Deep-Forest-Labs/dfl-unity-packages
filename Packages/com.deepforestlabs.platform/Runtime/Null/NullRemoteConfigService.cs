#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullRemoteConfigService : IRemoteConfigService
    {
        public UniTask<RemoteConfigRefreshStatus> Refresh(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullRemoteConfigService) + "." + nameof(Refresh), "skipped");
            return UniTask.FromResult(RemoteConfigRefreshStatus.Skipped);
        }

        public bool TryGetString(string key, out string value)
        {
            NullPlatformLog.Once(nameof(NullRemoteConfigService) + ".TryGet", "no remote values");
            value = string.Empty;
            return false;
        }

        public bool TryGetInt(string key, out int value)
        {
            value = 0;
            return false;
        }

        public bool TryGetFloat(string key, out float value)
        {
            value = 0f;
            return false;
        }

        public bool TryGetBool(string key, out bool value)
        {
            value = false;
            return false;
        }
    }
}
#nullable disable
