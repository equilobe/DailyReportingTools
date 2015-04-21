using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using System.Diagnostics;
using Equilobe.DailyReport.Models.Views;

namespace SourceControlLogReporter.RazorEngine
{
    public class ExtendedTemplateBase<TModel> : TemplateBase<TModel>
    {
        public string Partial<TPartialModel>(string path, TPartialModel model)
        {
            try
            {
                var template = File.ReadAllText(path);
                var partialViewResult = Razor.Parse(template, model);
                return partialViewResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Partial(string path)
        {
            try
            {
                var template = File.ReadAllText(path);
                var partialViewResult = Razor.Parse(template, typeof(object));
                return partialViewResult;
            }
          catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public string Dot()
        {
            return Partial("Views/dot.cshtml");
        }

        public string ErrorIcon()
        {
            return Partial("Views/errorIcon.cshtml");
        }

        public string Square(string color)
        {
            return Partial("Views/square.cshtml", new Square(color));
        }

        public string ChartBar(string color, double width, double barMaxWidth, string text)
        {
            var model = new ChartBar(color, width, barMaxWidth, text);

            return Partial("Views/chartBar.cshtml", model);
        }
    }
}
