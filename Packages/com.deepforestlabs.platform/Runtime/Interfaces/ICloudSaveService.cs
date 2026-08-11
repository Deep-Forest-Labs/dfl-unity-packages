#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Schema-agnostic blob/key cloud store. Game save schemas stay in game code (e.g. ISaveService).
    /// </summary>
    [Preserve]
    [RequireImplementors]
    public interface ICloudSaveService
    {
        bool IsAvailable { get; }

        UniTask<CloudSaveLoadResult> Load(string key, CancellationToken token);

        UniTask<CloudSaveWriteResult> Save(string key, string data, CancellationToken token);

        UniTask<CloudSaveWriteResult> Delete(string key, CancellationToken token);
    }
}
#nullable disable
