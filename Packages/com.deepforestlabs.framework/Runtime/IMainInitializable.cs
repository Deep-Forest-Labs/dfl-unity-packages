#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public interface IMainInitializable
    {
        UniTask InitializeAsync(IContainer container, CancellationToken token);
    }
}
#nullable disable
