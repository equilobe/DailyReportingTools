﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    public class User
    {
        public string JiraUserKey { get; set; }
        public List<string> SourceControlUsernames { get; set; }
        public string EmailOverride { get; set; }

        [XmlAttribute]
        public bool Ignored { get; set; }
    }
}
