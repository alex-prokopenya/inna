using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace InnaTourWebService.Models
{
    public struct DogovorInfo
    {
        [XmlAttribute("TourDate")]
        public string tourDate;

        [XmlAttribute("DogovorKey")]
        public int dgKey;

        [XmlAttribute("DogovorCode")]
        public string dgCode;

        [XmlAttribute("StatusKey")]
        public int statusKey;

        [XmlAttribute("StatusName")]
        public string statusName;

        [XmlAttribute("AgentKey")]
        public int agentKey;

        [XmlAttribute("AgentLogin")]
        public string agentLogin;

        [XmlAttribute("Price")]
        public decimal price;

        [XmlAttribute("Paid")]
        public decimal paid;

        [XmlAttribute("RateIsoCode")]
        public string rateIsoCode;
    }
}