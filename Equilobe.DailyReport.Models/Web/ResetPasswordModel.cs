using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Web
{
    public class ResetPasswordModel
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
        public string Code { get; set; }
    }
}
