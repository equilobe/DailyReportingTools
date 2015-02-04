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
    }
}
