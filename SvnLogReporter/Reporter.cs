using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SourceControlLogReporter.Model;
using RazorEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using RazorEngine.Templating;
using Octokit;

namespace SourceControlLogReporter
{
    public class Reporter
    {

        public static void WriteReport(Policy p, string report, string path)
        {
            Validation.EnsureDirectoryExists(p.GeneratedProperties.LogArchivePath);

            var archivedFilePath = Path.Combine(p.GeneratedProperties.LogArchivePath, Path.GetFileName(path));

            if (File.Exists(path))
            {
                File.Copy(path, archivedFilePath, true);
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(archivedFilePath, report);
            }

            Validation.EnsureDirectoryExists(p.GeneratedProperties.UnsentReportsPath);

            var reportPath = Path.Combine(p.GeneratedProperties.UnsentReportsPath, Path.GetFileNameWithoutExtension(path) + ".html");

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
