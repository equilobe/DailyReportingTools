using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class Button
    {
        public Uri Url { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }

        public Button()
        {

        }

        public Button(Uri url, string text, string color)
        {
            Url = url;
            Text = text;
            Color = color;
        }
    }    
}
