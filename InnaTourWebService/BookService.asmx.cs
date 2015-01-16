using InnaTourWebService.DataBase;
using InnaTourWebService.Helpers;
using InnaTourWebService.Models;
using Megatec.Common.BusinessRules.Base;
using Megatec.MasterTour.BusinessRules;
using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Web.Services;
using System.Xml.Serialization;

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

        [Serializable]
        [XmlRoot("Param")]
        public struct Pair<K, V>
        {
            [XmlAttribute("Key")]
            public K Key { get; set; }

            [XmlAttribute("Value")]
            public V Value { get; set; }
        }

#if DEBUG
        [WebMethod]
        public Response GetReportTest(string dogovorCode, string reportGuid)
        {
            return GetReport(dogovorCode, new Guid(reportGuid), null, FileType.rtf);
        }
#endif

        [WebMethod]
        public Response GetReport(string dogovorCode, Guid reportGuid, Pair<string, string>[] extraParams, FileType type)
        {
            try
            {
                //найти договор
                var masterHelper = new MasterTour();

                var dogovor = masterHelper.GetDogovorByCode(dogovorCode); //ищем путевку по коду

                if (dogovor == null)
                    throw new Exception(String.Format("Dogovor '{0}' not founded", dogovorCode));


                var data = new Hashtable();

                if(extraParams!= null)
                    foreach(Pair<string,string> dataItem in extraParams)
                        data[dataItem.Key] = dataItem.Value;

                return new Response()
                {
                    hasErrors = false,

                    value = new ReportResponse()
                                 {
                                     content = new ReportsGenerator().GenerateReport(dogovor, reportGuid, data, type),
                                     type = type
                                 }
                };

            }
            catch (Exception ex)
            {
                return new Response() { 
                                            errorMessage = ex.Message + " "+ ex.Source + " " + ex.StackTrace,
                                            hasErrors = true
                                        };
            }
        }

       
        [WebMethod]
        public Response GetDogovorInfo(string dogovorCode)
        {
            try
            {
                var masterHelper = new MasterTour();

                var dogovor = masterHelper.GetDogovorByCode(dogovorCode); //ищем путевку по коду

                if (dogovor == null)
                    throw new Exception(String.Format("Dogovor '{0}' not founded", dogovorCode));

                return new Response()
                {
                    value = new DogovorInfo() {
                        agentKey = dogovor.PartnerKey,
                        agentLogin = dogovor.DupUserKey > 0 ? (new DupUsers(new DataContainer()).FindByPKeyValue(dogovor.DupUserKey) as DupUser).ID: "", 
                        dgCode = dogovor.Code,
                        dgKey = dogovor.Key,
                        paid =  Convert.ToDecimal( dogovor.Payed ),
                        price = Convert.ToDecimal( dogovor.Price ),
                        rateIsoCode = dogovor.Rate.ISOCode,
                        statusKey = dogovor.OrderStatusKey,
                        statusName = dogovor.OrderStatus.Name_Rus,
                        tourDate = dogovor.TurDate.ToString(ConfigurationManager.AppSettings["datesFormat"])
                    }
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
        /// Создает бронь в Мастер-Туре.
        /// </summary>
        /// <returns>DogovorCode -- код созданной брони</returns>
        [WebMethod]
        public Response CreateDogovor(InTourist[] turists, UserInfo userInfo, InService[] services, string dogovorCode)
        {
            try
            {
                var mtHelper = new MasterTour();

                var dogovor = mtHelper.GetDogovorByCode(dogovorCode);
                if (dogovor != null)
                {
                    services.All(item => item.Validate(dogovor.NMen));
                    return new Response()
                    {
                        value = mtHelper.ReloadDogovorServices(services, dogovor)
                    };
                }
                //verify
                turists.All(item => item.Validate());

                userInfo.Validate();

                services.All(item => item.Validate(turists.Length));

                //создание путевки
                return new Response()
                {
                    value = mtHelper.CreateNewDogovor(turists, userInfo, services, dogovorCode)
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

                using(var masterFinanceHelper = new MasterFinance())
                {

                    var dogovor = masterHelper.GetDogovorByCode(dogovorCode); //ищем путевку по коду

                    if (dogovor == null)
                        throw new Exception(String.Format("Dogovor '{0}' not founded", dogovorCode));


                    if ((paidSum <= 0) && (paymentSys != "deposit"))
                      throw new Exception(String.Format("paidSum <= 0"));


                    if ((paymentSys == "deposit") && (dogovor.PartnerKey > 0))  // если заказ агентский и тип оплаты == депозит
                        masterFinanceHelper.DepositPayDogovor(dogovor.PartnerKey, dogovor.Key); //платим депозитом
                    else //иначе
                        masterFinanceHelper.PayDogovor(dogovor.Code,
                                                        paymentType,
                                                        paymentSys,
                                                        paidSum,
                                                        paymentId); // сохраняем платеж по карте
                 }

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
                try{ 
                    using(var masterFinance = new DataBase.MasterFinance())
                    {
                        return new Response(){ 
                         value = masterFinance.CallDepositGetValue(partnerId)
                        };
                    }
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

        /// <summary>
        /// Аннуляция договора
        /// </summary>
        /// <param name="dogovorCode">код договора</param>
        /// <param name="reason">код причины аннуляции</param>
        /// <param name="penalty">сумма штрафа в валюте договора</param>
        /// <returns></returns>
        [WebMethod]
        public Response AnnulateDogovor(string dogovorCode, short reason, decimal penalty)
        {
            try
            {
                var masterHelper = new MasterTour();

                var dogovor = masterHelper.GetDogovorByCode(dogovorCode); //ищем путевку по коду

                if (dogovor == null)
                    throw new Exception(String.Format("Dogovor '{0}' not founded", dogovorCode));

                dogovor.Annulate(reason, Convert.ToDouble(penalty));
                dogovor.DataContainer.Update();

                if(dogovor.IsAnnulated)
                    return new Response()
                    {
                        value = "success"
                    };
                else
                    return new Response()
                    {
                        value = "failed"
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


        [WebMethod]
        public Response CreatePartner(PartnerInfo partnerInfo)
        {
            if (partnerInfo.Validate())
            {
                var masterHelper = new MasterTour();

                return new Response()
                {
                    value = masterHelper.AddPartner(partnerInfo)
                };
            }
            else
                return new Response()
                {
                    hasErrors = true,
                    errorMessage = "Invalid Partner Info"
                };
        }
    }
}
