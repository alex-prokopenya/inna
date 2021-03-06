﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Globalization;


namespace InnaTourWebService.Models
{
    [XmlRoot("InService")]
    public class InService : IComparable<InService>
    {
        public int CompareTo(InService other) //для сортировки
        {
            if (other == null)
                return 1;

            if (other.Date == this.Date)
                return other.NDays - this.NDays;

            return this.Date.CompareTo(other.Date);
        }

       [XmlAttribute("ServiceType")]
       public ServiceType ServiceType;

       [XmlAttribute("ServiceKey")]
       public int ServiceKey;

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

       [XmlArray("NumDocs")]
       public string[] NumDocs;

        public bool Validate(int turistsCnt)
        {
            var errors = new List<string>();

            Regex mailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            UserInfo userInfo = null;

            //партнер-поставщик
            var mtHelper = new DataBase.MasterTour();

            userInfo = mtHelper.GetPartnerInfo(this.PartnerID);

            if (userInfo == null)
                errors.Add("unknown PartnerID");

            //дата услуги
            DateTime dateRes = DateTime.Today.AddDays(-1);

            if (DateTime.TryParseExact(this.Date, ConfigurationManager.AppSettings["DatesFormat"], CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes))
            {
                if (dateRes < DateTime.Today)
                    errors.Add("invalid Service Date");
            }
            else
                errors.Add("invalid Service Date format");
            
            int minLong = 1;

            //продолжительность услуги
            if (this.ServiceType == ServiceType.HOTEL)
                minLong = 2;

            if (minLong > this.NDays)
                errors.Add("invalid service NDays");

            if (this.ServiceKey < 1)
                errors.Add("invalid ServiceKey");

            //название услуги
            if ( String.IsNullOrEmpty(this.Title) )
                errors.Add("invalid title");
            else if (this.Title.Length > 50)
                this.Title = this.Title.Substring(0, 50);

            ////стоимость, комиссия, нетто
            //if(this.Price < 0)
            //    errors.Add("invalid Price");

            //if (this.NettoPrice < 0)
            //    errors.Add("invalid NettoPrice");

            //if (this.Comission < 0)
            //    errors.Add("invalid Comission");

            if (this.PartnerBookID == null)
                this.PartnerBookID = string.Empty;

            //индексы туристов
            if (this.TuristIndexes == null)
                this.TuristIndexes = new int[0];

            var usedIndexes = new List<int>();

            foreach(int index in this.TuristIndexes)
               if ((index < 1) || (index > turistsCnt) || (usedIndexes.Contains(index)))
               {
                   errors.Add("invalid TuristIndexes array");
                   break;
               }
               else
                   usedIndexes.Add(index);

            //номера билетов
            if (this.NumDocs != null)
            {
                for (int i = 0; i < this.NumDocs.Length; i++)
                    if (this.NumDocs[i].Length > 30)
                        this.NumDocs[i] = this.NumDocs[i].Substring(0, 30);
            }
            else
                this.NumDocs = new string[0];

            if (errors.Count > 0)
            {
                var sw = new StringWriter();
                new XmlSerializer(this.GetType()).Serialize(sw, this);

                throw new Exception(string.Format("Invalid object InService from {0}. Finded errors: {1}.", sw.ToString(), string.Join(";", errors)));
            }

            return true;
        }
    }
}