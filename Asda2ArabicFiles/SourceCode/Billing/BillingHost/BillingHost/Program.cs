using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billing.BLL;

namespace BillingHost
{
    class Program
    {
        static void Main(string[] args)
        {
            BillingManager.Instance.Start();
            Console.Read();
        }
    }
}
