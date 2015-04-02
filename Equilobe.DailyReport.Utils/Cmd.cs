using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Utils
{
    public class Cmd
    {
        public static string Execute(string command)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;

            var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            string output = string.Empty;
            using (StreamReader streamReader = process.StandardOutput)
            {
                output = streamReader.ReadToEnd();
            }

            return output;
        }
    }
}