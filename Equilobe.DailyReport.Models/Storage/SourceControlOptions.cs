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
        //Repo contains repository URL for SVN or repository Name for GitHub
        public string Repo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CommitUrl { get; set; }
    }
}
