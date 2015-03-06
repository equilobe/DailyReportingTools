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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SerializedPolicy>()
                .HasRequired(x => x.ReportSettings)
                .WithOptional(x => x.SerializedPolicy)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ReportExecutionSummary>()
                .HasRequired(x => x.ReportSettings)
                .WithOptional(x => x.ReportExecutionSummary)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<FinalDraftConfirmation>()
                .HasRequired(x => x.ReportSettings)
                .WithOptional(x => x.FinalDraftConfirmation)
                .WillCascadeOnDelete(true);
        }

        public DbSet<ReportSettings> ReportSettings { get; set; }
        public DbSet<InstalledInstance> InstalledInstances { get; set; }
        public DbSet<ReportExecutionSummary> ReportExecutionSummaries { get; set; }
        public DbSet<ReportExecutionInstance> ReportExecutionInstances { get; set; }
        public DbSet<FinalDraftConfirmation> FinalDraftConfirmations { get; set; }
        public DbSet<IndividualDraftConfirmation> IndividualDraftConfirmations { get; set; }
        public DbSet<SerializedPolicy> SerializedPolicies { get; set; }
    }
}
