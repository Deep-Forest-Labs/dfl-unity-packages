#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs;
using DeepForestLabs.Platform.Internal;

namespace DeepForestLabs.Platform
{
    public sealed class NullBootConfigClient : IBootConfigClient
    {
        [Dependency] private readonly IAccountService _account = default!;

        public UniTask<BootSnapshot> Fetch(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NullPlatformLog.Once(nameof(NullBootConfigClient) + "." + nameof(Fetch), "local stub");
            _ = _account.PlayerId;
            return UniTask.FromResult(new BootSnapshot(
                PlayerId: _account.PlayerId,
                EconomyId: "null",
                EconomyRevision: "0",
                EconomySource: "null"));
        }
    }
}
#nullable disable
