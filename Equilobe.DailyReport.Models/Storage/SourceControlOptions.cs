using Equilobe.DailyReport.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class SourceControlOptions
    {
        public SourceControlType Type { get; set; }
        public string RepoOwner { get; set; }
        public string RepoUrl { get; set; }
        public string RepoName { get; set; }
        public Credentials Credentials { get; set; }
        public string CommitUrl { get; set; }
    }
}
