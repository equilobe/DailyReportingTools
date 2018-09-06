using Equilobe.DailyReport.Models.Dashboard;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IUserEngagementDataService : IService
    {
        void UpdateUserEngagementStats(Dictionary<long, UserEngagement> engagement, DateTime day, TimeSpan offsetFromUtc);
    }
}
