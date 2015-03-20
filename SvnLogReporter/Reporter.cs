using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RazorEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using RazorEngine.Templating;
using Octokit;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Policy;

namespace SourceControlLogReporter
{
    public class Reporter
    {

        public static void WriteReport(Policy p, string report, string path)
        {
            Validations.EnsureDirectoryExists(p.LogArchivePath);

            var archivedFilePath = Path.Combine(p.LogArchivePath, Path.GetFileName(path));

            if (File.Exists(path))
            {
                File.Copy(path, archivedFilePath, true);
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(archivedFilePath, report);
            }

            Validations.EnsureDirectoryExists(p.UnsentReportsPath);

            var reportPath = Path.Combine(p.UnsentReportsPath, Path.GetFileNameWithoutExtension(path) + ".html");

            File.WriteAllText(reportPath, report);
        }

        //public static string ProcessReport(Policy p, Report report)
        //{
        //    try
        //    {
        //        string template = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Views\ReportTemplateSVN.cshtml");
        //        report.Title = p.ReportTitle;
        //        return Razor.Parse(template, report);
        //    }
        //    catch (TemplateCompilationException templateException)
        //    {
        //        foreach (var error in templateException.Errors)
        //        {
        //            Debug.WriteLine(error);
        //        }
        //        return "Error in template compilation";
        //    }
        //}           
    }
}
