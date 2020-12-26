using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// <see cref="Task"/> based async job task class.
    /// </summary>
    public class TaskAsyncJob
    {
        private readonly ILogger<TaskAsyncJob> _logger;

        /// <summary>
        /// Job name
        /// </summary>
        public string JobName { get; }

        /// <summary>
        /// task
        /// </summary>
        private Task Task { get; }

        /// <summary>
        /// sync object
        /// </summary>
        private ManualResetEvent ManualResetEvent { get; } = new(false);

        /// <summary>
        /// <see cref="CancellationTokenSource"/>
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; } = new();

        /// <summary>
        /// Job queue
        /// </summary>
        private readonly ConcurrentQueue<JobTask> _queue = new();

        /// <summary>
        /// The event when complete or fail the job.
        /// </summary>
        public event EventHandler<JobTaskCompletedEventArgs> JobTaskCompleted;

        public TaskAsyncJob(string jobName, ILogger<TaskAsyncJob> logger)
        {
            _logger = logger;
            JobName = jobName;

            Task = Task.Factory.StartNew(ProcessTask, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// The <see cref="Task"/> start and wait until enqueuing task if waiting job.
        /// </summary>
        private async Task ProcessTask()
        {
            _logger.LogInformation($"Process JobName '{JobName}'");
            while (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var jobTask))
                {
                    _logger.LogDebug($"Process Job Task Dequeue '{JobName}.'{jobTask.TaskId}''");

                    await ProcessTaskInternal(jobTask).ConfigureAwait(false);
                }
                else
                {
                    Pause();
                }
            }

            Cancel();
        }

        /// <summary>
        /// Process task for internal.
        /// </summary>
        /// <param name="jobTask"><see cref="JobTask"/></param>
        private async Task ProcessTaskInternal(JobTask jobTask)
        {
            using var scopeLogger = _logger.BeginScope($"Start JobTask '{JobName}'.'{jobTask.TaskId}'");
            try
            {
                _logger.LogInformation($"Process Job Task Start '{JobName}'.'{jobTask.TaskId}'");

                await jobTask.Func().ConfigureAwait(false);
                JobTaskCompleted?.Invoke(this, new JobTaskCompletedEventArgs(JobName, jobTask.TaskId, true));

                _logger.LogInformation($"Process Job Task Completed '{JobName}'.'{jobTask.TaskId}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Process Job Task Exception '{JobName}'.'{jobTask.TaskId}'");

                JobTaskCompleted?.Invoke(this, new JobTaskCompletedEventArgs(JobName, jobTask.TaskId, false));
            }
        }

        /// <summary>
        /// Enqueue TaskJob
        /// </summary>
        /// <param name="func">Job delegate</param>
        /// <param name="taskId">Job unique Id</param>
        public void Enqueue(Func<Task> func, string taskId = null)
        {
            var jobTask = new JobTask(taskId, func);
            _logger.LogDebug($"Enqueue task '{JobName}'.'{jobTask.TaskId}'");
            _queue.Enqueue(jobTask);

            ManualResetEvent.Set();
            ManualResetEvent.Reset();
        }

        /// <summary>
        /// Cancel the job and dispose <see cref="System.Threading.Tasks.Task"/> resource.
        /// </summary>
        public void Cancel()
        {
            _logger.LogInformation($"Cancel job '{JobName}'");

            CancellationTokenSource.Cancel();
            Task.Dispose();
        }

        /// <summary>
        /// Pause the job task.
        /// </summary>
        public void Pause()
        {
            ManualResetEvent.WaitOne();
        }
    }
}