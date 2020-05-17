using Boysenberry.DotNet.Hosting.StartupTasks.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Boysenberry.DotNet.Hosting.StartupTasks.Extensions
{
    public static class StartupTaskServiceCollectionExtensions
    {
        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask { return services.AddTransient<IStartupTask, T>(); }
    }
}
