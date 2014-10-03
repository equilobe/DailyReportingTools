using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;

namespace SvnLogReporter.RazorEngine
{
    public class ExtendedTemplateBase<TModel> : TemplateBase<TModel>
    {
        public string Partial<TPartialModel>(string path, TPartialModel model)
        {
            var template = File.ReadAllText(path);
            var partialViewResult = Razor.Parse(template, model);

            return partialViewResult;
        }
    }
}
