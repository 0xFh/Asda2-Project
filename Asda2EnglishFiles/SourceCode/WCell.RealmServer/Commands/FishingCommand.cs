using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Fishing;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class FishingCommand : RealmServerCommand
    {
        protected FishingCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Fishing", "fish", "F");
            this.EnglishDescription = "Asda2 fishing commands";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }

        public class SetFishingLevelCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Level", "lvl", "l");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character target = trigger.Args.Target as Character;
                if (target == null)
                {
                    trigger.Reply("Wrong target.");
                }
                else
                {
                    target.FishingLevel = trigger.Text.NextInt(0);
                    trigger.Reply("Done.");
                }
            }
        }

        public class CompleteBooksCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("CompleteBooks", "Book", "b");
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character target = trigger.Args.Target as Character;
                if (target == null)
                    trigger.Reply("Wrong target.");
                else if (trigger.Text.String.Contains("all"))
                {
                    foreach (Asda2FishingBook asda2FishingBook in target.RegisteredFishingBooks.Values)
                        asda2FishingBook.Complete();
                    trigger.Reply("Done. Teleport to another location to refresh books.");
                }
                else
                {
                    int num = trigger.Text.NextInt(0);
                    if (target.RegisteredFishingBooks.ContainsKey((byte) num))
                    {
                        target.RegisteredFishingBooks[(byte) num].Complete();
                        trigger.Reply("Done.");
                    }
                    else
                        trigger.Reply("Book not founded.");
                }
            }
        }
    }
}