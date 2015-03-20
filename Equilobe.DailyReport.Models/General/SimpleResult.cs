using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models
{
    public class SimpleResult
    {
        public static SimpleResult Success(string message)
        {
            return new SimpleResult
            {
                Message = message,
            };
        }

        public static SimpleResult Error(string message)
        {
            return new SimpleResult
            {
                Message = message,
                HasError = true
            };
        }

        public bool HasError { get; set; }
        public string Message { get; set; }
    }
}
