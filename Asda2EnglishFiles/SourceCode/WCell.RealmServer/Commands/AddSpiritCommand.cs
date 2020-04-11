using WCell.Constants.Updates;
using WCell.Intercommunication;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AddSpiritCommand : RealmServerCommand
    {
        protected AddSpiritCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("AddSpirit", "aspi");
            this.EnglishParamInfo = "<points>";
            this.EnglishDescription = "Adds points to base stat Energy";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("Enter the numper of points you want to add. Ex: AddEnergy 20");
            }
            else
            {
                Character character = trigger.Args.Character;
                if (character == null)
                    return;
                int points = trigger.Text.NextInt(0);
                string text = character.TryAddStatPoints(Asda2StatType.Spirit, points);
                trigger.Reply(text);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}