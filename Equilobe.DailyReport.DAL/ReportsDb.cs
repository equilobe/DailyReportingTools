using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Storage;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Equilobe.DailyReport.DAL
{
	public class ReportsDb : IdentityDbContext<ApplicationUser>
    {
        public ReportsDb()
            : base("name=ReportsDb")
        {

        }

		public static ReportsDb Create()
		{
			return new ReportsDb();
		}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);
			
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
