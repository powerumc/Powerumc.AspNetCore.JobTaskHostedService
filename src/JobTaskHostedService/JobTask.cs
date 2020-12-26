using System;
using System.Threading.Tasks;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// Job task entity class.
    /// </summary>
    public class JobTask
    {
        /// <summary>
        /// <see cref="JobTask"/> constructor.
        /// </summary>
        /// <param name="taskId">Job unique Id</param>
        /// <param name="func">Task delegate</param>
        public JobTask(string taskId, Func<Task> func)
        {
            TaskId = taskId ?? Guid.NewGuid().ToString();
            Func = func;
        }

        /// <summary>
        /// Task unique Id. If <see cref="TaskId"/> is null, return <see cref="Guid.NewGuid"/> as string.
        /// </summary>
        public string TaskId { get; }

        /// <summary>
        /// Task delegate
        /// </summary>
        public Func<Task> Func { get; }
    }
}