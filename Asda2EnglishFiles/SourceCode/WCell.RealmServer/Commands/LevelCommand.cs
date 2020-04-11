using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class LevelCommand : RealmServerCommand
    {
        protected LevelCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Level");
            this.EnglishParamInfo = "[-o] <level>";
            this.EnglishDescription =
                "Sets the target's level. Using -o Allows overriding the servers configured Max Level";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string str = trigger.Text.NextModifiers();
            Unit target = trigger.Args.Target;
            int num = trigger.Text.NextInt(target.Level);
            if (!str.Contains("o") && num > target.MaxLevel)
                trigger.Reply("Max Level is {0} use the -o switch if you intended to set above this",
                    (object) target.MaxLevel);
            else
                target.Level = num;
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}