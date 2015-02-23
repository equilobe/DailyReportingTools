using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Storage;

namespace Equilobe.DailyReport.DAL
{
    public class ReportsDb : DbContext
    {
        public ReportsDb()
            : base("name=ReportsDb")
        {

        }

        public DbSet<ReportSettings> ReportSettings { get; set; }
        public DbSet<ReportExecutionSummary> ReportExecutionSummaries { get; set; }
        public DbSet<ReportExecutionInstance> ReportExecutionInstances { get; set; }
        public DbSet<FinalDraftConfirmation> FinalDraftConfirmations { get; set; }
        public DbSet<IndividualDraftConfirmation> IndividualDraftConfirmations { get; set; }
        public DbSet<SerializedPolicy> SerializedPolicies { get; set; }
    }
}
