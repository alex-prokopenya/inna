using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace InnaTourWebService.Models
{
    [XmlRoot("UserInfo")]
    public class UserInfo
    {
        [XmlAttribute("AgentKey")]
        public int AgentKey = 0;

        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Phone")]
        public string Email;

        [XmlAttribute("LastName")]
        public string Phone;

        /// <summary>
        /// проверяет данные полученные в объект
        /// </summary>
        /// <returns>если все ок, возвращает true</returns>
        public bool Validate()
        {
            var errors = new List<string>();

            Regex mailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            UserInfo userInfo = null;

            if (this.AgentKey > 0)
            { 
                var mtHelper = new DataBase.MasterTour();

                userInfo = mtHelper.GetUserInfoForAgent(this.AgentKey);
            }

            if (!mailRegex.IsMatch(this.Email))
            { 
                if(userInfo == null)
                    errors.Add("invalid Email");
                else
                    this.Email = userInfo.Email;
            }

            if (!new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$").IsMatch(this.Phone))
            {
                if (userInfo == null)
                    errors.Add("invalid Phone");
                else
                    this.Phone = userInfo.Phone;
            }

            if (this.Name.Length < 1)
            {
                if (userInfo == null)
                    errors.Add("invalid Name");
                else
                    this.Name = userInfo.Name;
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