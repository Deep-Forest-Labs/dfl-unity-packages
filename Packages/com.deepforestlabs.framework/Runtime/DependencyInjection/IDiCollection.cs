#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DeepForestLabs
{
    public interface IDiCollection
    {
        CancellationToken Scope { get; }
        
        object Get(Type type);
        T Get<T>();
        T? Find<T>();
        bool TryGet<T>([NotNullWhen(true)] out T? instance);
    }
}
#nullable disable