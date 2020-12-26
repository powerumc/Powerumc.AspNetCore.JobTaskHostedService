using System;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// The EventArg when completed the job.
    /// </summary>
    public class JobTaskCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Job name
        /// </summary>
        public string JobName { get; }

        /// <summary>
        /// Job task unique Id
        /// </summary>
        public string TaskId { get; }

        /// <summary>
        /// Result the job task.
        /// </summary>
        public bool Succeed { get; }

        /// <summary>
        /// <see cref="JobTaskCompletedEventArgs"/> constructor.
        /// </summary>
        /// <param name="jobName">Job name</param>
        /// <param name="taskId">Job task unique Id</param>
        /// <param name="succeed">Result the job task</param>
        public JobTaskCompletedEventArgs(string jobName, string taskId, bool succeed)
        {
            JobName = jobName;
            TaskId = taskId;
            Succeed = succeed;
        }
    }
}