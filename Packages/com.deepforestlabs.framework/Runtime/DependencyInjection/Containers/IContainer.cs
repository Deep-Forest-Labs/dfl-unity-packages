#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeepForestLabs
{
    public partial interface IContainer : System.IAsyncDisposable, IDiCollection, IDisposable
    {
        string Name { get; }
        
        IContainer? Parent { get; }
        Transform Transform { get; }
        
        IReadOnlyList<IContainer> Children { get; }
        
        bool IsDisposed { get; }

        event Action<IContainer>? OnBuildComplete;
        event Action<IContainer>? OnPreDispose;

        UniTask EnableAddressables(string withCdn, CancellationToken token);

        UniTask UnloadUnusedResources(CancellationToken token);
        
        IContainerBuilder CreateChild(string name);

        UniTask<T> Create<T>(CancellationToken token) where T : class;
        
        UniTask<T> Create<T, TArgs>(TArgs args, CancellationToken token) where T : class;
        
        UniTask<T> Create<T, TArgs1, TArgs2>(TArgs1 args1, TArgs2 args2, CancellationToken token) where T : class;
        
        UniTask<T> Create<T, TArgs1, TArgs2, TArgs3>(TArgs1 args1, TArgs2 args2, TArgs3 args3, CancellationToken token)
            where T : class;
    }
}
#nullable disable