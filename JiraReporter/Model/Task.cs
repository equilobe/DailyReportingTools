using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Task
    {
        public Issue Issue { get; set; }
        public string CompletedTimeAgo { get; set; }
        public DateTime ResolutionDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool HasInProgress { get; set; }
        public bool HasInProgressAuthor { get; set; }
    }
}
