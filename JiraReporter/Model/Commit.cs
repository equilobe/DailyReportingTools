using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Commit
    {
        public SourceControlLogReporter.Model.LogEntry Entry { get; set; }
        public bool TaskSynced { get; set; }
    }
}
