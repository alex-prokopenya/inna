﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace InnaTourWebService.Models
{
    [XmlRoot("UserInfo")]
    public class UserInfo
    {
        [XmlAttribute("AgentKey")]
        public int AgentKey = 0;

        [XmlAttribute("Name")]
        public string Name;
     
        [XmlAttribute("Email")]
        public string Email;

        [XmlAttribute("Phone")]
        public string Phone;

        /// <summary>
        /// проверяет данные полученные в объект
        /// </summary>
        /// <returns>если все ок, возвращает true</returns>
        public bool Validate()
        {
            var errors = new List<string>();

            Regex mailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

          //  int partnerKey = 0;

          /*  if (this.AgentLogin != "")
            { 
                var mtHelper = new DataBase.MasterTour();

                partnerKey = mtHelper.GetAgentInfo(this.AgentLogin);
            }
            */

            if (this.AgentKey == 0)
            {
                if (!mailRegex.IsMatch(this.Email))
                    errors.Add("invalid Email");

                if (!new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$").IsMatch(this.Phone))
                    errors.Add("invalid Phone");

                if (this.Name == null)
                    this.Name = "";
            }

            if (errors.Count > 0)
            {
                var sw = new StringWriter();
                new XmlSerializer(this.GetType()).Serialize(sw, this);

                throw new Exception(string.Format("Invalid object UserInfo from {0}. Finded errors: {1}.", sw.ToString(), string.Join(";", errors)));
            }

            return true;
        }
    }
}