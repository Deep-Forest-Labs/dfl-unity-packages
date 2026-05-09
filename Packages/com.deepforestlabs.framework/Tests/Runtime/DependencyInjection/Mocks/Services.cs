#nullable enable
using System;

#pragma warning disable CS0414
// ReSharper disable ConvertToAutoProperty
// Do not add as transient with any other service being transient as well. Will cause stack overflow as transients
// cannot be co-dependent in IoC dependency injection
namespace DeepForestLabs.DependencyInjection.Mocks
{
    public interface IService : IDisposable
    {
        bool IsDisposed { get; }
    }
    
    public interface IServiceA : IService
    {
        IServiceB ServiceB  { get; }
        IServiceC ServiceC  { get; }
        IServiceD ServiceD  { get; }
        IServiceE ServiceE  { get; }
    }
    public sealed class ServiceA : DisposableService, IServiceA
    {
        [Dependency] private readonly IServiceB _serviceB = null!;
        [Dependency] private readonly IServiceC _serviceC = null!;
        [Dependency] private readonly IServiceD _serviceD = null!;
        [Dependency] private readonly IServiceE _serviceE = null!;
        
        public IServiceB ServiceB => _serviceB;
        public IServiceC ServiceC => _serviceC;
        public IServiceD ServiceD => _serviceD;
        public IServiceE ServiceE => _serviceE;
    }

    public interface IServiceB : IService
    {
        IServiceC ServiceC  { get; }
        IServiceD ServiceD  { get; }
        IServiceE ServiceE  { get; }
    }
    
    public sealed class ServiceB : DisposableService, IServiceB
    {
        [Dependency] public IServiceC ServiceC { get; } = null!;
        [Dependency] public IServiceD ServiceD { get; } = null!;
        [Dependency] public IServiceE ServiceE { get; } = null!;
    }

    public interface IServiceC : IService
    {
        IServiceD ServiceD  { get; }
        IServiceE ServiceE  { get; }
    }
    public sealed class ServiceC : DisposableService, IServiceC
    {
        [Dependency] public IServiceD ServiceD { get; } = null!;
        [Dependency] public IServiceE ServiceE { get; } = null!;
    }

    public interface IServiceD : IService
    {
        IServiceA ServiceA  { get; }
        IServiceB ServiceB  { get; }
        IServiceE ServiceE  { get; }
    }
    
    public sealed class ServiceD : DisposableService, IServiceD
    {
        [Dependency] private readonly IServiceA _serviceA = null!;
        [Dependency] private readonly IServiceB _serviceB = null!;
        [Dependency] private readonly IServiceE _serviceE = null!;
        
        public IServiceA ServiceA => _serviceA;
        public IServiceB ServiceB => _serviceB;
        public IServiceE ServiceE => _serviceE;
    }
    
    //Transient safe, will never be co-dependent
    public interface IServiceE : IService
    {
    }
    
    public sealed class ServiceE : DisposableService, IServiceE
    {
    }

    public abstract class DisposableService : IService
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public static class ServicesExtensions
    {
        internal static IContainerBuilder AddScopedServiceA(this IContainerBuilder container)
        {
            container.AddScoped<IServiceA, ServiceA>();
            return container;
        }
        
        internal static IContainerBuilder AddSingletonServiceA(this IContainerBuilder container)
        {
            container.AddSingleton<IServiceA>(new ServiceA());
            return container;
        }
        
        internal static IContainerBuilder AddTransientServiceA(this IContainerBuilder container)
        {
            container.AddTransient<IServiceA, ServiceA>();
            return container;
        }

        internal static IContainerBuilder AddScopedServiceB(this IContainerBuilder container)
        {
            container.AddScoped<IServiceB, ServiceB>();
            return container;
        }
        
        internal static IContainerBuilder AddSingletonServiceB(this IContainerBuilder container)
        {
            container.AddSingleton<IServiceB>( new ServiceB());
            return container;
        }
        
        internal static IContainerBuilder AddTransientServiceB(this IContainerBuilder container)
        {
            container.AddTransient<IServiceB, ServiceB>();
            return container;
        }
        
        internal static IContainerBuilder AddScopedServiceC(this IContainerBuilder container)
        {
            container.AddScoped<IServiceC, ServiceC>();
            return container;
        }
        
        internal static IContainerBuilder AddSingletonServiceC(this IContainerBuilder container)
        {
            container.AddSingleton<IServiceC>(new ServiceC());
            return container;
        }
        
        internal static IContainerBuilder AddTransientServiceC(this IContainerBuilder container)
        {
            container.AddTransient<IServiceC, ServiceC>();
            return container;
        }
        
        internal static IContainerBuilder AddScopedServiceD(this IContainerBuilder container)
        {
            container.AddScoped<IServiceD, ServiceD>();
            return container;
        }
        
        internal static IContainerBuilder AddSingletonServiceD(this IContainerBuilder container)
        {
            container.AddSingleton<IServiceD>(new ServiceD());
            return container;
        }
        
        internal static IContainerBuilder AddTransientServiceD(this IContainerBuilder container)
        {
            container.AddTransient<IServiceD, ServiceD>();
            return container;
        }
        
        internal static IContainerBuilder AddScopedServiceE(this IContainerBuilder container)
        {
            container.AddScoped<IServiceE, ServiceE>();
            return container;
        }
        
        internal static IContainerBuilder AddSingletonServiceE(this IContainerBuilder container)
        {
            container.AddSingleton<IServiceE>(new ServiceE());
            return container;
        }
        
        internal static IContainerBuilder AddTransientServiceE(this IContainerBuilder container)
        {
            container.AddTransient<IServiceE, ServiceE>();
            return container;
        }
    }
}
#pragma warning restore CS0414
#nullable disable