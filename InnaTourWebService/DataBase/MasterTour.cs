using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using Megatec.MasterTour.DataAccess;
using Megatec.MasterTour.BusinessRules;
using Megatec.Common.DataAccess;
using InnaTourWebService.Models;
using System.Globalization;

namespace InnaTourWebService.DataBase
{
    public class MasterTour
    {
        private string dateFormat;

        private int aviaServiceKey;

        private int hotelServiceKey;

        /// <summary>
        /// создает объект MasterTour
        /// </summary>
        public MasterTour()
        {
            Megatec.MasterTour.DataAccess.Manager.ConnectionString = ConfigurationManager.AppSettings["connectionString"];
            dateFormat = ConfigurationManager.AppSettings["datesFormat"];

            aviaServiceKey = Convert.ToInt32(ConfigurationManager.AppSettings["AviaBookingServiceKey"]);
            hotelServiceKey = Convert.ToInt32(ConfigurationManager.AppSettings["HotelBookingServiceKey"]);
        }

        /// <summary>
        /// Ищет путевку по ее коду
        /// </summary>
        /// <param name="dogovorCode">код путевки</param>
        /// <returns>объект Dogovor</returns>
        public Dogovor GetDogovorByCode(string dogovorCode)
        {
            if (dogovorCode.Length > 10)
                dogovorCode = dogovorCode.Substring(0, 10);

            var dogs = new Dogovors(new Megatec.Common.BusinessRules.Base.DataContainer());

            dogs.RowFilter = string.Format("dg_code='{0}'", DataBaseProvider.SafeSqlLiteral(dogovorCode));

            dogs.Fill();

            if (dogs.Count > 0)
                return dogs[0];
            else
                return null;
        }

        /// <summary>
        /// получает информацию об агенте по его ID
        /// </summary>
        /// <param name="partnerKey">pr_key партнера</param>
        /// <returns>информация об агенте</returns>
        public UserInfo GetPartnerInfo(int partnerKey)
        {
            var prs = new Partners(new Megatec.Common.BusinessRules.Base.DataContainer());

            var pr = prs.FindByPKeyValue(partnerKey) as Partner;

            if (pr == null) throw new Exception("Partner not founded for pr_key = " + partnerKey);

            return new UserInfo() { 
                Name = pr.Name
            };
        }

        /// <summary>
        /// получает информацию об агенте по его логину
        /// </summary>
        /// <param name="agentLogin">логин агента</param>
        /// <returns>информация об агенте</returns>
        public int GetAgentInfo(string agentLogin)
        {
            var dupUsers = new DupUsers(new Megatec.Common.BusinessRules.Base.DataContainer());

            dupUsers.RowFilter = "us_id='" + DataBase.DataBaseProvider.SafeSqlLiteral(agentLogin) + "'";

            dupUsers.Fill();

            if (dupUsers.Count == 0) throw new Exception("Partner not founded for login = " + agentLogin);

            return dupUsers[0].PartnerKey;
        }



        public TurList GetTurlistByKey(int tlKey)
        {
            var turlists = new TurLists(new Megatec.Common.BusinessRules.Base.DataContainer());

            var tl = turlists.FindByPKeyValue(tlKey) as TurList;

            if (tl == null)
                Helpers.Logger.WriteToLog("tour not founded for key " + tlKey);

            return tl;
        }


        public DupUser GetDupUserByLogin(string agentLogin)
        {
            var dupUsers = new DupUsers(new Megatec.Common.BusinessRules.Base.DataContainer());

            dupUsers.RowFilter = "us_id='" + DataBase.DataBaseProvider.SafeSqlLiteral(agentLogin) + "'";

            dupUsers.Fill();

            if (dupUsers.Count == 0) throw new Exception("Partner not founded for login = " + agentLogin);

            return dupUsers[0];
        }


