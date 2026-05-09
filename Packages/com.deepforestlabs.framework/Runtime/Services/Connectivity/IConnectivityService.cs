using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs.Services.Connectivity
{
    public interface IConnectivityService
    {
        UniTask WaitForConnection();
        UniTask WaitForConnection(CancellationToken token);
    }
}