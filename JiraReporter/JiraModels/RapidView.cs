﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class RapidView
    {
        [DataMember]
        public int rapidViewId { get; set; }
        [DataMember]
        public SprintsData sprintsData { get; set; }
    }
}
