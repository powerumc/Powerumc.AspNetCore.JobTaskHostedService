using System;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// Job 의 작업이 완료되면 발생하는 이벤트 매개변수 클래스
    /// </summary>
    public class JobTaskCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Job 이름
        /// </summary>
        public string JobName { get; }
        
        /// <summary>
        /// Job 의 작업의 고유 ID
        /// </summary>
        public string TaskId { get; }
        
        /// <summary>
        /// 작업이 성공했는지 여부
        /// </summary>
        public bool Succeed { get; }

        /// <summary>
        /// Job 의 작업이 완료되면 발생하는 객체를 생성
        /// </summary>
        /// <param name="jobName">Job 이름</param>
        /// <param name="taskId">Job 의 작업의 고유 ID</param>
        /// <param name="succeed">작업이 성공했는지 여부</param>
        public JobTaskCompletedEventArgs(string jobName, string taskId, bool succeed)
        {
            JobName = jobName;
            TaskId = taskId;
            Succeed = succeed;
        }
    }
}