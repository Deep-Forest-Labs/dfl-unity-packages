#nullable enable
using DeepForestLabs.Logger;
using DeepForestLabs.Factories;
using DeepForestLabs.Services;
using DeepForestLabs.States.UnobservedExceptions;
using UnityEngine;

namespace DeepForestLabs
{
    public abstract class MainArgs : ContainerBuilderFactory
    {
        [SerializeField] private ScriptableObjectAssetRefT<LogFilter> _logFilter = default!;
        [SerializeField] private ScriptableObjectAssetRefT<ContainerBuilderFactory> _appContainerFactory = default!;
        
        public override IContainerBuilder AddToBuilder(IContainerBuilder builder)
        {
            return base.AddToBuilder(builder)
                .AddScriptableObject(_logFilter)
                .AddScriptableObject(_appContainerFactory)
                .AddAlias<ILogFilter, LogFilter>()
                .AddScopedComponent<AudioListener>()
                .AddScoped<ILoggingService, LoggingService>()
                .AddScoped<UnobservedExceptionState>();
        }
    }
}
#nullable disable