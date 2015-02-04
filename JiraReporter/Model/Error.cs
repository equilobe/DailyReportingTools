using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public enum ErrorType { HasRemaining, HasNoTimeSpent, HasNoRemaining, Unassigned, NotConfirmed}
    public class Error
    {
        public ErrorType Type { get; set; }

        public Error()
        {

        }
        
        public Error(ErrorType type)
        {
            Type = type;
        }
    }
}
