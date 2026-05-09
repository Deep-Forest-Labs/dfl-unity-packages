#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public interface IInitializable
    {
        UniTask Initialize(CancellationToken token);
    }
}
#nullable disable