        /// <summary>
        /// создает бронь в Мастер-Туре
        /// </summary>
        /// <param name="tourists">список туристов</param>
        /// <param name="userInfo">информация о покупателе</param>
        /// <param name="services">список услуг</param>
        /// <returns>номер брони</returns>
        public string CreateNewDogovor(InTourist[] tourists, UserInfo userInfo, InService[] services, string dogovorCode)
        {
            Array.Sort<InService>(services);

            //создали пустой договор
            Dogovor dogovor = CreateEmptyDogovor(userInfo, 
                                                 DateTime.ParseExact(services[0].Date, 
                                                                     dateFormat,
                                                                     null),
                                                 dogovorCode);

            dogovor.NMen = (short)tourists.Length;
            dogovor.DataContainer.Update();

            //добавляем туристов
            foreach (var tourist in tourists)
                AddTouristToDogovor(dogovor, tourist);

            //добавляем услуги
            foreach (var service in services)
                AddServiceToDogovor(dogovor, service);

            //MyCalculateTotalCost
            MyCalculateCost(dogovor);

            dogovor.Turists.Fill();
           
            dogovor.DataContainer.Update();

            return dogovor.Code;
        }


        #region private methods

        private void AddTouristToDogovor(Dogovor dogovor, InTourist tourist)
        {
            Turist tst = dogovor.Turists.NewRow();    // создаем новый объект "турист"
            tst.NameRus = tourist.LastName;          // проставляем имя
            tst.NameLat = tst.NameRus;

            tst.FNameRus = tourist.FirstName;           // проставляем фамилию
            tst.FNameLat = tst.FNameLat;

            tst.Birthday = DateTime.ParseExact(tourist.BirthDate, dateFormat, null ); //дату рождения
            tst.CreatorKey = dogovor.CreatorKey;       //создатель

            if(tourist.PassrortType == PasportType.FOREIGN)
            {
                if (tourist.PassportValidDate != "")
                    tst.PasportDateEnd = DateTime.ParseExact(tourist.PassportValidDate, dateFormat, null);

                tst.PasportNum = tourist.PasspordNumber;      //номер и ...
                tst.PasportType = tourist.PasspordCode;       //... серия паспорта
            }
            else // внутренний паспорт
            {
                tst.PaspRuNum = tourist.PasspordNumber;      //номер и ...
                tst.PaspRUser = tourist.PasspordCode;       //... серия паспорта
            }

            tst.DogovorCode = dogovor.Code;              //код путевки
            tst.DogovorKey = dogovor.Key;                //ключ путевки

            tst.Citizen = tourist.Citizenship;          //код гражданства туриста

            if (tourist.Sex == Sex.F)                  //пол туриста
            {
                tst.RealSex = Turist.RealSex_Female;
                if (tst.Age > Convert.ToInt32(ConfigurationManager.AppSettings["ChildAgeLimit"]))//ребенок или взрослый в зависимости от возраста
                    tst.Sex = Turist.Sex_Female;
                else if(tst.Age < 2)
                    tst.Sex = Turist.Sex_Infant;
                else
                    tst.Sex = Turist.Sex_Child;
            }
            else
            {
                tst.RealSex = Turist.RealSex_Male;
                if (tst.Age > Convert.ToInt32(ConfigurationManager.AppSettings["ChildAgeLimit"]))//ребенок или взрослый в зависимости от возраста
                    tst.Sex = Turist.Sex_Male;
                else if (tst.Age < 2)
                    tst.Sex = Turist.Sex_Infant;
                else
                    tst.Sex = Turist.Sex_Child;
            }

            dogovor.Turists.Add(tst);                              //Добавляем к туристам в путевке 
            dogovor.Turists.DataContainer.Update();                //Сохраняем изменения
        }

