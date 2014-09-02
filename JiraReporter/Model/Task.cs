using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Task
    {
        public Issue Issue;
        public string CompletedTimeAgo;
        public DateTime ResolutionDate;
    }
}
