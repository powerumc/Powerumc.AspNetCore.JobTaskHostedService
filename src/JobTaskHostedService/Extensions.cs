using Microsoft.Extensions.DependencyInjection;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    public static class Extensions
    {
        public static IServiceCollection AddJobTaskHostedService(this IServiceCollection services)
        {
            return services.AddSingleton<IJobTaskHostedService, JobTaskHostedService>()
                .AddHostedService(provider => provider.GetRequiredService<IJobTaskHostedService>());
        }
    }
}