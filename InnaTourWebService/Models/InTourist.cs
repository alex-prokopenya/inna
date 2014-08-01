using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Configuration;
namespace InnaTourWebService.Models
{
    [XmlRoot("InTourist")]
    public class InTourist
    {
        [XmlAttribute("FirstName")]
        public string FirstName;

        [XmlAttribute("Sex")]
        public Sex Sex;

        [XmlAttribute("LastName")]
        public string LastName;

        [XmlAttribute("BirthDate")]
        public string BirthDate;

        [XmlAttribute("PassrortType")]
        public PasportType PassrortType;

        [XmlAttribute("PasspordNumber")]
        public string PasspordNumber;

        [XmlAttribute("PasspordCode")]
        public string PasspordCode;

        [XmlAttribute("PassportValidDate")]
        public string PassportValidDate;

        [XmlAttribute("Citizenship")]
        public string Citizenship;

        /// <summary>
        /// проверяет данные полученные в объект
        /// </summary>
        /// <returns>если все ок, возвращает true</returns>
        public bool Validate()
        {
            var errors = new List<string>();

            var latin = new Regex(@"^[a-zA-Z\-]{2,50}$");

            //проверяем имя
            if (!latin.IsMatch(this.FirstName))
                errors.Add("invalid FirstName");

            //проверяем фамилию
            if (!latin.IsMatch(this.LastName))
                errors.Add("invalid LastName");

            //проверяем гражданство
            if (this.Citizenship.Length != 2)
                errors.Add("invalid Citizenship");

            //проверяем дату рождения
            DateTime dateRes = DateTime.Today;

            if (DateTime.TryParseExact(this.BirthDate, ConfigurationManager.AppSettings["DatesFormat"], CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes))
            {
                if (dateRes>= DateTime.Today)
                    errors.Add("invalid BirthDate");
            }
            else
                errors.Add("invalid BirthDate format");

            //проверяем срок действия паспорта
            if (this.PassportValidDate != "")
            {
                if (DateTime.TryParseExact(this.PassportValidDate, ConfigurationManager.AppSettings["DatesFormat"], CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes))
                {
                    if (dateRes <= DateTime.Today)
                        errors.Add("invalid PassportValidDate");
                }
                else
                    errors.Add("invalid PassportValidDate format");
            }
            //если есть ошибки, выводим
            if (errors.Count > 0)
            {
                var sw = new StringWriter();
                new XmlSerializer(this.GetType()).Serialize(sw, this);

                throw new Exception(string.Format("Invalid object InTurist from {0}. Finded errors: {1}.", sw.ToString(), string.Join(";",errors)));
            }

            return true;
        }

    }
}