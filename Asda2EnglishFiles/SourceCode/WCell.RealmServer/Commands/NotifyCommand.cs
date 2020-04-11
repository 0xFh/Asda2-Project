using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class NotifyCommand : RealmServerCommand
    {
        protected NotifyCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Notify", "SendNotification");
            this.EnglishParamInfo = "<text>";
            this.EnglishDescription = "Notifies the target with a flashing message.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string remainder = trigger.Text.Remainder;
            if (remainder.Length <= 0)
                return;
            ((Character) trigger.Args.Target).Notify(remainder);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}