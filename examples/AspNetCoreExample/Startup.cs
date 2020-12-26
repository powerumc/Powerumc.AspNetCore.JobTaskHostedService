using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Powerumc.AspNetCore.JobTaskHostedService;

namespace AspNetCoreExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddJobTaskHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            var jobService = app.ApplicationServices.GetRequiredService<IJobTaskHostedService>();
            var job = jobService.CreateJob("job 1");
            job.Enqueue(async () =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"HELLO");
            }, "00000");

            for (var i = 0; i < 10; i++)
            {
                var ii = i;
                job.Enqueue(async () =>
                {
                    await Task.Delay(1000);
                    Console.WriteLine($"{job.JobName} {ii}");
                }, ii.ToString());
            }
        }
    }
}