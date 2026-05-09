#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        IContainerBuilder AddTransient<T>() 
            where T : new();

        IContainerBuilder AddTransient<T, U>()
            where U : T, new();

        IContainerBuilder AddTransient<T>(Func<IDiCollection, T> factory);

        IContainerBuilder AddTransient<T>(Func<IDiCollection, CancellationToken, UniTask<T>> factory);
    }
}
#nullable disable