using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InnaTourWebService.Models
{
    /// <summary>
    /// Информация о депозите агента
    /// </summary>
    public class DepositInfo
    {
        private decimal deposit;

        public decimal Deposit
        {
            get { return deposit; }
            set { deposit = value; }
        }


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