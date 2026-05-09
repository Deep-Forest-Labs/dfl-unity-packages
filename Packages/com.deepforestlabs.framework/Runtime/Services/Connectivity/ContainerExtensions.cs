#nullable enable
namespace DeepForestLabs.Services.Connectivity
{
    public static class ContainerExtensions
    {
        public static IContainerBuilder AddConnectivityService(this IContainerBuilder container)
        {
            return container
                .AddScoped<IConnectivityService, ConnectivityService>()
                .AddScoped<RetryState>();
        }
    }
}
#nullable disable