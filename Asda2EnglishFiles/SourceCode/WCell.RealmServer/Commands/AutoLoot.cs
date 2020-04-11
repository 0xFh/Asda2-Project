using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AutoLoot : RealmServerCommand
    {
        protected AutoLoot()
        {
        }

        protected override void Initialize()
        {
            this.Init(nameof(AutoLoot), "al");
            this.EnglishParamInfo = "[-o] <level>";
            this.EnglishDescription = "Enables or disbles autoloot";
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
                target.AutoLoot = !target.AutoLoot;
                trigger.Reply("Autoloot {0}.", target.AutoLoot ? (object) "On" : (object) "Off");
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}