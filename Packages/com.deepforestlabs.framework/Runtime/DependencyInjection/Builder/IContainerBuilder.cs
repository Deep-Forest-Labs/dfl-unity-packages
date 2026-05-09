#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder : IDiCollection
    {
        string Name { get; }
        IContainer? Parent { get; }
        IDiCollection Collection { get; }

        event Action<IContainer>? OnPreInitialize;
        event Action<IContainer>? OnBuildComplete;

        UniTask<IContainer> Build(CancellationToken token);
    }
}
#nullable disable