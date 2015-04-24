using Equilobe.DailyReport.Models.TaskScheduling;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ITaskSchedulerService : IService
    {
        void DeleteTask(string uniqueProjectKey);
        void DeleteMultipleTasks(ProjectListContext context);
        bool TryRunReportTask(ProjectContext context);
        void SetTask(ScheduledTaskContext context);
    }
}