        private void AddServiceToDogovor(Dogovor dogovor, InService service)
        {
            dogovor.Turists.Sort = "tu_key asc";

            dogovor.Turists.Fill();

            var dateStart = DateTime.ParseExact( service.Date, this.dateFormat, null );

            var dateEnd = dateStart.AddDays(service.NDays - 1);

            DogovorList dl = dogovor.DogovorLists.NewRow();
           
            dl.TourKey = dl.Dogovor.TourKey;
            dl.PacketKey = dl.Dogovor.TourKey;
            dl.CreatorKey = dl.Dogovor.CreatorKey;        //копируем ключ создателя путевки
            dl.OwnerKey = dl.Dogovor.OwnerKey;            //копируем ключ создателя путевки
            dl.DateBegin = dateStart;
            dl.DateEnd = dateEnd;

            dl.NDays = (short)(service.NDays);
           

            if (dl.Dogovor.TurDate > dl.DateBegin)      //корректируем даты тура в путевке
            {
                dl.Dogovor.NDays += (short)(dl.Dogovor.TurDate - dl.DateBegin).Days;
                dl.Dogovor.TurDate = dl.DateBegin;
                dl.Dogovor.DataContainer.Update();
            }

            if (dl.DateEnd > dl.Dogovor.DateEnd)        //корректируем дату окончания тура в путевке
            {
                dl.Dogovor.NDays += (short)(dl.DateEnd - dl.Dogovor.DateEnd).Days;
                dl.Dogovor.DataContainer.Update();
            }

            dl.TurDate = dl.Dogovor.TurDate;
            dl.Day = (short)((dl.DateBegin - dl.TurDate).Days + 1);

            dl.NMen = (short)(service.TuristIndexes.Length > 0 ? service.TuristIndexes.Length : dogovor.Turists.Count);             
            dl.ServiceKey = service.ServiceType == ServiceType.AVIA ? aviaServiceKey: hotelServiceKey;
            dl.CityKey = dogovor.CityKey;

            //Добавляем услугу
            dl.SubService = AddServiceList(dl.ServiceKey, service.Title);

            dl.SubCode1 = 0;
            dl.SubCode2 = 0;

            //разбираемся с ценами

            dl.Brutto   = service.Price;
            dl.Netto    = service.NettoPrice;
            dl.Discount = service.Comission;
            
            dl.PartnerKey = service.PartnerID;               //проставляем поставщика услуги
            dl.Comment = service.PartnerBookID;
            dl.BuildName();
            dogovor.DogovorLists.Add(dl);

            dogovor.DogovorLists.DataContainer.Update();

            dl.FormulaNetto = dl.Netto.ToString("0.00").Replace(".", ",");
            dl.FormulaBrutto = dl.Brutto.ToString("0.00").Replace(".", ",");
            dl.FormulaDiscount = dl.Discount.ToString("0.00").Replace(".", ",");

            dl.DataContainer.Update();

            //садим людей на услугу
            this.AddTouristsToService(dogovor, service, dl);

            dogovor.DogovorLists.DataContainer.Update();        //сохраняем изменения в услугах
        }

        /// <summary>
        /// создает привязки туристов к услуге
        /// </summary>
        /// <param name="dogovor">путевка</param>
        /// <param name="service">полученная сервисом услуга</param>
        /// <param name="dl">объект DogovorList -- добавленная в МТ услуга</param>
        private void AddTouristsToService(Dogovor dogovor, InService service, DogovorList dl)
        {
            //садим людей на услугу
            TuristServices tServices = new TuristServices(new Megatec.Common.BusinessRules.Base.DataContainer());
            for (int index = 0; index < dogovor.Turists.Count; index++)       //Просматриваем услуги в путевке
            {
                if ((service.TuristIndexes.Length == 0) || (service.TuristIndexes.Contains(index + 1)))
                {
                    var docNumIndex = index; 

                    if(service.TuristIndexes.Contains(index + 1))
                        docNumIndex = Array.IndexOf( service.TuristIndexes, (index + 1));

                    Turist tst = dogovor.Turists[index];

                    TuristService ts = tServices.NewRow();  //садим его на услугу
                    ts.Turist = tst;
                    ts.DogovorList = dl;

                    
                    ts.Numdoc = service.NumDocs.Length > docNumIndex ? service.NumDocs[docNumIndex]: "";//

                    tServices.Add(ts);
                    tServices.DataContainer.Update();           //сохраняем изменения
                }
            }
        }

        private ServiceList AddServiceList(int serviceKey, string name)
        {
            if (name.Length > 50) name = name.Substring(0, 50);

            ServiceLists sls = new ServiceLists(new Megatec.Common.BusinessRules.Base.DataContainer());
            sls.RowFilter = "sl_name = '" + name + "' and sl_svkey=" + serviceKey;
            sls.Fill();

            if (sls.Count > 0) return sls[0];

            ServiceList sl = sls.NewRow();

            sl.Name = name;
            sl.ServiceKey = serviceKey;

            sls.Add(sl);
            sls.DataContainer.Update();

            return sl;
        }

