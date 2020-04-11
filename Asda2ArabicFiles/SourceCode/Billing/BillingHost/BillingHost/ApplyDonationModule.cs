using System;
using System.IO;
using System.Linq;
using Nancy;

namespace BillingHost
{
    public class ApplyDonationModule : NancyModule
    {
        public ApplyDonationModule()
        {

            Get["/apply_donation/{payerName}/{transactionId}/{characterName}"] = _ =>
            {
                try
                {

                    string payerName = _.payerName;
                    int transactionId;
                    try
                    {
                        transactionId = int.Parse(_.transactionId.ToString());
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("wrong_transaction_id");
                    }
                    string characterName = _.characterName;
                    using (var context = new asda2x100Entities())
                    {
                        var donationRecord = context.donations.FirstOrDefault(d => d.PayerName == payerName && d.TransactionId == transactionId);
                        var characterRecord = context.characterrecord.FirstOrDefault(c => c.Name == characterName);
                        if (characterRecord == null)
                        {
                            throw new InvalidOperationException("character_not_found");
                        }
                        if (donationRecord == null)
                        {
                            throw new InvalidOperationException("donation_not_founded");
                        }
                        if (donationRecord.IsDelivered)
                        {
                            throw new InvalidOperationException("already_delivered");
                        }
                        if (donationRecord.CharacterName != null)
                        {
                            throw new InvalidOperationException("character_name_already_seted");
                        }
                        donationRecord.CharacterName = characterRecord.Name;
                        context.SaveChanges();
                    }
                    return "ok";
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            };

            Get["/Scripts/{path}"] = _ =>
            {
                try
                {
#if DEBUG
                    return File.ReadAllText(Path.Combine("..\\..\\Scripts", _.path));
#endif
                    return File.ReadAllText(Path.Combine("Scripts", _.path));
                }
                catch (Exception)
                {
                    return "";
                }
            };
            Get["/Views/{path}"] = _ =>
            {
                try
                {
#if DEBUG
                    return File.ReadAllText(Path.Combine("..\\..\\Views", _.path));
#endif
                    return File.ReadAllText(Path.Combine("Views", _.path));
                }
                catch (Exception)
                {
                    return "";
                }
            };
        }
    }
}