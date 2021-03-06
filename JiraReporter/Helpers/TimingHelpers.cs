﻿using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Helpers
{
    class TimingHelpers
    {
        public static void SetAverageWorkStringFormat(Timing timing)
        {
            timing.AverageWorkedMonthString = (timing.AverageWorkedMonth / 3600).RoundDoubleOneDecimal();
            timing.AverageWorkedSprintString = (timing.AverageWorkedSprint / 3600).RoundDoubleOneDecimal();
            timing.AverageWorkedLast7DaysString = (timing.AverageWorkedLast7Days / 3600).RoundDoubleOneDecimal();
            timing.AverageWorkedString = (timing.AverageWorked / 3600).RoundDoubleOneDecimal();
        }

        public static void SetAverageRemainingStringFormat(TimingDetailed timing)
        {
            timing.RemainingSprintAverageString = (timing.RemainingSprintAverage / 3600).RoundDoubleOneDecimal();
            timing.RemainingMonthAverageString = (timing.RemainingMonthAverage / 3600).RoundDoubleOneDecimal();
        }
    }
}
