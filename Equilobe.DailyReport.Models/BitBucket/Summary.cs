using System;
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
                try
                {
                    return (Markup)Enum.Parse(typeof(Markup), MarkupType);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
