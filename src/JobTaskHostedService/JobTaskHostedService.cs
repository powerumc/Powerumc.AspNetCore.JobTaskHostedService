using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// Background hosted service
    /// </summary>
    internal class JobTaskHostedService : IJobTaskHostedService
    {
        private readonly ILogger<JobTaskHostedService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Job dictionary
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskAsyncJob> _jobs = new();

        /// <inheritdoc/>
        public event EventHandler<JobTaskCompletedEventArgs> JobTaskCompleted;

        public JobTaskHostedService(ILogger<JobTaskHostedService> logger,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Start the job
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start {nameof(JobTaskHostedService)}");

            cancellationToken.Register(() =>
            {
                _logger.LogInformation($"Cancel {nameof(JobTaskHostedService)}");
                //_waitHandle.Set();
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop the job.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stop {nameof(JobTaskHostedService)}");
            foreach (var job in _jobs)
            {
                try
                {
                    job.Value.Cancel();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"StopAsync Exception '{job.Value.JobName}'");
                }
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public TaskAsyncJob CreateJob(string jobName)
        {
            _logger.LogInformation($"CreateJob '{jobName}'");

            var job = new TaskAsyncJob(jobName, _loggerFactory.CreateLogger<TaskAsyncJob>());
            job.JobTaskCompleted += JobOnJobTaskCompleted;
            _jobs.TryAdd(jobName, job);

            return job;
        }

        /// <inheritdoc/>
        public void Stop(string jobName)
        {
            _logger.LogInformation($"Stop Job '{jobName}'");

            if (_jobs.TryGetValue(jobName, out var job))
            {
                job.JobTaskCompleted -= JobOnJobTaskCompleted;
                job.Cancel();
            }
        }

        private void JobOnJobTaskCompleted(object sender, JobTaskCompletedEventArgs e)
        {
            JobTaskCompleted?.Invoke(this, e);
        }
    }
}