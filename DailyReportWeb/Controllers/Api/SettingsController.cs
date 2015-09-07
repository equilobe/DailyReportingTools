using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class SettingsController : ApiController
    {
        public ISettingsService SettingsService { get; set; }
        public IDataService DataService { get; set; }

        public FullReportSettings Get(long id)
        {
            if (!DataService.IsInstanceActive(id))
                return null;

            return SettingsService.GetSyncedReportSettings(new ItemContext(id));
        }

        public void Post([FromBody]FullReportSettings updatedFullSettings)
        {
            if (!Validations.Time(updatedFullSettings.ReportTime) ||
                !Validations.Mails(updatedFullSettings.DraftEmails) ||
                !Validations.Mails(updatedFullSettings.Emails) ||
                (updatedFullSettings.SourceControlOptions.Type == SourceControlType.SVN && !Validations.Url(updatedFullSettings.SourceControlOptions.Repo)))
                throw new ArgumentException();

            SettingsService.SetFullReportSettings(updatedFullSettings);
        }
    }
}
