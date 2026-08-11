#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IAnalyticsService
    {
        bool IsEnabled { get; }

        void Track(string eventName, IReadOnlyDictionary<string, object?>? parameters = null);

        UniTask Flush(CancellationToken token);
    }
}
#nullable disable
