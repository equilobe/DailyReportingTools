using Equilobe.DailyReport.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models
{
    public class ItemContext : UserContext
    {
        public ItemContext(long id)
        {
            Id = id;
        }

        public long Id { get; set; }
    }
}
