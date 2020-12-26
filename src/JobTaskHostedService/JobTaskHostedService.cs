using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// 백그라운드 Job 서비스 클래스 입니다.
    /// </summary>
    internal class JobTaskHostedService : IJobTaskHostedService
    {
        private readonly ILogger<JobTaskHostedService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Job 목록
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
        /// 백그라운드 서비스를 시작
        /// </summary>
        /// <param name="cancellationToken">취소 토큰</param>
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
        /// 백그라운드 작업을 중지합니다.
        /// </summary>
        /// <param name="cancellationToken">취소 토큰</param>
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
            
            // Job 을 생성
            var job = new TaskAsyncJob(jobName, _loggerFactory.CreateLogger<TaskAsyncJob>());
            job.JobTaskCompleted += JobOnJobTaskCompleted;
            _jobs.TryAdd(jobName, job);

            return job;
        }

        /// <inheritdoc/>
        public void Stop(string jobName)
        {
            _logger.LogInformation($"Stop Job '{jobName}'");
            
            // Job 제거
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