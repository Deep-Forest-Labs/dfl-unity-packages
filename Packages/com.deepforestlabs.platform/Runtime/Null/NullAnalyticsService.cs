#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public bool IsEnabled => false;

        public void Track(string eventName, IReadOnlyDictionary<string, object?>? parameters = null)
        {
            NullPlatformLog.Once(
                nameof(NullAnalyticsService) + "." + nameof(Track),
                "dropped event '{0}'",
                eventName);
        }

        public UniTask Flush(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullAnalyticsService) + "." + nameof(Flush), "no-op");
            return UniTask.CompletedTask;
        }
    }
}
#nullable disable
