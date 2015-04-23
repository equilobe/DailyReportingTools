using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class ChartElement
    {
        public string ActualValue { get; set; }
        public double ActualValueSeconds { get; set; }
        public double Width { get; set; }
        public string WidthPx
        {
            get
            {
                return Width.ToString() + "px";
            }
        }
    }
}
