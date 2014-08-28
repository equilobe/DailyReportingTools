using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter.Model
{
    public class Topic
    {
        public string Header { get; set; }
        public IEnumerable<string> ToDos { get; set; }
    }
}
