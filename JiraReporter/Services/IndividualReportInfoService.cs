using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Services
{
    class IndividualReportInfoService
    {
        public void SetIndividualDraftInfo(List<JiraAuthor> authors, JiraReport context)
        {
            if (!context.IsIndividualDraft)
                return;

            foreach (var author in authors)
            {
                var individualDraft = GenerateIndividualDraftInfo(author, context);
                author.IndividualDraftInfo = individualDraft;
            }
        }

        public IndividualDraftInfo GetIndividualDraftInfo(JiraReport context)
        {
            var draftConfirmation = new IndividualDraftConfirmation();

            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.UniqueProjectKey);
                draftConfirmation = report.IndividualDraftConfirmations.SingleOrDefault(dr => dr.UniqueUserKey == context.ExecutionInstance.UniqueUserKey);
            }

            var draft = new IndividualDraftInfo();
            draftConfirmation.CopyPropertiesOnObjects(draft);
            //{
            //    UserKey = draftConfirmation.UniqueUserKey,
            //    Username = draftConfirmation.Username,
            //    IsLead = draftConfirmation.IsProjectLead
            //};

            if (draftConfirmation.LastDateConfirmed != null)
                draft.LastConfirmationDate = draftConfirmation.LastDateConfirmed.Value;

            SetIndividualUrls(draft, context);

            return draft;
        }

        private IndividualDraftInfo GenerateIndividualDraftInfo(JiraAuthor author, JiraReport context)
        {
            var individualDraft = new IndividualDraftInfo
            {
                Username = author.Username,
                UniqueUserKey = RandomString.Get(),
                IsProjectLead = author.IsProjectLead,
                ReportDate = context.ToDate
            };
            SetIndividualUrls(individualDraft, context);

            return individualDraft;
        }

        private void SetIndividualUrls(IndividualDraftInfo individualDraft, JiraReport context)
        {
            individualDraft.ConfirmationDraftUrl = GetUrl(individualDraft, context.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = GetUrl(individualDraft, context.SendIndividualDraftUrl);
            if (individualDraft.IsProjectLead)
                individualDraft.ForceDraftUrl = GetUrl(individualDraft, context.SendDraftUrl);
        }

        private Uri GetUrl(IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UniqueUserKey);

            return new Uri(baseUrl + "&" + url);
        }
    }
}
