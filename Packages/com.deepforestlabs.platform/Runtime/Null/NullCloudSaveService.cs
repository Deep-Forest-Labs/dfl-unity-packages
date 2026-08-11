#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullCloudSaveService : ICloudSaveService
    {
        public bool IsAvailable => false;

        public UniTask<CloudSaveLoadResult> Load(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullCloudSaveService) + "." + nameof(Load), "unavailable key '{0}'", key);
            return UniTask.FromResult(new CloudSaveLoadResult(CloudSaveStatus.Unavailable, null));
        }

        public UniTask<CloudSaveWriteResult> Save(string key, string data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullCloudSaveService) + "." + nameof(Save), "unavailable key '{0}'", key);
            return UniTask.FromResult(new CloudSaveWriteResult(CloudSaveStatus.Unavailable));
        }

        public UniTask<CloudSaveWriteResult> Delete(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullCloudSaveService) + "." + nameof(Delete), "unavailable key '{0}'", key);
            return UniTask.FromResult(new CloudSaveWriteResult(CloudSaveStatus.Unavailable));
        }
    }
}
#nullable disable
