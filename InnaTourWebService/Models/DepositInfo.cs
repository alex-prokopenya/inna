using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace InnaTourWebService.Models
{
    /// <summary>
    /// Информация о депозите агента
    /// </summary>
    public class DepositInfo
    {
        [XmlAttribute("deposit")]
        private decimal deposit;

        public decimal Deposit
        {
            get { return deposit; }
            set { deposit = value; }
        }

         [XmlAttribute("rate")]
        private string rateIsoCode;

        public string RateIsoCode
        {
            get { return rateIsoCode; }
            set { rateIsoCode = value; }
        }


        public DepositInfo() 
        {


        }
    }
}