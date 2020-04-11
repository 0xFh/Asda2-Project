using WCell.Constants.Updates;
using WCell.Intercommunication;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AddLuckCommand : RealmServerCommand
    {
        protected AddLuckCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("AddLuck", "aluc");
            this.EnglishParamInfo = "<points>";
            this.EnglishDescription = "Adds points to base stat Luck";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("Enter the numper of points you want to add. Ex: AddLuck 20");
            }
            else
            {
                Character character = trigger.Args.Character;
                if (character == null)
                    return;
                int points = trigger.Text.NextInt(0);
                string text = character.TryAddStatPoints(Asda2StatType.Luck, points);
                trigger.Reply(text);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}