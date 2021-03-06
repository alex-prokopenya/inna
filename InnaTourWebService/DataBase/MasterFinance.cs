﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InnaTourWebService.Models;

using System.Data;

namespace InnaTourWebService.DataBase
{
    public class MasterFinance:IDisposable
    {
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                this.db.Dispose();

            disposed = true;
        }

        private DataBaseProvider db = new DataBaseProvider();
        /// <summary>
        /// запрашиваем размер депозита по id агента
        /// </summary>
        /// <param name="partnerId">id партнера из таблицы tbl_Partners</param>
        /// <returns>DepositInfo -- сумма размера депозита и код валюты</returns>
        public DepositInfo[] CallDepositGetValue(int partnerId)
        {
            var result = this.db.SelectDeposits(partnerId);

            var deposits = new Dictionary<string, DepositInfo>();
            deposits.Add("RUR", new DepositInfo("RUR"));

            if(result != null)
                foreach (DataRow row in result.Rows)
                {
                    var depRate = row["dep_rate_iso"].ToString();
                    var depValue = Convert.ToDecimal( row["DAL_DepositSum"] );

                    if (!deposits.ContainsKey(depRate))
                        deposits[depRate] = new DepositInfo(depRate);

                    deposits[depRate].Deposit += depValue;

                    var limRate = row["lim_rate_iso"].ToString();
                    var limValue = Convert.ToDecimal(row["DAL_LimitSum"]);

                    if (!deposits.ContainsKey(limRate))
                        deposits[limRate] = new DepositInfo(limRate);

                    deposits[limRate].Limit += limValue;
                }

            return deposits.Values.ToArray();
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
                    throw new Exception(String.Format("Pay deposit exception. Recieved code {0}. Error text '{1}'.", output["p_nErrorCode"], output["p_sErrorString"]));
            }
        }

        /// <summary>
        /// создает проводку к путевке
        /// </summary>
        /// <param name="dgCode">код путевки</param>
        /// <param name="paymentType">код способа оплаты: 0 - оплата по карте</param>
        /// <param name="paymentSys">название платежной системы</param>
        /// <param name="paidSum">оплаченная сумма</param>
        /// <param name="paymentId">идентификатор платежа</param>
        public void PayDogovor(string dgCode, int paymentType, string paymentSys, decimal paidSum, string paymentId)
        {
            var inpParams = new Dictionary<string, object>();  //собираем параметры для сохранения новой строки в таблице оплат
            inpParams.Add("DP_TXN_ID", paymentId);
            inpParams.Add("DP_TXN_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            inpParams.Add("DP_DGCOD", dgCode);
            inpParams.Add("DP_SUM", paidSum);
            inpParams.Add("DP_PAYTYPE", 0);
            inpParams.Add("DP_RESULT", 0);
            inpParams.Add("DP_PAYMENTSYS", paymentSys);

            db.InsertLineToTable("FIN_DOGOVOR_PAID", inpParams); //сохраняем запись в БД
        }
    }
}