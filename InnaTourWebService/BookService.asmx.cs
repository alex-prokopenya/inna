using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace InnaTourWebService
{
    /// <summary>
    /// Summary description for BookService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class BookService : System.Web.Services.WebService
    {

        /// <summary>
        /// Созздает бронь в Мастер-Туре.
        /// </summary>
        /// <returns>DogovorCode -- код созданной брони</returns>
        [WebMethod]
        public string CreateDogovor()
        {
            return "Hello World!";
        }


        /// <summary>
        /// Проводит платеж брони
        /// </summary>
        /// <returns>PaymentId -- идентификатор созданной проводки</returns>
        [WebMethod]
        public string CreateDogovorPayment()
        {
            return "Hello World!";
        }


        /// <summary>
        /// Возвращает информацию о депозите и кредитном лимите агента
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string GetDepositAndReceivable()
        {
            return "Hello World";
        }
    }
}
