using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter.Model
{
    public class Author
    {
        public string Name { get; set; }
        public IEnumerable<LogEntry> Entries { get; set; }
        public int EntryCount { get; set; }
    }
}
