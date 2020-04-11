using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class PinCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Pin", "PinDown", "Freeze");
            this.EnglishDescription = "Pins the targeted Unit down. Pinned down Units cannot move, fight or logout.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            if (object.ReferenceEquals((object) target, (object) trigger.Args.User))
            {
                target = target.Target;
                if (target == null)
                {
                    trigger.Reply("You cannot pin yourself down.");
                    return;
                }
            }

            target.IsPinnedDown = trigger.Text.NextBool(!target.IsPinnedDown);
            trigger.Reply("{0} has been {1}.", (object) target.Name,
                target.IsPinnedDown ? (object) "Pinned" : (object) "Unpinned");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}