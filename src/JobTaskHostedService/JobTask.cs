using System;
using System.Threading.Tasks;

namespace Powerumc.AspNetCore.JobTaskHostedService
{
    /// <summary>
    /// Job 에 작업을 할당하는 엔티티 클래스
    /// </summary>
    public class JobTask
    {
        /// <summary>
        /// JobTask 객체를 생성
        /// </summary>
        /// <param name="taskId">작업의 고유 ID</param>
        /// <param name="func">작업 대리자</param>
        public JobTask(string taskId, Func<Task> func)
        {
            TaskId = taskId ?? Guid.NewGuid().ToString();
            Func = func;
        }

        /// <summary>
        /// 작업의 고유 ID, 값이 null 인 경우 <see cref="Guid.NewGuid"/> 로 생성된 문자열을 반환
        /// </summary>
        public string TaskId { get; }
        
        /// <summary>
        /// 작업 대리자
        /// </summary>
        public Func<Task> Func { get; }
    }
}