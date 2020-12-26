using System;
using Microsoft.Extensions.Hosting;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// 백그라운드 작업 서비스의 인터페이스 입니다.
    /// </summary>
    public interface IJobTaskHostedService : IHostedService
    {
        /// <summary>
        /// Job을 생성합니다.
        /// </summary>
        /// <param name="jobName">Job 이름</param>
        TaskAsyncJob CreateJob(string jobName);
        
        /// <summary>
        /// Job을 중지하고 할당 해제 합니다.
        /// </summary>
        /// <param name="jobName">Job 이름</param>
        void Stop(string jobName);
        
        /// <summary>
        /// Job 의 작업이 완료될 때 발생하는 이벤트
        /// </summary>
        event EventHandler<JobTaskCompletedEventArgs> JobTaskCompleted;
    }
}