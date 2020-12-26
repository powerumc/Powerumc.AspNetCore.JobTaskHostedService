using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// <see cref="Task"/> 기반의 비동기 작업 클래스 입니다.
    /// </summary>
    public class TaskAsyncJob
    {
        private readonly ILogger<TaskAsyncJob> _logger;
        
        /// <summary>
        /// Job 이름
        /// </summary>
        public string JobName { get; }
        
        /// <summary>
        /// 작업 개체
        /// </summary>
        private Task Task { get; }
        
        /// <summary>
        /// Job 을 제어하는 동기화 객체
        /// </summary>
        private ManualResetEvent ManualResetEvent { get; } = new(false);
        
        /// <summary>
        /// 취소 토큰
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; } = new();

        /// <summary>
        /// 작업 큐
        /// </summary>
        private readonly ConcurrentQueue<JobTask> _queue = new();

        /// <summary>
        /// Job 의 작업이 완료히거나 실패하면 발생하는 이벤트
        /// </summary>
        public event EventHandler<JobTaskCompletedEventArgs> JobTaskCompleted; 

        public TaskAsyncJob(string jobName, ILogger<TaskAsyncJob> logger)
        {
            _logger = logger;
            JobName = jobName;
            
            // Task 를 생성
            Task = Task.Factory.StartNew(ProcessTask, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// <see cref="Task"/> 가 시작되고 큐에 대기중인 작업이 없으면 큐에 작업이 들어올 때 까지 대기
        /// </summary>
        private async Task ProcessTask()
        {
            _logger.LogInformation($"ProcessTask JobName '{JobName}'");
            while (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var jobTask))
                {
                    _logger.LogInformation($"ProcessTask Dequeue '{JobName}.'{jobTask.TaskId}''");

                    // 다음 작업 시작
                    await Task.ContinueWith(async task => { await ProcessTaskInternal(jobTask); }, CancellationTokenSource.Token)
                        .ConfigureAwait(false);
                }
                else
                {
                    // 큐에 작업이 없으면 대기
                    ManualResetEvent.WaitOne();
                }
            }

            Cancel();
        }

        /// <summary>
        /// 작업을 시작
        /// </summary>
        /// <param name="jobTask"><see cref="JobTask"/> 객체</param>
        private async Task ProcessTaskInternal(JobTask jobTask)
        {
            using var scopeLogger = _logger.BeginScope($"Start JobTask '{JobName}'.'{jobTask.TaskId}'");
            try
            {
                _logger.LogInformation($"ProcessTask JobTask Start '{JobName}'.'{jobTask.TaskId}'");
                
                await jobTask.Func().ConfigureAwait(false);
                JobTaskCompleted?.Invoke(this, new JobTaskCompletedEventArgs(JobName, jobTask.TaskId, true));
                
                _logger.LogInformation($"ProcessTask JobTask Success '{JobName}'.'{jobTask.TaskId}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"ProcessTask JobTask Exception '{JobName}'.'{jobTask.TaskId}'");
                
                JobTaskCompleted?.Invoke(this, new JobTaskCompletedEventArgs(JobName, jobTask.TaskId, false));
            }
        }

        /// <summary>
        /// TaskJob 의 큐에 작업을 추가
        /// </summary>
        /// <param name="func">작업 대리자</param>
        /// <param name="taskId">작업 고유 ID</param>
        public void Enqueue(Func<Task> func, string taskId = null)
        {
            var jobTask = new JobTask(taskId, func);
            _logger.LogInformation($"Enqueue task '{JobName}'.'{jobTask.TaskId}'");
            _queue.Enqueue(jobTask);

            // 큐에 작업이 등록되면 대기 핸들의 Signal 을 리셋
            ManualResetEvent.Set();
            ManualResetEvent.Reset();
        }

        /// <summary>
        /// Job 의 작업을 최소하고 <see cref="System.Threading.Tasks.Task"/> 리소스를 해제
        /// </summary>
        public void Cancel()
        {
            _logger.LogInformation($"Cancel job '{JobName}'");
            
            CancellationTokenSource.Cancel();
            Task.Dispose();
        }
    }
}