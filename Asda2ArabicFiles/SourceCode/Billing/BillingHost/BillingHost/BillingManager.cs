using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Billing.BLL.Pop3;
using BillingHost;

namespace Billing.BLL
{
    public class BillingManager
    {
        private static BillingManager _instance;
        public static BillingManager Instance
        {
            get
            {
                lock (typeof(BillingManager))
                {
                    if (_instance == null)
                    {
                        _instance = new BillingManager();
                    }
                }
                return _instance;
            }
        }

        private Thread CheckMailThread;

        private void CheckMailCaller()
        {
            while (true)
            {
                CheckMail();
                Thread.Sleep(5000);
            }
        }

        public void Start()
        {
            const string url = "http://localhost:9090";
            var nancyHost = new Nancy.Hosting.Self.NancyHost(new Uri(url));
            nancyHost.Start();
            CheckMailThread = new Thread(CheckMailCaller);
            CheckMailThread.Start();
                
            
            Console.WriteLine("Billing service is listening on {0}", url);
        }

        public void Stop()
        {
            CheckMailThread.Abort();
        }

        private void CheckMail()
        {Console.WriteLine("start check mail");
            try
            {

                var popClient = new Pop3MimeClient("pop.gmail.com", 995, true, "plus79672798453@gmail.com", "jjpLMr6TN6wTajpyu4Wb");

                popClient.ReadTimeout = 10000; //give pop server 60 seconds to answer

                //establish connection
                popClient.Connect();

                List<int> list;
                popClient.GetEmailIdList(out list);

                foreach (var mailId in list)
                {
                    RxMailMessage mm;
                    popClient.GetEmail(mailId, out mm);
                    if (mm != null)
                    {
                        if (
                            mm.From.Address != "no-reply@cashu.com" ||
                            mm.From.DisplayName != "cashU Team" ||
                            mm.From.Host != "cashu.com" ||
                            mm.From.User != "no-reply")
                        {
                            popClient.DeleteEmail(mailId);
                        }
                        else
                        {
                            try
                            {
                                ProcessPaymentEmail(mm.Entities[0].Body);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to process email. " + ex.Message);
                            }
                            popClient.DeleteEmail(mailId);
                        }
                    }
                }
                popClient.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to check emails. " + ex.Message);
            }
            Console.WriteLine("end check mail");
        }
        const string RowStart = "<font color='#FAAF07' style='font-family:tahoma; font-size:10px; font-weight:bold; line-height:22px'>";

        private void ProcessPaymentEmail(string body)
        {
            var dataStrings = new List<string>();
            var indexOfStart = 0;
            while (true)
            {
                indexOfStart = body.IndexOf(RowStart, indexOfStart + 1, StringComparison.Ordinal);
                if (indexOfStart == -1)
                    break;
                var indexOfEnd = body.IndexOf("<br />", indexOfStart + 1, StringComparison.Ordinal);
                var startIndex = indexOfStart + RowStart.Length;
                var dataStr = body.Substring(startIndex, indexOfEnd - startIndex).Replace('\r', ' ').Replace('\t', ' ').Replace('\n', ' ');
                dataStrings.Add(dataStr.Trim());
            }
            var payerName = dataStrings[0];
            var paymentDate = dataStrings[1];
            var donatePlanName = dataStrings[2];
            var donateAmountStr = dataStrings[3].Split(' ');
            var currency = donateAmountStr[0];
            var amount = double.Parse(donateAmountStr[1].Replace('.',',')) * 100;
            var transactionId = long.Parse(dataStrings[4]);
            if (currency != "USD")
                throw new InvalidOperationException("Currency is not USD! " + currency);
            using (var dbContext = new asda2x100Entities())
            {
                var record = dbContext.donations.FirstOrDefault(d => d.TransactionId == transactionId && d.Wallet == 2);
                if (record != null)
                {
                    Console.WriteLine("duplication record " + transactionId);
                    return;
                }
                dbContext.donations.Add(new donations
                {
                    Amount = (int)amount,
                    CreateDateTime = DateTime.Now,
                    Wallet = 2,
                    TransactionId = transactionId,
                    IsDelivered = false,
                    PayerName = payerName
                });

                dbContext.SaveChanges();
                var info = string.Format("{0} {1} {2} {3} {4} {5} {6}", DateTime.Now, payerName, paymentDate,
                    donatePlanName, currency, amount, transactionId);
                Console.WriteLine("to db => " + info);
                using (var sw = new StreamWriter("cashu.txt", true))
                {
                    sw.WriteLine(info);
                }
            }
        }
    }
}
