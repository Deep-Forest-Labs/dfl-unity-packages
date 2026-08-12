#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace DeepForestLabs.Platform
{
    /// <summary>
    /// Cold-start boot payload (user stub + economy). Local/in-memory now; swap to HTTP <c>/boot</c> later.
    /// </summary>
    [Preserve]
    [RequireImplementors]
    public interface IBootConfigClient
    {
        UniTask<BootSnapshot> Fetch(CancellationToken token);
    }

    public readonly record struct BootSnapshot(
        string PlayerId,
        string EconomyId,
        string EconomyRevision,
        string EconomySource);
}
#nullable disable
