using Equilobe.DailyReport.DAL;
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

           // var individualDrafts = new List<IndividualDraftInfo>();

            foreach (var author in authors)
            {
                var individualDraft = GenerateIndividualDraftInfo(author, context);
         //       individualDrafts.Add(individualDraft);
                author.IndividualDraftInfo = individualDraft;
            }

     //       context.IndividualDrafts = individualDrafts;
        }

        private IndividualDraftInfo GenerateIndividualDraftInfo(JiraAuthor author, JiraReport context)
        {
            var individualDraft = new IndividualDraftInfo
            {
                Username = author.Username,
                UserKey = RandomGenerator.GetRandomString(),
                IsLead = author.IsProjectLead
            };
            SetIndividualUrls(individualDraft, context);

            return individualDraft;
        }

        private void SetIndividualUrls(IndividualDraftInfo individualDraft, JiraReport context)
        {
            individualDraft.ConfirmationDraftUrl = GetUrl(individualDraft, context.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = GetUrl(individualDraft, context.SendIndividualDraftUrl);
            if (individualDraft.IsLead)
                individualDraft.ForceDraftUrl = GetUrl(individualDraft, context.SendDraftUrl);
        }

        private static Uri GetUrl(IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UserKey);

            return new Uri(baseUrl + "&" + url);
        }

        public IndividualDraftInfo GetIndividualDraftInfo(JiraReport context)
        {
            var draftConfirmation= new IndividualDraftConfirmation();

            using(var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == context.UniqueProjectKey);
                draftConfirmation = report.IndividualDraftConfirmations.SingleOrDefault(dr => dr.UniqueUserKey == context.ExecutionInstance.UniqueUserKey);
            }

            var draft = new IndividualDraftInfo
            {
                UserKey = draftConfirmation.UniqueUserKey,
                Username = draftConfirmation.Username,
                IsLead = draftConfirmation.IsProjectLead,
                LastConfirmationDate = draftConfirmation.LastDateConfirmed.Value
            };

            SetIndividualUrls(draft, context);

            return draft;
        }
    }
}
