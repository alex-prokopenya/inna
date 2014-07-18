﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InnaTourWebService.Models;

using System.Data;

namespace InnaTourWebService.DataBase
{
    public class MasterFinance
    {
        private DataBaseProvider db = new DataBaseProvider();


        /// <summary>
        /// запрашиваем размер депозита по id агента
        /// </summary>
        /// <param name="partnerId">id партнера из таблицы tbl_Partners</param>
        /// <returns>DepositInfo -- сумма размера депозита и код валюты</returns>
        public DepositInfo[] CallDepositGetValue(int partnerId)
        {
            var inpParams = new Dictionary<string, object>(); //составляем список параметров

            inpParams.Add("p_nPartner", partnerId);
            inpParams.Add("p_nErrorCode", "");
            inpParams.Add("p_sErrorString", "");

            var result = this.db.CallStoredProcedure("FIN_DepositGetValue", inpParams, new string[] { "p_nErrorCode", "p_sErrorString"});

            if (result.ContainsKey("dataSet"))
            {
                var resp = new List<DepositInfo>();

                var rows = result["dataSet"] as DataSet;

                if((rows.Tables.Count>0) && (rows.Tables[0].Columns.Contains("DEP_VALUE")))
                {
                    foreach (DataRow row in rows.Tables[0].Rows)
                        resp.Add(new DepositInfo() { 
                                      Deposit =  Convert.ToDecimal(row["DEP_VALUE"]), 
                                      RateIsoCode = row["DEP_RAISOCODE"].ToString()});

                    return resp.ToArray();
                }
            }
            
            if (result.ContainsKey("output"))
            {
                var output = result["output"] as Dictionary<string, object>;

                throw new Exception(String.Format( "Check deposit exception. Recieved code {0}. Error text '{}'.", output["p_nErrorCode"], output["p_sErrorString"] ) );
            }

            throw new Exception("Incorrect FIN_DepositGetValue result");
        }

        /// <summary>
        /// создает проводку для агентской заявки, используя депозит
        /// </summary>
        /// <param name="partnerId">id партнера из таблицы tbl_Partners</param>
        /// <param name="dgKey">id путевки -- dg_key из tbl_dogovors</param>
        public void DepositPayDogovor(int partnerId, int dgKey)
        {
            var inpParams = new Dictionary<string, object>(); //составляем список параметров

            inpParams.Add("p_nPartner", partnerId);
            inpParams.Add("p_nDGKey", dgKey);
            inpParams.Add("p_nErrorCode", "");
            inpParams.Add("p_sErrorString", "");

            var result = this.db.CallStoredProcedure("FIN_DepositPayDogovor", inpParams, new string[] { "p_nErrorCode", "p_sErrorString" });


            if (result.ContainsKey("output"))
            {
                var output = result["output"] as Dictionary<string, object>;

                if (Convert.ToInt32( output["p_nErrorCode"] ) > 0)
                    throw new Exception(String.Format("Pay deposit exception. Recieved code {0}. Error text '{}'.", output["p_nErrorCode"], output["p_sErrorString"]));
            }
        }

        public void PayDogovor(string dgCode, int paymentType, string paymentSys, decimal paidSum, string paymentId)
        {
            var db = new DataBaseProvider();

            var inpParams = new Dictionary<string, object>(); 
            inpParams.Add("DP_TXN_ID", paymentId);
            inpParams.Add("DP_TXN_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            inpParams.Add("DP_DGCOD", dgCode);
            inpParams.Add("DP_SUM", paidSum);
            inpParams.Add("DP_PAYTYPE", 0);
            inpParams.Add("DP_RESULT", 0);
            inpParams.Add("DP_PAYMENTSYS", paymentSys);

            db.InsertLineToTable("FIN_DOGOVOR_PAID", inpParams);
        }
    }
}