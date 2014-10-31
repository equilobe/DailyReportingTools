using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    class SvnNotAvailableException : Exception
    {

        public SvnNotAvailableException(Exception ex)
            : base("Could not connect to SVN.", ex)
        {

        }
    }
}
