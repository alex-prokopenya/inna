using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Threading;

namespace InnaTourWebService.Helpers
{
    public class Logger
    {
        /// <summary>
        /// пишет в лог детали эксепшена
        /// </summary>
        /// <param name="ex">объект Exception</param>
        public static void ReportException(Exception ex)
        {
            WriteToLog(ex.Message + "\n" + ex.StackTrace);
        }

        /// <summary>
        /// пишет сообщение в лог-файл
        /// </summary>
        /// <param name="message"> текст сообщения</param>
        public static void WriteToLog(string message)
        {
            int ats = 5;
            while (ats-- > 0) //делаем несколько попыток записи
            {
                try
                {
                    StreamWriter outfile = new StreamWriter("" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + DateTime.Today.ToString("yyyy-MM-dd") + ".log", true);
                    
                    outfile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
                    outfile.WriteLine();
                    outfile.WriteLine();

                    outfile.Close();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }     
        }

        public static void WriteToBookLog(string message)
        {
            int ats = 5; 
            while (ats-- > 0) //делаем несколько попыток записи
            {
                try
                {
                    StreamWriter outfile = new StreamWriter("" + AppDomain.CurrentDomain.BaseDirectory + @"/log/book_" + DateTime.Today.ToString("yyyy-MM-dd") + ".log", true);

                    outfile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
                    outfile.WriteLine();
                    outfile.WriteLine();

                    outfile.Close();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}