using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using InnaTourWebService.Models;
using InnaTourWebService.DataBase;

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
    public partial class BookService2 : System.Web.Services.WebService
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
        public Response CreateDogovorPayment(string dogovorCode, int paymentType, string paymentSys, decimal paidSum, string paymentId)
        {
            try
            {
                var masterHelper = new MasterTour();
                var masterFinanceHelper = new MasterFinance();

                var dogovor = masterHelper.GetDogovorByCode(dogovorCode); //ищем путевку по коду

                if (dogovor == null)
                    throw new Exception(String.Format("Dogovor '{0}' not founded", dogovorCode));

                if (dogovor.PartnerKey > 0)  // если заказ агентский
                    masterFinanceHelper.DepositPayDogovor(dogovor.PartnerKey, dogovor.Key); //платим депозитом
                else //иначе
                    masterFinanceHelper.PayDogovor(dogovor.Code,
                                                    paymentType,
                                                    paymentSys,
                                                    paidSum,
                                                    paymentId); // сохраняем платеж по карте
            }
            catch (Exception ex)
            {
                return new Response() { value = "", 
                                        hasErrors = true,
                                        errorMessage = ex.Message + "\n" + ex.StackTrace };
            }

            return new Response() { value = "success",
                                    hasErrors = false,
                                    errorMessage = ""};
        }

        /// <summary>
        /// Возвращает информацию о депозите и кредитном лимите агента
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public DepositInfo[] GetDepositAndReceivable(int partnerId)
        {
            //throw new Exception("test");

            return new DepositInfo[] { new DepositInfo() { RateIsoCode = "11", Deposit = 111 } };
            /*

            return new Response()
            {
                value = new DepositInfo[]{new DepositInfo(){RateIsoCode = "11", Deposit = 111}},
                hasErrors = false,
                errorMessage = ""
            };

            try
            {
                var masterFinance = new DataBase.MasterFinance();

                return new Response()
                { 
                    value = masterFinance.CallDepositGetValue(partnerId),
                    hasErrors = false,
                    errorMessage = ""
                };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    value = "",
                    hasErrors = true,
                    errorMessage = ex.Message + "\n" + ex.StackTrace
                };
            }
             */
        }
    }
}
