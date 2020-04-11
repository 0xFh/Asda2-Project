using WCell.Constants.Updates;
using WCell.Intercommunication;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AddStaminaCommand : RealmServerCommand
    {
        protected AddStaminaCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("AddStamina", "asta");
            this.EnglishParamInfo = "<points>";
            this.EnglishDescription = "Adds points to base stat stamina";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("Enter the numper of points you want to add. Ex: AddStamina 20");
            }
            else
            {
                Character character = trigger.Args.Character;
                if (character == null)
                    return;
                int points = trigger.Text.NextInt(0);
                string text = character.TryAddStatPoints(Asda2StatType.Stamina, points);
                trigger.Reply(text);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}