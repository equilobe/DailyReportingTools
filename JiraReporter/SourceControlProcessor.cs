﻿using SvnLogReporter;
using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class SourceControlProcessor
    {
        public static readonly Dictionary<SourceControlType, Func<Policy, Options, ReportBase>> Processors = new Dictionary<SourceControlType, Func<Policy, Options, ReportBase>>()
        {
            {SourceControlType.GitHub, ReportBase.Create<GitHubReport> },
            {SourceControlType.SVN, ReportBase.Create<SvnReport>}
        };
    }
}