using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ServiceTest.InnaService.BookServiceSoapClient();

            Console.WriteLine(client.CreateDogovorPayment("123", 1, "1", 1m, "1"));
            Console.ReadKey();
        }
    }
}
