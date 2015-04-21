using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class UserImageContext
    {
        public string Username { get; set; }
        public long InstanceId { get; set; }
        public byte[] Image { get; set; }
    }
}
