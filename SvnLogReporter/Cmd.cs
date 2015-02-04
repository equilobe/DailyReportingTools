using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    public class Cmd
    {
        public static void ExecuteSingleCommand(string command, string directory=null)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            if(!string.IsNullOrEmpty(directory))
                process.StartInfo.WorkingDirectory = directory;
            process.StartInfo.Arguments = string.Format("/c {0}", command);
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
        }
    }
}
