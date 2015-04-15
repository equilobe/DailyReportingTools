using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class UserImage
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public byte[] ImageContent{ get; set; }
        public string Key { get; set; }
        public string Username{ get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
    }
}