        /// <summary>
        /// создаем пустую путевку на выбранного пользователя, пакет, дату
        /// </summary>
        /// <param name="userInfo">информация о покупателе</param>
        /// <returns>ссылка на созданную путевку</returns>
        private Dogovor CreateEmptyDogovor(UserInfo userInfo, DateTime startDate, string dogovorCode)
        {
            Dogovors dogs = new Dogovors(new Megatec.Common.BusinessRules.Base.DataContainer());
            Dogovor dog = dogs.NewRow();

            if ((dogovorCode != null) && (dogovorCode.Length > 0))
            {
                if (GetDogovorByCode(dogovorCode) != null)
                    throw new Exception(String.Format("dogovor {0} already exists", dogovorCode));
                else
                    dog.Code = dogovorCode;
            }

            //берем информацию о туре, к которму цепляем
            var turList = this.GetTurlistByKey(Convert.ToInt32(ConfigurationManager.AppSettings["BookingPacketKey"]));
            dog.CountryKey = turList.CountryKey; //привязываем к стране 
            dog.CityKey = turList.CTDepartureKey; // ... городу
            dog.TourKey = turList.Key;  // ... пакету

            //даты и продолжительность
            dog.TurDate = startDate; //дата тура -- ставится по первой услуге, далее может быть изменена
            dog.NDays = 1;           //продолжительность. млжет измениться в процессе добавления услуг
         
            //информация о покупателе
            dog.MainMenEMail = userInfo.Email != "" ? userInfo.Email : "";
            dog.MainMenPhone = userInfo.Phone != "" ? userInfo.Phone : "";
            dog.MainMenComment = userInfo.Phone != "" ? userInfo.Phone : "";
            dog.MainMen = userInfo.Name != "" ? userInfo.Name : "";//;
           
            //получаем текущего пользователя - покупателя
            dog.PartnerKey = userInfo.AgentKey;

            dog.AdvertisementKey = userInfo.AgentKey > 0 ? 21 : 5;

            //получаем пользоваетля в из строки подключения к базе
            var users = new Users(new Megatec.Common.BusinessRules.Base.DataContainer());
            users.Fill();

            dog.CreatorKey = users.CurrentUser.Key;
            dog.OwnerKey = users.CurrentUser.Key;

            //получаем код национальной валюты
            string rateCode = this.GetNationalRateCode();
            dog.RateCode = rateCode;

            dogs.Add(dog); //добавляем запись в БД

            dogs.DataContainer.Update(); // !! проверить, работет ли без этого!

            return dog;
        }

        /// <summary>
        /// ищет код национальной валюты
        /// </summary>
        /// <returns>код нац. валюты</returns>
        private string GetNationalRateCode()
        {
            var rates = new Rates(new Megatec.Common.BusinessRules.Base.DataContainer());
            rates.RowFilter = "RA_National = 1";
            rates.Fill();

            string rateCode = "рб";

            if (rates.Count > 0) rateCode = rates[0].Code;
            return rateCode;
        }


        private void MyCalculateCost(Dogovor dog)                             //Расчитываем стоимость
        {
            dog.DogovorLists.Fill();
            foreach (DogovorList dl in dog.DogovorLists)                      //По всем услугам в путевке
            {
                try
                {
                    if ((dl.FormulaBrutto != "") && (dl.FormulaBrutto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        dl.Brutto = Convert.ToDouble(dl.FormulaBrutto);    //проставляем брутто из поля "Formula"

                        dl.FormulaBrutto = dl.Brutto.ToString("0.00").Replace(".", ",");
                        dog.Price += dl.Brutto;
                    }

                    if ((dl.FormulaNetto != "") && (dl.FormulaNetto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        dl.Netto = System.Convert.ToDouble(dl.FormulaNetto);      //проставляем брутто из поля "Formula"
                        dl.FormulaNetto = dl.Netto.ToString("0.00").Replace(".", ",");
                    }

                    if ((dl.FormulaDiscount != "") && (dl.FormulaDiscount.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        dl.Discount = System.Convert.ToDouble(dl.FormulaDiscount);      //проставляем брутто из поля "Formula"
                        dog.DiscountSum += dl.Discount;
                        dl.FormulaDiscount = dl.Discount.ToString("0.00").Replace(".", ",");
                    }

                    dl.DataContainer.Update();   
                    dog.DataContainer.Update();
                }
                catch (Exception ex)
                {
                    Helpers.Logger.WriteToLog("in myCalc " + ex.Message + " " + ex.StackTrace + " " + dl.Name );
                }
            }
          
        }
        #endregion
    }
}