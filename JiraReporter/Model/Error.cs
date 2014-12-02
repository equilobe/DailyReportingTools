using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public enum ErrorType { HasRemaining, HasNoTimeSpent, HasNoRemaining, Unassigned}
    public class Error
    {
        public ErrorType Type { get; set; }
    }
}
