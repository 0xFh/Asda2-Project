using System;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class EmailCommand : RealmServerCommand
    {
        protected EmailCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Email", "SetEmail");
            this.EnglishParamInfo = "<email>";
            this.EnglishDescription = "Sets the Account's current email address.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string email = trigger.Text.NextWord();
            if (!Utility.IsValidEMailAddress(email))
            {
                trigger.Reply("Invalid Mail address.");
            }
            else
            {
                trigger.Reply("Setting mail address to " + email + "...");
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
                {
                    if (((Character) trigger.Args.Target).Account.SetEmail(email))
                        trigger.Reply("Done.");
                    else
                        trigger.Reply("Could not change email-address.");
                })));
            }
        }
    }
}