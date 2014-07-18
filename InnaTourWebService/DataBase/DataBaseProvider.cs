using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using System.Configuration;

using Microsoft.VisualStudio.QualityTools.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InnaTourWebService.Helpers;

namespace InnaTourWebService.DataBase
{
    public class DataBaseProvider: IDisposable
    {
        #region Fields
        //подключение к базе
        SqlConnection myConnection = null;
        #endregion

        public void Dispose()
        {
            if ((this.myConnection.State != System.Data.ConnectionState.Broken) && (this.myConnection.State != System.Data.ConnectionState.Broken))
                this.myConnection.Close();
        }
        
        #region public methods
        public DataBaseProvider() //конструктор
        {
            //создаем подключение к базе
            this.myConnection = new SqlConnection(ConfigurationManager.AppSettings["connectionString"]);

            this.OpenConnection();
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
            this.OpenConnection(); //открываем соединение

            var com = new SqlCommand(procName, myConnection); //создаем команду вызова хранимки

            com.CommandType = CommandType.StoredProcedure;

            foreach (string key in procParams.Keys) //добавляем параметры
                com.Parameters.Add(new SqlParameter("@" + key, procParams[key]));
        
            com.CommandTimeout = 3000;

            var dataSet = new DataSet(); //создаем дата-сет для ответа
            var adapter = new SqlDataAdapter(com);

            adapter.Fill(dataSet); //получаем данные

            var outputValues = new Dictionary<string, object>();

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
            string comString = string.Format("insert into {0}({1}) values(@{2})", tableName, string.Join(", ", values.Keys),  string.Join(",@", values.Keys));

            var cmd = new SqlCommand(comString, this.myConnection);

            //add params


            cmd.Parameters.Add("@ID", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            return 0;
        }

        #endregion

        #region private methods
        private void OpenConnection()
        {
            try
            {
                if ((this.myConnection.State == System.Data.ConnectionState.Closed) || (this.myConnection.State == System.Data.ConnectionState.Broken))
                    this.myConnection.Open();
            }
            catch (Exception ex)
            {
                Logger.ReportException(ex); // записывем эксепшн в лог

                throw ex;// бросаем дальше
            }
        }
        #endregion

         public static string SafeSqlLiteral(string inputSQL)
         {
            return inputSQL.Replace("'", "''");
         }
    }
}