using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter.Model
{
    public class SourceControl
    {
        public SourceControlType Type { get; set; }
        public string RepoOwner { get; set; }
        public string RepoUrl { get; set; }
        public string RepoName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
