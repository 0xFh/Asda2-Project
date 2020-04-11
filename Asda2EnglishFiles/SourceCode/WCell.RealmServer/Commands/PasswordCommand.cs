using NLog;
using System;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// TODO: Figure out how to verify password
    /// TODO: PW should be queried after the command has been executed
    /// </summary>
    public class PasswordCommand : RealmServerCommand
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        protected PasswordCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Password", "Pass");
            this.EnglishParamInfo = "<oldpw> <newpw> <newpw>";
            this.EnglishDescription = "Changes your password.";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Player; }
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flag = trigger.Args.Character == trigger.Args.Target;
            if (!flag && trigger.Args.Character != null && trigger.Args.Character.Role < RoleStatus.Admin)
            {
                trigger.Reply("Only Admins or Account-owners are allowed to change Account passwords.");
            }
            else
            {
                string oldPass = !flag ? (string) null : trigger.Text.NextWord();
                string pass = trigger.Text.NextWord();
                string str = trigger.Text.NextWord();
                if (pass.Length < 3)
                    trigger.Reply("Account password must at least be {0} characters long.", (object) 3);
                else if (pass.Length > 16)
                    trigger.Reply("Account password length must not exceed {0} characters.", (object) 16);
                else if (pass != str)
                {
                    trigger.Reply("Passwords don't match.");
                }
                else
                {
                    trigger.Reply("Setting password...");
                    ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
                    {
                        if (((Character) trigger.Args.Target).Account.SetPass(oldPass, pass))
                            trigger.Reply("Done.");
                        else
                            trigger.Reply("Unable to set Password. Make sure your old password is correct.");
                    })));
                }
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}