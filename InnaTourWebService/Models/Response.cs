using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;


namespace InnaTourWebService.Models
{
    public class Response
    {
        [XmlElement("value")]
        public object value;

        [XmlElement("hasErrors")]
        public bool hasErrors;

        [XmlElement("errorMessage")]
        public string errorMessage;
    }
}