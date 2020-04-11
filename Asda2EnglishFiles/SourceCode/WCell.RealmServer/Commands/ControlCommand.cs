using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ControlCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Control", "Enslave");
            this.EnglishParamInfo = "[-p]";
            this.EnglishDescription =
                "Makes the current Target your minion. -p makes it your current Pet or companion if possible.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            Character character = trigger.Args.Character;
            if (target == character)
                target = character.Target;
            if (!(target is NPC))
            {
                trigger.Reply("Invalid target - Need to target an NPC.");
            }
            else
            {
                NPC minion = (NPC) target;
                if (trigger.Text.NextModifiers() == "p")
                    character.MakePet(minion, 0);
                else
                    character.Enslave(minion, 0);
            }
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }
    }
}