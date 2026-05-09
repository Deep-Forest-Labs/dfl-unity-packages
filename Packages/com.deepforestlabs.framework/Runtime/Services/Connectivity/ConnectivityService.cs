#nullable enable
using System;
using System.Threading;
using DeepForestLabs.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace DeepForestLabs.Services.Connectivity
{
    internal sealed class ConnectivityService : IConnectivityService, IInitializable
    {
        [Dependency] private readonly CancellationToken _scope = default;
        [Dependency] private readonly IContainer _container = null!;
        [Dependency] private readonly RetryState _retryState = null!;
        [Dependency] private readonly IConnectionErrorStateController _connectionErrorState = null!;
        
        private UniTaskCompletionSource? _completionSource = null!;

        public UniTask Initialize (CancellationToken token)
        {
            WaitForConnection(token).Forget();
            return UniTask.CompletedTask;
        }

        public async UniTask WaitForConnection() => await WaitForConnection(_container.Get<CancellationToken>());
        public async UniTask WaitForConnection(CancellationToken token)
        {
            if (_completionSource == null)
            {
                _completionSource = new UniTaskCompletionSource();
                WaitForConnectivityInternal(_scope).Forget();
            }

            await _completionSource.Task
                .AttachExternalCancellation(token);
        }
        
        private async UniTask WaitForConnectivityInternal(CancellationToken token)
        {
            Log.Info("Checking network connection.");
            ResultE result = await _retryState.Run(token);
            if (!result.IsValid)
            {
                Log.Info("Network connection unavailable: {0}", result.ErrorMessage);
                await _connectionErrorState.Run(token);
            }
            
            _completionSource?.TrySetResult();
            _completionSource = null;
        }
    }
}
#nullable disable