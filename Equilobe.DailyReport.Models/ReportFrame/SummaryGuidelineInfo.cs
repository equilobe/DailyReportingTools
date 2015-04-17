using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class SummaryGuidelineInfo
    {
        public int GuidelinesRate { get; set; }
        public double GuidelineWidth { get; set; }
        public string GuidelineWidthString
        {
            get
            {
                return GuidelineWidth.ToString() + "px";
            }
        }

        public string GuidelineCounterMarginLeft
        {
            get
            {
                return (GuidelineWidth / 2).ToString() + "px";
            }
        }

        public double GuidelineWidthSmall
        {
            get
            {
                return GuidelineWidth / 2;
            }
        }

        public int GuidelinesCount { get; set; }

        public readonly int ChartMaxWidth = 300;
        public string ChartMaxWidthString
        {
            get
            {
                return ChartMaxWidth.ToString() + "px";
            }
        }
    }
}
