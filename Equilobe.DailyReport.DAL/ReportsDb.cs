﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Storage;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;

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
			
			modelBuilder.Entity<SerializedAdvancedSettings>()
                .HasRequired(x => x.BasicSettings)
                .WithOptional(x => x.SerializedAdvancedSettings)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ReportExecutionSummary>()
                .HasRequired(x => x.BasicSettings)
                .WithOptional(x => x.ReportExecutionSummary)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<FinalDraftConfirmation>()
                .HasRequired(x => x.BasicSettings)
                .WithOptional(x => x.FinalDraftConfirmation)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.InstalledInstances)
                .WithOptional(x => x.User)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Subscription>()
                .HasRequired(x => x.InstalledInstance)
                .WithMany(x => x.Subscriptions)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<HtmlReport>()
                .HasRequired(x => x.BasicSettings)
                .WithMany(x => x.HtmlReports)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Payment>()
                .HasRequired(x => x.Subscription)
                .WithMany(x => x.Payments)
                .WillCascadeOnDelete(true);

			modelBuilder.Entity<ApplicationUser>().ToTable("Users");
			modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
			modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
			modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
			modelBuilder.Entity<IdentityRole>().ToTable("Roles");

        }

        public DbSet<BasicSettings> BasicSettings { get; set; }
        public DbSet<InstalledInstance> InstalledInstances { get; set; }
        public DbSet<ReportExecutionSummary> ReportExecutionSummaries { get; set; }
        public DbSet<ReportExecutionInstance> ReportExecutionInstances { get; set; }
        public DbSet<FinalDraftConfirmation> FinalDraftConfirmations { get; set; }
        public DbSet<IndividualDraftConfirmation> IndividualDraftConfirmations { get; set; }
        public DbSet<SerializedAdvancedSettings> SerializedAdvancedSettings { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Subscription> Subscriptions{ get; set; }
        public DbSet<HtmlReport> HtmlReports { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<IPNLog> IPNLogs { get; set; }
    }
}
