using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
   public class StatusChartWidths
    {
       public double DaySeconds { get; set; }
       public double DayWidth { get; set; }
       public string DayWidthString
       {
           get
           {
               return DayWidth.ToString();
           }
       }
       public string DayWidthPx
       {
           get
           {
               return DayWidth.ToString() + "px";
           }
       }

       public double EstimatedSeconds { get; set; }
       public double EstimatedWidth { get; set; }
       public string EstimatedWidthString
       {
           get
           {
               return EstimatedWidth.ToString();
           }
       }
       public string EstimatedWidthPx
       {
           get
           {
               return EstimatedWidth.ToString() + "px";
           }
       }

       public double DoneSeconds { get; set; }
       public double DoneWidth { get; set; }
       public string DoneWidthString
       {
           get
           {
               return DoneWidth.ToString();
           }
       }
       public string DoneWidthPx
       {
           get
           {
               return DoneWidth.ToString() + "px";
           }
       }

       public double RemainingSeconds { get; set; }
       public double RemainingWidth { get; set; }
       public string RemainingWidthString
       {
           get
           {
               return RemainingWidth.ToString();
           }
       }
       public string RemainingWidthPx
       {
           get
           {
               return RemainingWidth.ToString() + "px";
           }
       }
    }
}
