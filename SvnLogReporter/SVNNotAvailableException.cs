using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter
{
    class SvnNotAvailableException : Exception
    {

        public SvnNotAvailableException(Exception ex)
            : base("Could not connect to SVN.", ex)
        {

        }
    }
}
