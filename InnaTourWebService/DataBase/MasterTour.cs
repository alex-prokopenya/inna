using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using Megatec.MasterTour.DataAccess;
using Megatec.MasterTour.BusinessRules;
using Megatec.Common.DataAccess;
using InnaTourWebService.Models;

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
        /// <param name="partnerKey"></param>
        /// <returns>информация об агенте</returns>
        public UserInfo GetUserInfoForAgent(int partnerKey)
        {
            var prs = new Partners(new Megatec.Common.BusinessRules.Base.DataContainer());

            var pr = prs.FindByPKeyValue(partnerKey) as Partner;

            if (pr == null) throw new Exception("Partner not founded for pr_key = " + partnerKey);

            return new UserInfo() { 
                AgentKey = partnerKey,
                Email = pr.Email,
                Name = pr.Name,
                Phone = pr.Phones
            };
        }


        public string CreateNewDogovor(InTourist[] turists, UserInfo userInfo, InService[] services)
        {
            return "test";
        }
    }
}