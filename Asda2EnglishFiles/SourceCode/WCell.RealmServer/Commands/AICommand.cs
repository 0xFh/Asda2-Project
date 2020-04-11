using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AICommand : RealmServerCommand
    {
        protected AICommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("AI");
            this.EnglishDescription = "Provides Commands to interact with AI.";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }

        public class AIActiveCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Active");
                this.EnglishParamInfo = "<1/0>";
                this.EnglishDescription = "Activates/Deactivates AI of target.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (target == trigger.Args.Character)
                    target = trigger.Args.Character.Target;
                if (!(target is NPC))
                {
                    trigger.Reply("Must target NPC.");
                }
                else
                {
                    IBrain brain = target.Brain;
                    if (brain == null)
                    {
                        trigger.Reply(target.Name + " doesn't have a brain.");
                    }
                    else
                    {
                        bool flag = !trigger.Text.HasNext ? !brain.IsRunning : trigger.Text.NextBool();
                        brain.IsRunning = flag;
                        trigger.Reply(target.Name + "'s Brain is now: " + (flag ? "Activated" : "Deactivated"));
                    }
                }
            }
        }

        public class AIMoveToMeCommand : RealmServerCommand.SubCommand
        {
            protected AIMoveToMeCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("MoveToMe", "Come");
                this.EnglishDescription = "Moves a target NPC to the character.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (target == trigger.Args.Character)
                    target = trigger.Args.Character.Target;
                if (!(target is NPC))
                    trigger.Reply("Can only command NPCs.");
                else
                    target.MoveToThenIdle((IHasPosition) trigger.Args.Character);
            }
        }

        public class AIFollowCommand : RealmServerCommand.SubCommand
        {
            protected AIFollowCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Follow");
                this.EnglishDescription = "Moves a target NPC to the character.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (target == trigger.Args.Character)
                    target = trigger.Args.Character.Target;
                if (!(target is NPC))
                    trigger.Reply("Can only command NPCs.");
                else
                    target.Follow((Unit) trigger.Args.Character);
            }
        }
    }
}