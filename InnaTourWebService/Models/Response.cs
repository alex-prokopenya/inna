﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;


namespace InnaTourWebService.Models
{
    public class Response
    {
        [XmlElement(Type = typeof(String), ElementName = "result")]
        [XmlElement(Type = typeof(int), ElementName = "new_id")]
        [XmlElement(Type = typeof(DepositInfo[]), ElementName = "Deposits")]
        [XmlElement(Type = typeof(DogovorInfo), ElementName = "dogovorInfo")]
        [XmlElement(Type = typeof(ReportResponse), ElementName = "report")]

        public object value;

        [XmlElement("hasErrors")]
        public bool hasErrors = false;

        [XmlElement("errorMessage")]
        public string errorMessage = "";
    }
}