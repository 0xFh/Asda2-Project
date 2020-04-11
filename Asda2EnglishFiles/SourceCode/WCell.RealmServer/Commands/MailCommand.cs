using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Network;
using WCell.Util.Commands;
using WCell.Util.Strings;

namespace WCell.RealmServer.Commands
{
    public class MailCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Mail");
        }

        public class ReadMailCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Read", "R");
                this.EnglishParamInfo = "";
                this.EnglishDescription = "Read all mails";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character character = trigger.Args.Character;
                if (character == null)
                    trigger.Reply("Cannot read Mails if no Character is given (yet).");
                else
                    character.MailAccount.SendMailList();
            }
        }

        public class SendMailCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Send", "S");
                this.EnglishParamInfo = "[-i[c][m] <ItemId> [<CoD>] [<money>]] <receiver> <subject>, <text>";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                StringStream text = trigger.Text;
                string str = text.NextModifiers();
                List<ItemRecord> itemRecordList = new List<ItemRecord>();
                uint cod = 0;
                uint money = 0;
                if (str.Contains("i"))
                {
                    ItemTemplate template = ItemMgr.GetTemplate(trigger.Text.NextEnum<Asda2ItemId>(Asda2ItemId.None));
                    if (template == null)
                    {
                        trigger.Reply("Invalid ItemId.");
                        return;
                    }

                    ItemRecord record = ItemRecord.CreateRecord(template);
                    itemRecordList.Add(record);
                    record.SaveLater();
                }

                if (str.Contains("c"))
                    cod = text.NextUInt();
                string recipientName = trigger.Text.NextWord();
                if (recipientName.Length == 0)
                {
                    trigger.Reply("Could not send mail - Unknown Recipient: " + recipientName);
                }
                else
                {
                    string subject = trigger.Text.NextWord(",");
                    string remainder = trigger.Text.Remainder;
                    MailError mailError = MailMgr.SendMail(recipientName, subject, remainder, MailStationary.GM,
                        (ICollection<ItemRecord>) itemRecordList, money, cod, (IPacketReceiver) trigger.Args.User);
                    if (mailError == MailError.OK)
                        trigger.Reply("Done.");
                    else
                        trigger.Reply("Could not send mail: " + (object) mailError);
                }
            }
        }
    }
}