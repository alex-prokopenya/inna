using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Threading;
using ServiceTest.InnaService;
using System.Xml;
using System.Xml.Serialization;


namespace ServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //создаем заказы
                var codes = TestCreateDogovor();

                //платим
                foreach (string code in codes)
                {
                    TestPayments(code, 1, "PSB", new Random(DateTime.Now.Millisecond).Next(10000) + 1000, (new Random().Next(10000) + 10000).ToString());
                    Thread.Sleep(1000);
                }

                //платим
                foreach (string code in codes)
                {
                    TestDepositPayments(code);
                    Thread.Sleep(1000);
                }

                //берем инфо по депозиту
                int[] partnerKeys = new int[] {56820,59527,59224,6,21,973 };

                foreach (int prKey in partnerKeys)
                    TestDeposit(prKey);

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                Console.ReadKey();
            }
        }

        private static void TestDeposit(int partnerKey)
        {
            var client = new BookServiceSoapClient();
            client.Open();

            WriteToLog(String.Format("check deposit partner key {0}", partnerKey));

            var resp = client.GetDepositAndReceivable(partnerKey);

            WriteToLog(Serialize(resp, resp.GetType()));

            client.Close();
        }

        private static void TestDepositPayments(string dogCode)
        {
            var client = new BookServiceSoapClient();
            client.Open();

            WriteToLog(String.Format("dogCode = {0}", dogCode));

            var resp = client.CreateDogovorDepositPayment(dogCode);

            WriteToLog(Serialize(resp, resp.GetType()));

            client.Close();
        }

        private static void TestPayments(string dogCode, int paymentType, string paymentSys, decimal paidSum, string paymentId)
        {
            var client = new BookServiceSoapClient();
            client.Open();

            WriteToLog(String.Format("dogCode = {0}, paymentType={1}, paymentSys={2}, paidSum={3}, paymentId={4}", dogCode, paymentType, paymentSys, paidSum, paymentId));

            var resp = client.CreateDogovorPayment(dogCode, paymentType, paymentSys, paidSum, paymentId);

            WriteToLog(Serialize(resp, resp.GetType()));

            client.Close();
        }

        private static List<string> TestCreateDogovor()
        {
            List<string> createdDogovors = new List<string>();

            WriteToLog("TestCreateDogovor");

            var client = new BookServiceSoapClient();
            client.Open();

            var turists = new InTourist[] { 
            
                new InTourist(){
                    BirthDate = "2010-01-01",
                    Citizenship = "BY",
                    FirstName = "Alexey",
                    LastName = "Ivanov",
                    PasspordCode = "MP",
                    PasspordNumber = "1122334455",
                    PassportValidDate = "",
                    PassrortType = PasportType.INTERNAL,
                    Sex = Sex.M
                },
                new InTourist(){
                    BirthDate = "1980-01-01",
                    Citizenship = "RU",
                    FirstName = "Alena",
                    LastName = "Frolova",
                    PasspordCode = "67",
                    PasspordNumber = "132659616",
                    PassportValidDate = "2020-01-01",
                    PassrortType = PasportType.FOREIGN,
                    Sex = Sex.F
                },
                new InTourist(){
                    BirthDate = "1999-12-01",
                    Citizenship = "UA",
                    FirstName = "Alexey",
                    LastName = "Ivanov",
                    PasspordCode = "MP",
                    PasspordNumber = "1122334455",
                    PassportValidDate = "",
                    PassrortType = PasportType.INTERNAL,
                    Sex = Sex.M
                }
            };

            var userInfo = new InnaService.UserInfo(){
                AgentLogin = "",
                Email = "nomail@tut.by",
                Name = "Ivan",
                Phone = "12454345"
            };

            var AgentInfo = new InnaService.UserInfo(){
                AgentLogin = "geotrav"
            };

            var AgentInfo2 = new InnaService.UserInfo(){
                AgentLogin = "YTV"
            };

            var servicesOne = new InService[]{ //отель
                new InService(){
                    Comission = 10,
                    Date = "2014-10-09",
                    NDays = 12,
                    NettoPrice = 1980,
                    PartnerBookID = "21234234234224",
                    PartnerID = 57448,
                    Price = 2030,
                    ServiceType = ServiceType.HOTEL,
                    Title = "Grand efe 4*, Кушадасы, dbl sea view, All",
                    TuristIndexes = new int[0]
                }
            };

            var servicesTwo = new InService[]{ //перелет
                new InService(){
                    Comission = 500,
                    Date = "2015-10-09",
                    NDays = 1,
                    NettoPrice = 500,
                    PartnerBookID = "1234232345",
                    PartnerID = 57448,
                    Price = 1500,
                    ServiceType = ServiceType.AVIA,
                    Title = "Минск-Измир, B2 8420, 18:00-20:30",
                    TuristIndexes = new int[0],
                    NumDocs = new string[]{"num_one","num_two"}
                }
            };

            var servicesThree = new InService[]{ //отели + перелеты
                
                 new InService(){
                        Comission = 0,
                        Date = "2014-10-09",
                        NDays = 1,
                        NettoPrice = 0,
                        PartnerBookID = "1234232345",
                        PartnerID = 57448,
                        Price = 0,
                        ServiceType = ServiceType.AVIA,
                        Title = "Измир-Минск, B2 8420, 18:00-20:30",
                        TuristIndexes = new int[0],
                        NumDocs = new string[]{"num_1","num_2"}
                    },

                    new InService(){
                        Comission = 100,
                        Date = "2014-10-01",
                        NDays = 1,
                        NettoPrice = 560,
                        PartnerBookID = "1234232345",
                        PartnerID = 57448,
                        Price = 700,
                        ServiceType = ServiceType.AVIA,
                        Title = "Минск-Измир, B2 8420, 12:00-14:30",
                        TuristIndexes = new int[0]
                    },

                    new InService(){
                        Comission = 10,
                        Date = "2014-10-01",
                        NDays = 4,
                        NettoPrice = 1980,
                        PartnerBookID = "21234234234224",
                        PartnerID = 57448,
                        Price = 2030,
                        ServiceType = ServiceType.HOTEL,
                        Title = "Grand efe 4*, Кушадасы, tpl sea view, All",
                        TuristIndexes = new int[] {1,2,3}
                    },
                    new InService(){
                        Comission = 10,
                        Date = "2014-10-04",
                        NDays = 6,
                        NettoPrice = 2980,
                        PartnerBookID = "21234234234224",
                        PartnerID = 57448,
                        Price = 3030,
                        ServiceType = ServiceType.HOTEL,
                        Title = "Grand efe 4*, Кушадасы, dbl sea view, All",
                        TuristIndexes = new int[] {1,3}
                    },
                    new InService(){
                        Comission = 10,
                        Date = "2014-10-04",
                        NDays = 6,
                        NettoPrice = 3980,
                        PartnerBookID = "21234234234224",
                        PartnerID = 57448,
                        Price = 4030,
                        ServiceType = ServiceType.HOTEL,
                        Title = "Grand efe 4*, Кушадасы, sgl sea view, All",
                        TuristIndexes = new int[]{2}
                    },
            };

            WriteToLog(""); WriteToLog("");
            WriteToLog("try turists, userInfo, servicesOne");

            Thread.Sleep(1000);
            var resp = client.CreateDogovor(turists, userInfo, servicesOne);

            WriteToLog(Serialize(resp, resp.GetType()));

            if ((resp.Item is string) && (resp.hasErrors == false))
                createdDogovors.Add(resp.Item.ToString());

            WriteToLog(""); WriteToLog("");
            WriteToLog("try turists, AgentInfo, servicesTwo");

            Thread.Sleep(1000);
            resp = client.CreateDogovor(turists, AgentInfo, servicesTwo);

            WriteToLog(Serialize(resp, resp.GetType()));

            if ((resp.Item is string) && (resp.hasErrors == false))
                createdDogovors.Add(resp.Item.ToString());

            WriteToLog(""); WriteToLog("");
            WriteToLog("try turists, userInfo, servicesThree");

            Thread.Sleep(1000);
            resp = client.CreateDogovor(turists, userInfo, servicesThree);

            WriteToLog(Serialize(resp, resp.GetType()));

            if ((resp.Item is string) && (resp.hasErrors == false))
                createdDogovors.Add(resp.Item.ToString());

            WriteToLog(""); WriteToLog("");
            WriteToLog("try turists, AgentInfo, servicesThree");

            Thread.Sleep(1000);
            resp = client.CreateDogovor(turists, AgentInfo2, servicesThree);

            WriteToLog(Serialize(resp, resp.GetType()));

            if ((resp.Item is string) && (resp.hasErrors == false))
                createdDogovors.Add(resp.Item.ToString());

            client.Close();

            WriteToLog("end of TestCreateDogovor");

            return createdDogovors;
        }

        private static string Serialize(object obj, Type type)
        {
            var sw = new StringWriter();
            new XmlSerializer(type).Serialize(sw, obj);

            return sw.ToString();
        }


        private static void WriteToLog(string message)
        {
            Console.WriteLine(message);
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
    }
}
