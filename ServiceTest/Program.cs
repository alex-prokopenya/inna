using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
        //    CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ",";

            decimal test = 1134M;

            Console.WriteLine(test.ToString());

            Console.WriteLine(test.ToString("0,00"));

            Console.WriteLine(test.ToString("G"));

            Console.WriteLine(test.ToString("0,0000"));

            Console.ReadKey();
            return;
            var client = new ServiceTest.InnaService.BookServiceSoapClient();

            var resp = client.GetDepositAndReceivable(1);

            if (resp.Item is InnaService.InTourist)
                Console.WriteLine((resp.Item as InnaService.InTourist).name);

            else
                if (resp.Item is InnaService.DepositInfo[])
                    Console.WriteLine((resp.Item as InnaService.DepositInfo[])[0].Deposit);

            Console.ReadKey();
        }
    }
}
