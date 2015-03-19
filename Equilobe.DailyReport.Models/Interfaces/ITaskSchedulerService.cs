using System;
using Equilobe.DailyReport.Models.TaskScheduling;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ITaskSchedulerService : IService
    {
        void CreateTask(ScheduledTaskContext context);
        void DeleteMultipleTasks(ProjectListContext context);
        bool TryRunReportTask(ProjectContext context);
        void UpdateTask(ScheduledTaskContext context);
    }
}
