using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;


namespace InnaTourWebService.Models
{
    [XmlRoot("InService")]
    public class InService
    {
       [XmlAttribute("ServiceType")]
       public ServiceType ServiceType;
       
       [XmlAttribute("Date")]
       public string Date;
       
       [XmlAttribute("NDays")]
       public int NDays;
       
       [XmlAttribute("Title")]
       public string Title;
       
       [XmlAttribute("PartnerID")]
       public int PartnerID;
       
       [XmlAttribute("PartnerBookID")]
       public string PartnerBookID;
       
       [XmlAttribute("Price")]
       public int Price;
       
       [XmlAttribute("NettoPrice")]
       public int NettoPrice;
       
       [XmlAttribute("Comission")]
       public int Comission;
       
       [XmlArray("TuristIndexes")]
       public int[] TuristIndexes;

        public bool Validate(int turistsCnt)
        {
            var errors = new List<string>();

            Regex mailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            UserInfo userInfo = null;

            //партнер-поставщик
            var mtHelper = new DataBase.MasterTour();

            userInfo = mtHelper.GetUserInfoForAgent(this.PartnerID);

            if (userInfo == null)
                errors.Add("unknown PartnerID");

            int minLong = 1;

            //продолжительность услуги
            if (this.ServiceType == ServiceType.HOTEL)
                minLong = 2;

            if (minLong > this.NDays)
                errors.Add("invalid service NDays");

            //название услуги
            if (this.Title.Length == 0)
                errors.Add("invalid title");
            else if (this.Title.Length > 50)
                this.Title = this.Title.Substring(0, 50);

            //стоимость, комиссия, нетто
            if(this.Price < 0)
                errors.Add("invalid Price");

            if (this.NettoPrice < 0)
                errors.Add("invalid NettoPrice");

            if (this.Comission < 0)
                errors.Add("invalid Comission");

            //индексы туристов
            var usedIndexes = new List<int>();
            foreach(int index in this.TuristIndexes)
                if ((index < 1) || (index > turistsCnt) || (usedIndexes.Contains(index)))
                {
                    errors.Add("invalid TuristIndexes array");
                    break;
                }
                else
                {
                    usedIndexes.Add(index);
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