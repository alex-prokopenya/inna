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
using System.Configuration;

namespace InnaTourWebService.DataBase
{
    public class MasterTour
    {
        /// <summary>
        /// создает объект MasterTour
        /// </summary>
        public MasterTour()
        {
            Megatec.MasterTour.DataAccess.Manager.ConnectionString = ConfigurationManager.AppSettings["connectionString"];
        }

        /// <summary>
        /// Ищет путевку по ее коду
        /// </summary>
        /// <param name="dogovorCode">код путевки</param>
        /// <returns>объект Dogovor</returns>
        public Dogovor GetDogovorByCode(string dogovorCode)
        {
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
        public string CreateNewDogovor(InTourist[] tourists, UserInfo userInfo, InService[] services)
        {
            //создали пустой договор
            Dogovor dogovor = CreateEmptyDogovor(userInfo, 
                                                 DateTime.ParseExact(services[0].Date, 
                                                                     ConfigurationManager.AppSettings["DatesFormat"],
                                                                     null));

            foreach (var tourist in tourists)
                AddTouristToDogovor(dogovor, tourist);

            foreach (var service in services)
                AddServiceToDogovor(dogovor, service);

            //MyCalculateTotalCost

            return dogovor.Code;
        }


        #region private methods

        private void AddTouristToDogovor(Dogovor dogovor, InTourist tourist)
        { 
        
        }

        private void AddServiceToDogovor(Dogovor dogovor, InService service)
        { 
        
        }

        /// <summary>
        /// создаем пустую путевку на выбранного пользователя, пакет, дату
        /// </summary>
        /// <param name="userInfo">информация о покупателе</param>
        /// <returns>ссылка на созданную путевку</returns>
        private Dogovor CreateEmptyDogovor(UserInfo userInfo, DateTime startDate)
        {
            Dogovors dogs = new Dogovors(new Megatec.Common.BusinessRules.Base.DataContainer());
            Dogovor dog = dogs.NewRow();

            //берем информацию о туре, к которму цепляем
            var turList = this.GetTurlistByKey(Convert.ToInt32(ConfigurationManager.AppSettings["BookingPacketKey"]));
            dog.CountryKey = turList.CountryKey;
            dog.CityKey = turList.CityKey;
            dog.TourKey = turList.Key;

            dog.TurDate = startDate;
            dog.NDays = 1;
            dog.MainMenEMail = userInfo.Email != "" ? userInfo.Email : "";
            dog.MainMenPhone = userInfo.Phone != "" ? userInfo.Phone : "";
            dog.MainMen = userInfo.Name !="" ? userInfo.Name : "";//;
           
            //получаем текущего пользователя - покупателя
            var dupUser = userInfo.AgentLogin != "" ? this.GetDupUserByLogin(userInfo.AgentLogin) : null;
            dog.PartnerKey = dupUser != null ? dupUser.PartnerKey: 0;
            dog.DupUserKey = dupUser != null ? dupUser.Key : 0;

            //получаем пользоваетля в из строки подключения к базе
            var users = new Users(new Megatec.Common.BusinessRules.Base.DataContainer());
            users.Fill();
            dog.CreatorKey = users.CurrentUser.Key;
            dog.OwnerKey = users.CurrentUser.Key;

            //получаем код национальной валюты
            string rateCode = this.GetNationalRateCode();
            dog.RateCode = rateCode;

            dogs.Add(dog); //добавляем запись в БД

            dogs.DataCache.Update(); // !! проверить, работет ли без этого!

            return dog;
        }

        private string GetNationalRateCode()
        {
            var rates = new Rates(new Megatec.Common.BusinessRules.Base.DataContainer());
            rates.RowFilter = "RA_National = 1";
            rates.Fill();

            string rateCode = "рб";

            if (rates.Count > 0) rateCode = rates[0].Code;
            return rateCode;
        }

        #endregion
    }
}