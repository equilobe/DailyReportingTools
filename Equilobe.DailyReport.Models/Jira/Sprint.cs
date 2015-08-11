﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class Sprint
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int sequence { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public int linkedPagesCount { get; set; }
        [DataMember]
        public string startDate { get; set; }
        [DataMember]
        public string endDate { get; set; }
        [DataMember]
        public string completeDate { get; set; }

        public DateTime? StartDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(startDate);
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }
        public DateTime? EndDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(endDate);
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? CompletedDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(completeDate);
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }
    }
}
