using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AbandonCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Abandon");
            this.EnglishDescription = "Abandons the current Target";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            Character character = trigger.Args.Character;
            if (target == character)
                target = character.Target;
            if (!(target is NPC))
                trigger.Reply("Invalid target - Need to target an NPC.");
            else
                target.Master = target;
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }
    }
}