using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Views
{
    public class ChartBar
    {
        public string Color { get; set; }
        public string Width { get; set; }
        public string BarMaxWidth { get; set; }
        public string Text { get; set; }

        public ChartBar(string color, string width, string barMaxWidth, string text)
        {
            Color = color;
            Width = width;
            BarMaxWidth = barMaxWidth;
            Text = text;
        }
    }
}
