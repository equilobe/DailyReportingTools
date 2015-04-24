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
        public double Width { get; set; }
        public double BarMaxWidth { get; set; }
        public string Text { get; set; }

        public ChartBar(string color, double width, double barMaxWidth, string text)
        {
            Color = color;
            Width = width;
            BarMaxWidth = barMaxWidth;
            Text = text;
        }
    }
}
