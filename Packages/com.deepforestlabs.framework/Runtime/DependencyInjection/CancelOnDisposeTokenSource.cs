#nullable enable
using System;
using System.Threading;

namespace DeepForestLabs
{
    public sealed class CancelOnDisposeTokenSource : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancelOnDisposeTokenSource(CancellationTokenSource cancellationTokenSource) => _cancellationTokenSource = cancellationTokenSource;

        public CancelOnDisposeTokenSource(CancellationToken token) 
            => _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        
        public CancelOnDisposeTokenSource(params CancellationToken[] tokens)
            => _cancellationTokenSource = tokens is { Length: > 0 }
                ? CancellationTokenSource.CreateLinkedTokenSource(tokens)
                : new CancellationTokenSource();

        public CancellationToken Token => _cancellationTokenSource.Token;

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public static CancelOnDisposeTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            return new CancelOnDisposeTokenSource(tokens);
        }
    }
}
#nullable disable