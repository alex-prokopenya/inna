using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using InnaTourWebService.Models;
using InnaTourWebService.DataBase;
using InnaTourWebService.Helpers;

namespace InnaTourWebService
{
    /// <summary>
    /// Summary description for BookService
    /// </summary>
    
    [WebService(Namespace = "http://inna.ru/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public partial class BookService : System.Web.Services.WebService
    {
        /// <summary>
        /// Созздает бронь в Мастер-Туре.
        /// </summary>
        /// <returns>DogovorCode -- код созданной брони</returns>
        [WebMethod]
        public Response CreateDogovor(InTourist[] turists, UserInfo userInfo, InService[] services)
        {
            try
            {
                //verify
                turists.All(item => item.Validate());

                userInfo.Validate();

                services.All(item => item.Validate(turists.Length));

                //создание путевки
                var mtHelper = new MasterTour();

                return new Response()
                {
                    value = mtHelper.CreateNewDogovor(turists, userInfo, services)
                };
            }
            catch (Exception ex)
            {
                Logger.ReportException(ex); //пишем в лог ошибки

                return new Response() //отдаем ответ с ошибкой
                {
                    hasErrors = true,
                    errorMessage = ex.Message + " " + ex.StackTrace
                };
            }
        }

         /// <summary>
        /// Проводит платеж брони
        /// </summary>
        /// <returns>PaymentId -- идентификатор созданной проводки</returns>
        [WebMethod]
        public Response CreateDogovorDepositPayment(string dogovorCode)
        {
            return CreateDogovorPayment(dogovorCode, 0, "deposit", 0m, "0");
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

                if ((paymentSys != "deposit") && (dogovor.PartnerKey > 0))
                    throw new Exception(String.Format("only deposit payment awailable"));
                else
                    if ((paidSum <= 0) && (dogovor.PartnerKey == 0))
                        throw new Exception(String.Format("paidSum <= 0"));

                if (dogovor.PartnerKey > 0)  // если заказ агентский
                    masterFinanceHelper.DepositPayDogovor(dogovor.PartnerKey, dogovor.Key); //платим депозитом
                else //иначе
                    masterFinanceHelper.PayDogovor(dogovor.Code,
                                                    paymentType,
                                                    paymentSys,
                                                    paidSum,
                                                    paymentId); // сохраняем платеж по карте

                return new Response()
                {
                    value =  "success"
                };
            }
            catch (Exception ex) 
            {
                Logger.ReportException(ex);

                return new Response()
                {
                    hasErrors = true,
                    errorMessage = ex.Message + " " + ex.StackTrace
                };
            }

        }

        /// <summary>
        /// Возвращает информацию о депозите и кредитном лимите агента
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public Response GetDepositAndReceivable(int partnerId)
        {
            //return new Response()
            //{
            //    value = new DepositInfo[] { new DepositInfo(){Deposit=112, RateIsoCode="BYR"}}
            //};

                try{
                    var masterFinance = new DataBase.MasterFinance();

                    return new Response(){ 
                     value = masterFinance.CallDepositGetValue(partnerId)
                    };
                }
                catch(Exception ex)
                {
                    Logger.ReportException(ex);
                    
                    return new Response(){ 
                     hasErrors = true,
                     errorMessage = ex.Message + " " + ex.StackTrace
                    };
                }
        }

    }
}
