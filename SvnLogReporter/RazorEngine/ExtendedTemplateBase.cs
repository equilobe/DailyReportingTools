using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using System.Diagnostics;

namespace SvnLogReporter.RazorEngine
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
            catch (TemplateCompilationException templateException)
            {
                foreach (var error in templateException.Errors)
                {
                    Debug.WriteLine(error);
                }
                return "Error in partial view compilation";
            }        
        }
    }
}
