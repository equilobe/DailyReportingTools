using Equilobe.DailyReport.Models.Enums;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Summary
    {
        [DataMember(Name = "raw")]
        public string Raw { get; set; }

        [DataMember(Name = "markup")]
        private string MarkupType { get; set; }

        [DataMember(Name = "html")]
        public string Html { get; set; }

        public Markup? Markup
        {
            get
            {
                return MarkupType.ToEnum<Markup>(true);
            }
        }
    }
}
