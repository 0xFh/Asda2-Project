using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class MoneyCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Money");
            EnglishDescription = "Used for manipulation money";
        }
        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
        public class AddCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Add", "a");
                EnglishDescription = "Adds money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var amount = trigger.Text.NextUInt(1);
                var character = trigger.Args.Target as Character;
                if (character != null)
                {
                    character.AddMoney(amount);
                    character.SendMoneyUpdate();
                    trigger.Reply("Done.");
                }
                else
                {
                    trigger.Reply("Wrong target.");
                }
            }
        }
        public class SubstractCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Substract", "Sub", "S");
                EnglishDescription = "Substacts money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var amount = trigger.Text.NextUInt(1);
                var character = trigger.Args.Target as Character;
                if (character != null)
                {
                    character.SubtractMoney(amount);
                    character.SendMoneyUpdate();

                    trigger.Reply("Done.");
                }
                else
                {
                    trigger.Reply("Wrong target.");
                }
            }
        }
        public class SetCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Set");
                EnglishDescription = "Sets money";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var amount = trigger.Text.NextUInt(1);
                var character = trigger.Args.Target as Character;
                if (character != null)
                {
                    character.AddMoney(amount);
                    character.SubtractMoney(1);
                    character.SendMoneyUpdate();
                    trigger.Reply("Done.");
                }
                else
                {
                    trigger.Reply("Wrong target.");
                }
            }
        }
    }
}