using JiraReporter.Model;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
namespace JiraReporter
{
    class ReportProcessor
    {
        public static string ProcessReport(Report report)
        {
            string template = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Views\TimesheetReportTemplate.cshtml");
          //  var partial = RenderPartialToString(@"Views\_partial.cshtml", report);
         //   template += partial;
            return Razor.Parse(template, report);
        }

        public static string RenderPartialToString(string viewPath, object model)
        {
            string viewAbsolutePath = MapPath(viewPath);

            var viewSource = File.ReadAllText(viewAbsolutePath);

            string renderedText = Razor.Parse(viewSource, model);
            return renderedText;
        }

        public static string MapPath(string filePath)
        {
            return HttpContext.Current != null ? HttpContext.Current.Server.MapPath(filePath) : string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, filePath.Replace("~", string.Empty).TrimStart('/'));
        }    
    }         
}
