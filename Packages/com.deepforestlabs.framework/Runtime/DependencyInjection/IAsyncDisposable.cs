#nullable enable
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public interface IAsyncDisposable
    {
        UniTask DisposeAsync();
    }
}
#nullable disable