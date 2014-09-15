using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Author
    {
        public string Name { get; set; }
        public List<Issue> Issues { get; set; }
        public string TimeLogged { get; set; }
        public int TimeSpent { get; set; }      
    }
}
