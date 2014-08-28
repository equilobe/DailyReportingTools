using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter
{
    public class Cmd
    {
        public static void ExecuteSingleCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = string.Format("/c {0}", command);
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
           // process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            //string error = process.StandardError.ReadToEnd();
                   
            process.WaitForExit();
            //if (error != "")
            //   throw new ApplicationException(error);
          
        }
    }
}
