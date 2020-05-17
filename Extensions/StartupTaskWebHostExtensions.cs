using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Boysenberry.DotNet.Hosting.StartupTasks.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Boysenberry.DotNet.Hosting.StartupTasks.Extensions
{
    public static class StartupTaskWebHostExtensions
    {
        public static async Task RunWithTasksAsync(this IHostBuilder builder, CancellationToken cancellationToken = default)
        {
            var host = builder.UseConsoleLifetime().Build();

            var startupTasks = host.Services.GetServices<IStartupTask>();
            foreach (var startupTask in startupTasks)
            {
                using var scope = host.Services.CreateScope();
                var serviceProvider = scope.ServiceProvider;

                var startupTaskType = startupTask.GetType();
                var startupTaskExecuteMethod = startupTaskType.GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance);
                if (startupTaskExecuteMethod == null) throw new InvalidOperationException($"A public method named 'ExecuteAsync' could not be found in the '{startupTaskType.FullName}' type.");

                var startupTaskParameters = PopulateParameters(cancellationToken, startupTaskExecuteMethod, serviceProvider);

                await (Task) startupTaskExecuteMethod.Invoke(startupTask, startupTaskParameters);
            }

            await host.RunAsync(cancellationToken);
        }

        private static object[] PopulateParameters(CancellationToken cancellationToken, MethodInfo startupTaskExecuteMethod, IServiceProvider serviceProvider)
        {
            var services = new List<object>();
            foreach (var parameter in startupTaskExecuteMethod.GetParameters())
                if (parameter.ParameterType == typeof(CancellationToken))
                    services.Add(cancellationToken);
                else
                    services.Add(serviceProvider.GetService(parameter.ParameterType));
            return services.ToArray();
        }
    }
}
