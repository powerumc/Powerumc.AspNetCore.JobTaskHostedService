using Microsoft.Extensions.DependencyInjection;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registration the <see cref="IJobTaskHostedService"/>.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        public static IServiceCollection AddJobTaskHostedService(this IServiceCollection services)
        {
            return services.AddSingleton<IJobTaskHostedService, JobTaskHostedService>()
                .AddHostedService(provider => provider.GetRequiredService<IJobTaskHostedService>());
        }
    }
}