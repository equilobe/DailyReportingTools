using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class IndividualReportInfoService
    {
        public void SetIndividualDraftInfo(List<Author> authors, Policy policy)
        {
            if (!policy.GeneratedProperties.IsIndividualDraft)
                return;

            var individualDrafts = new List<SourceControlLogReporter.Model.IndividualDraftInfo>();

            foreach (var author in authors)
            {
                var individualDraft = GenerateIndividualDraftInfo(author, policy);
                individualDrafts.Add(individualDraft);
                author.IndividualDraftInfo = individualDraft;
            }

            policy.GeneratedProperties.IndividualDrafts = individualDrafts;
        }

        private SourceControlLogReporter.Model.IndividualDraftInfo GenerateIndividualDraftInfo(Author author, Policy policy)
        {
            var individualDraft = new SourceControlLogReporter.Model.IndividualDraftInfo
            {
                Name = author.Name,
                Username = author.Username,
                UserKey = RandomGenerator.GetRandomString(),
                IsLead = author.IsProjectLead
            };
            SetIndividualUrls(individualDraft, policy);

            return individualDraft;
        }

        private void SetIndividualUrls(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, Policy policy)
        {
            individualDraft.ConfirmationDraftUrl = GetUrl(individualDraft, policy.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = GetUrl(individualDraft, policy.ResendIndividualDraft);
            if (individualDraft.IsLead)
                individualDraft.ForceDraftUrl = GetUrl(individualDraft, policy.ResendDraftUrl);
        }

        private static Uri GetUrl(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UserKey);

            return new Uri(baseUrl + "&" + url);
        }

        public SourceControlLogReporter.Model.IndividualDraftInfo GetIndividualDraftInfo(string key, Policy policy)
        {
            var draft = policy.GeneratedProperties.IndividualDrafts.Single(d => d.UserKey == key);
            SetIndividualUrls(draft, policy);

            return draft;
        }
    }
}
