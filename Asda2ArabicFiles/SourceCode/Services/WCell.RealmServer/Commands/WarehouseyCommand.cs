using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class WarehouseyCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.Player;
            }
        }
        protected override void Initialize()
        {
            Init("Warehouse");

            EnglishDescription = "Used for manipulation warehouse";
        }
        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
        public class UnlockCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Unlock", "u");
                EnglishDescription = "Unlocks your warehouse";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (!trigger.Args.Character.IsWarehouseLocked)
                {
                    trigger.Reply("Warehouse is not locked.");
                    return;
                }
                var whPass = trigger.Args.Character.Record.WarehousePassword;
                if (whPass == null)
                {
                    trigger.Reply("Password not seted yet. Warehouse is not locked.");
                    return;
                }
                var pass = trigger.Text.NextWord();
                if (pass != whPass)
                {
                    trigger.Reply("Wrong password.");
                    return;
                }
                trigger.Args.Character.IsWarehouseLocked = false;
                trigger.Reply("Warehouse unlocked.");
            }
        }
        public class SetPasswordCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Set");
                EnglishDescription = "Sets warehouse password";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (trigger.Args.Character.Record.WarehousePassword != null)
                {
                    trigger.Reply("Password is already seted. Use <#warehouse reset [oldpass] [newpass]> command to reset it.");
                    return;
                }
                var pass = trigger.Text.NextWord();
                if (string.IsNullOrWhiteSpace(pass))
                {
                    trigger.Reply("Enter not empty password.");
                    return;
                }
                trigger.Args.Character.Record.WarehousePassword = pass;
                trigger.Reply(string.Format("Password was seted to [{0}].", pass));
            }
        }
        public class ResetPasswordCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Reset");
                EnglishDescription = "Resets warehouse password";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (trigger.Args.Character.Record.WarehousePassword == null)
                {
                    trigger.Reply("Password is not seted yet. Use <#warehouse set [pass]> command to set it.");
                    return;
                }
                var oldPass = trigger.Text.NextWord();
                if (string.IsNullOrWhiteSpace(oldPass))
                {
                    trigger.Reply("Enter not empty old password.");
                    return;
                }
                if (oldPass != trigger.Args.Character.Record.WarehousePassword)
                {
                    trigger.Reply("Wrong password.");
                    return;
                }
                string newPass = null;
                if (trigger.Text.HasNext) newPass = trigger.Text.NextWord();
                if (string.IsNullOrWhiteSpace(newPass))
                {
                    trigger.Reply("You enter empty new password. Clearing your warehouse password.");
                    trigger.Args.Character.Record.WarehousePassword = null;
                    return;
                }
                trigger.Args.Character.Record.WarehousePassword = newPass;
                trigger.Reply(string.Format("Password was seted to [{0}].", newPass));
            }
        }
    }
}