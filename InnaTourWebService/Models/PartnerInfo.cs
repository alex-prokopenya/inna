using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Megatec.MasterTour.BusinessRules;

namespace InnaTourWebService.Models
{
    [XmlRoot("PartnerInfo")]
    public class PartnerInfo
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Status")]
        public string Status;

        [XmlArray("Properties")]
        public int[] Properties;

        [XmlAttribute("RegisterSeries")]
        public string RegisterSeries;

        [XmlAttribute("RegisterNumber")]
        public string RegisterNumber;

        [XmlAttribute("RegistredAddress")]
        public string RegistredAddress;

        [XmlAttribute("RegistredAddressIndex")]
        public string RegistredAddressIndex;

        [XmlAttribute("Address")]
        public string Address;

        [XmlAttribute("PostIndex")]
        public string PostIndex;

        [XmlAttribute("Boss")]
        public string Boss;

        [XmlAttribute("BossName")]
        public string BossName;

        [XmlAttribute("INN")]
        public string INN;

        [XmlAttribute("KPP")]
        public string KPP;

        [XmlAttribute("Email")]
        public string Email;

        [XmlAttribute("Site")]
        public string Site;

        [XmlAttribute("Phones")]
        public string Phones;

        [XmlAttribute("Fax")]
        public string Fax;

        /// <summary>
        /// проверяет данные полученные в объект
        /// </summary>
        /// <returns>если все ок, возвращает true</returns>
        public bool Validate()
        {
            var errors = new List<string>();

            //проверяем название
            if (string.IsNullOrEmpty(this.Name))
            {
                errors.Add("invalid Partner Name");   
            }
            //если есть ошибки, выводим
            if (errors.Count > 0)
            {
                var sw = new StringWriter();
                new XmlSerializer(this.GetType()).Serialize(sw, this);

                throw new Exception(string.Format("Invalid object PartnerInfo from {0}. Finded errors: {1}.", sw.ToString(), string.Join(";", errors)));
            }

            return true;
        }

    }
}