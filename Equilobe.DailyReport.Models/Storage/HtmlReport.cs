using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class HtmlReport
    {
        public long Id { get; set; }
        public long BasicSettingsId { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; }
        public string AccesToken { get; set; }
        public string Title { get; set; }
        public string UniqueUserKey { get; set; }
        public string HtmlContent { get; set; }

        public virtual BasicSettings BasicSettings { get; set; }
    }
}
