using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Views
{
    public class Spacing
    {
        public int Height { get; set; }
        public string HeightPx
        {
            get
            {
                return Height + "px";
            }
        }
        public int Colspan { get; set; }

        public Spacing(int height, int colspan)
        {
            Height = height;
            Colspan = colspan;
        }
    }
}
