using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;


namespace InnaTourWebService.Models
{
    public class Response
    {
        [XmlElement(Type = typeof(String), ElementName = "result")]
        [XmlElement(Type = typeof(DepositInfo[]), ElementName = "Deposits")]
        public object value;

        [XmlElement("hasErrors")]
        public bool hasErrors = false;

        [XmlElement("errorMessage")]
        public string errorMessage = "";
    }
}