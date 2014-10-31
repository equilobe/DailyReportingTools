using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter.Model
{
    public class User
    {
        public string JiraAuthor { get; set; }
        public string SourceControlAuthor { get; set; }
    }
}
