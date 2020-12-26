using System;
using Microsoft.Extensions.Hosting;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// Interface for background job service.
    /// </summary>
    public interface IJobTaskHostedService : IHostedService
    {
        /// <summary>
        /// Create the job
        /// </summary>
        /// <param name="jobName">Job name</param>
        TaskAsyncJob CreateJob(string jobName);

        /// <summary>
        /// Stop and dispose the job.
        /// </summary>
        /// <param name="jobName">Job 이름</param>
        void Stop(string jobName);

        /// <summary>
        /// The event when the complete job.
        /// </summary>
        event EventHandler<JobTaskCompletedEventArgs> JobTaskCompleted;
    }
}