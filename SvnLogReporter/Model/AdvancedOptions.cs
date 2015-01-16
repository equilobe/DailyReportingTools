using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{

    public class AdvancedOptions
    {
        [XmlAttribute]
        public bool NoDraft { get; set; }

        [XmlAttribute]
        public bool NoIndividualDraft { get; set; }

        public string ReportTitle { get; set; }

        [DefaultValue("permanent")]
        public string PermanentTaskLabel { get; set; }

        [DefaultValue("Reopened")]
        public string ReopenedStatus { get; set; }

        public string AdditionalWorkflowStatuses { get; set; }

        public List<string> WorkflowStatuses
        {
            get
            {
                if (AdditionalWorkflowStatuses == null)
                    return null;

                return AdditionalWorkflowStatuses.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        [DefaultValue("Saturday Sunday")]
        public string WeekendDays { get; set; }

        [XmlIgnore]
        public List<DayOfWeek> WeekendDaysList
        {
            get
            {
                var daysList = WeekendDays.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var weekendDaysEnum = new List<DayOfWeek>();
                try
                {
                    foreach (var day in daysList)
                    {
                        var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
                        weekendDaysEnum.Add(dayOfWeek);
                    }
                    return weekendDaysEnum;
                }
                catch (Exception)
                {
                    return new List<DayOfWeek>(){
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    };
                }
            }
        }

        public AdvancedOptions()
        {
            PermanentTaskLabel = "permanent";
            WeekendDays = "Saturday Sunday";
            ReopenedStatus = "Reopened";
        }
    }
}
