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
