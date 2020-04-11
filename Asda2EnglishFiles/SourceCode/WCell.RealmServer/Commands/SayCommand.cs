using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SayCommand : RealmServerCommand
    {
        protected SayCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Say");
            this.EnglishParamInfo = "<text>";
            this.EnglishDescription = "Say something";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            trigger.Args.Target.Say(trigger.Text.Remainder.Trim());
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}