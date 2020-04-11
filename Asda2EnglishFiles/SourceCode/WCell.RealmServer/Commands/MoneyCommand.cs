using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class MoneyCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Money");
            this.EnglishDescription = "Used for manipulation money";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        public class AddCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Add", "a");
                this.EnglishDescription = "Adds money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint amount = trigger.Text.NextUInt(1U);
                Character target = trigger.Args.Target as Character;
                if (target != null)
                {
                    target.AddMoney(amount);
                    target.SendMoneyUpdate();
                    trigger.Reply("Done.");
                }
                else
                    trigger.Reply("Wrong target.");
            }
        }

        public class SubstractCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Substract", "Sub", "S");
                this.EnglishDescription = "Substacts money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint amount = trigger.Text.NextUInt(1U);
                Character target = trigger.Args.Target as Character;
                if (target != null)
                {
                    target.SubtractMoney(amount);
                    target.SendMoneyUpdate();
                    trigger.Reply("Done.");
                }
                else
                    trigger.Reply("Wrong target.");
            }
        }

        public class SetCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Set");
                this.EnglishDescription = "Sets money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint amount = trigger.Text.NextUInt(1U);
                Character target = trigger.Args.Target as Character;
                if (target != null)
                {
                    target.AddMoney(amount);
                    target.SubtractMoney(1U);
                    target.SendMoneyUpdate();
                    trigger.Reply("Done.");
                }
                else
                    trigger.Reply("Wrong target.");
            }
        }
    }
}