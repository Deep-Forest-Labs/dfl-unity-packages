#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IRemoteConfigService
    {
        UniTask Refresh(CancellationToken token);

        bool TryGetString(string key, out string value);

        bool TryGetInt(string key, out int value);

        bool TryGetFloat(string key, out float value);

        bool TryGetBool(string key, out bool value);
    }
}
#nullable disable
