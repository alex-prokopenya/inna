using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using System.Configuration;

using InnaTourWebService.Helpers;

namespace InnaTourWebService.DataBase
{
    internal class DataBaseProvider: IDisposable
    {
        #region Fields
        //подключение к базе
        SqlConnection myConnection = null;
        #endregion

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
            {
                this.myConnection.Dispose();

                disposed = true;
            }
        }
        
        #region public methods
        public DataBaseProvider() //конструктор
        {
            //создаем подключение к базе
            this.myConnection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]);

            this.OpenSqlConnection();
        }


        /// <summary>
        /// выбираем данные из view [FIN_DepositAndReceivable]
        /// </summary>
        /// <param name="partnerKey"></param>
        /// <returns></returns>
        public DataTable SelectDeposits(int partnerKey )
        {
            OpenSqlConnection();

            string comString = string.Format("select [DAL_PRKEY],[DAL_CompanyKey],[DAL_DepositSum] ,[DAL_DepositRate],[DAL_LimitSum],[DAL_LimitRate], " +
                                             "[DAL_UpdateDate], b.RA_ISOCode as dep_rate_iso, a.RA_ISOCode as lim_rate_iso " +
                                             "from [dbo].[FIN_DepositAndReceivable], dbo.Rates as a, dbo.Rates as b "+
                                             "where a.RA_CODE = DAL_LimitRate and b.RA_CODE = DAL_DepositRate and DAL_PRKEY = " + partnerKey);

            var cmd = new SqlCommand(comString, this.myConnection);

            var dataSet = new DataSet();
            var sqlAdaptert = new SqlDataAdapter(cmd).Fill(dataSet);

            if (dataSet.Tables.Count > 0)
                return dataSet.Tables[0];
            else
                return null;
        }

        /// <summary>
        /// вызывает хранимую процедуру
        /// </summary>
        /// <param name="procName">имя процедуры</param>
        /// <param name="procParams">параметры, передаваемые в процедуру</param>
        /// <param name="outputParams">исходящие параметры</param>
        /// <returns>возвращает Dictionary c ключами dataSet и output</returns>
        public Dictionary<string, object> CallStoredProcedure(string procName, Dictionary<string, object> procParams, string[] outputParams)
        {
            this.OpenSqlConnection(); //открываем соединение

            var com = new SqlCommand(procName, myConnection); //создаем команду вызова хранимки

            com.CommandType = CommandType.StoredProcedure;

            foreach (string key in procParams.Keys) //добавляем параметры
                com.Parameters.Add(new SqlParameter("@" + key, procParams[key]) 
                {
                    Direction = outputParams.Contains(key)? ParameterDirection.Output: ParameterDirection.Input,
                    Size = 200 //размер получаемого ответа
                });

            com.CommandTimeout = 3000;

            var dataSet = new DataSet(); //создаем дата-сет для ответа
            var adapter = new SqlDataAdapter(com);

            adapter.Fill(dataSet); //получаем данные

            var outputValues = new Dictionary<string, object>();

#if DEBUG
            foreach (string key in procParams.Keys) //
               Logger.WriteToLog ("for key "+key+" value " + com.Parameters["@" + key].Value);
#endif

            foreach(string key in outputParams) //
                outputValues.Add(key, com.Parameters["@"+key].Value);

            var result = new Dictionary<string, object>();

            result.Add("dataSet", dataSet);         //пакуем ответ
            result.Add("output", outputValues);     

            return result;
        }

        /// <summary>
        /// Добавляет в запись в таблицу
        /// </summary>
        /// <param name="tableName">имя таблицы</param>
        /// <param name="values">набор присваиваемых знацений</param>
        /// <returns>id новой строки</returns>
        public int InsertLineToTable(string tableName, Dictionary<string, object> values)
        {
            OpenSqlConnection();

            string comString = string.Format("insert into {0}({1}) values(@{2}) SET @ID = SCOPE_IDENTITY();", tableName.ToString(), string.Join(", ", values.Keys),  string.Join(",@", values.Keys)); //"рыба" для запроса на добавление записси

            comString = SafeSqlLiteral(comString); //экранируем одинарные кавычки

           //Добавить вывод id

            var cmd = new SqlCommand(comString, this.myConnection); //создем объект SqlCommand

            //add params
            foreach (string key in values.Keys) //добавляем параметры
                cmd.Parameters.Add(new SqlParameter(key, values[key]));

            cmd.Parameters.Add("@ID", SqlDbType.Int, 4).Direction = ParameterDirection.Output; //

            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["@ID"].Value);
        }

        #endregion

        #region private methods
        private void OpenSqlConnection()
        {
            try
            {
                if ((this.myConnection.State == System.Data.ConnectionState.Closed) || (this.myConnection.State == System.Data.ConnectionState.Broken))
                    this.myConnection.Open();
            }
            catch (Exception ex)
            {
                Logger.ReportException(ex); // записывем эксепшн в лог

                throw;// бросаем дальше
            }
        }
        #endregion

        /// <summary>
        /// экранирует одинарные кавычки -- защита от инъекции
        /// </summary>
        /// <param name="inputSQL">sql - запрос</param>
        /// <returns></returns>
         public static string SafeSqlLiteral(string inputSQL)
         {
            return inputSQL.Replace("'", "''");
         }
    }
}