#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    [Preserve]
    [RequireImplementors]
    public interface IAccountService
    {
        string PlayerId { get; }

        bool IsLinked { get; }

        UniTask EnsureAnonymousAsync(CancellationToken token);
    }
}
#nullable disable
