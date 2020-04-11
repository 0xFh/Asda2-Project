using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class KillCommand : RealmServerCommand
    {
        protected KillCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Kill");
            this.EnglishDescription = "Kills your current target.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            if (target == trigger.Args.Character)
            {
                target = target.Target;
                if (target == null)
                {
                    trigger.Reply("Invalid Target.");
                    return;
                }
            }

            target.Kill((Unit) trigger.Args.Character);
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }
    }
}