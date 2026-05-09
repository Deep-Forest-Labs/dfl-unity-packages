#nullable enable
using Moq;

namespace DeepForestLabs.DependencyInjection
{
    public static class MockServiceProviderExtensions
    {
        public static IContainerBuilder AddMockScoped<T>(this IContainerBuilder builder)
            where T : class
        {
            Mock<T> created = new();
            builder.AddScoped(_ => created);
            builder.AddScoped(_ => created.Object);
            return builder;
        }
        
        public static IContainerBuilder AddMockSingleton<T>(this IContainerBuilder builder)
            where T : class
        {
            Mock<T> created = new Mock<T>();
            builder.AddSingleton(created);
            builder.AddSingleton(created.Object);
            return builder;
        }
        
        public static IContainerBuilder AddMockTransient<T>(this IContainerBuilder builder)
            where T : class
        {
            Mock<T> created = new Mock<T>();
            builder.AddTransient(_ => created);
            builder.AddSingleton(created.Object);
            
            return builder;
        }
    }
}
#nullable disable