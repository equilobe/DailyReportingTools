using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
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
        public void SetIndividualDraftInfo(List<JiraAuthor> authors, JiraPolicy policy)
        {
            if (!policy.GeneratedProperties.IsIndividualDraft)
                return;

            var individualDrafts = new List<IndividualDraftInfo>();

            foreach (var author in authors)
            {
                var individualDraft = GenerateIndividualDraftInfo(author, policy);
                individualDrafts.Add(individualDraft);
                author.IndividualDraftInfo = individualDraft;
            }

            policy.GeneratedProperties.IndividualDrafts = individualDrafts;
        }

        private IndividualDraftInfo GenerateIndividualDraftInfo(JiraAuthor author, JiraPolicy policy)
        {
            var individualDraft = new IndividualDraftInfo
            {
                Name = author.Name,
                Username = author.Username,
                UserKey = RandomGenerator.GetRandomString(),
                IsLead = author.IsProjectLead
            };
            SetIndividualUrls(individualDraft, policy);

            return individualDraft;
        }

        private void SetIndividualUrls(IndividualDraftInfo individualDraft, JiraPolicy policy)
        {
            individualDraft.ConfirmationDraftUrl = GetUrl(individualDraft, policy.GeneratedProperties.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = GetUrl(individualDraft, policy.GeneratedProperties.ResendIndividualDraftUrl);
            if (individualDraft.IsLead)
                individualDraft.ForceDraftUrl = GetUrl(individualDraft, policy.GeneratedProperties.ResendDraftUrl);
        }

        private static Uri GetUrl(IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UserKey);

            return new Uri(baseUrl + "&" + url);
        }

        public IndividualDraftInfo GetIndividualDraftInfo(string key, JiraPolicy policy)
        {
            var draft = policy.GeneratedProperties.IndividualDrafts.Single(d => d.UserKey == key);
            SetIndividualUrls(draft, policy);

            return draft;
        }
    }
}
