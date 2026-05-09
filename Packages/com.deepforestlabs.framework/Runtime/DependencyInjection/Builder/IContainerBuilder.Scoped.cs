#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeepForestLabs.Factories;

namespace DeepForestLabs
{
    public partial interface IContainerBuilder
    {
        IContainerBuilder AddScoped<T>()
            where T : new();

        IContainerBuilder AddScoped<T, U>()
            where U : T, new();

        IContainerBuilder AddScoped<T>(Func<IDiCollection, T> factory);
        IContainerBuilder AddScoped<T>(Func<IDiCollection, CancellationToken, UniTask<T>> asyncFactory);
        IContainerBuilder AddScoped(Type t, Func<IDiCollection, object> factory);
    }
}
#nullable disable