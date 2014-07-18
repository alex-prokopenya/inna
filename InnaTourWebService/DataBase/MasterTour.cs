using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using Megatec.MasterTour.DataAccess;
using Megatec.MasterTour.BusinessRules;
using Megatec.Common.DataAccess;

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
            var dogs = new Dogovors(new DataCache());

            dogs.RowFilter = string.Format("dg_code='{0}'", DataBaseProvider.SafeSqlLiteral(dogovorCode));

            dogs.Fill();

            if (dogs.Count > 0)
                return dogs[0];
            else
                return null;
        }


    }
}