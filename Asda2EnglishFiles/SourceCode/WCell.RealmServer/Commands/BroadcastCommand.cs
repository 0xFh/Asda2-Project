using WCell.RealmServer.Handlers;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    public class BroadcastCommand : RealmServerCommand
    {
        protected BroadcastCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Broadcast", "Brc");
            this.EnglishParamInfo = "<text>";
            this.EnglishDescription = "Broadcasts the given text throughout the world.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Color yellow = Color.Yellow;
            if (trigger.Text.NextModifiers() == "c")
            {
                string result = (string) null;
                if (!trigger.Text.NextString(out result, " ") || result == null)
                {
                    trigger.Reply("Invalid color: \"" + result + "\"");
                    return;
                }

                //if (!Color.getColorByName(ref yellow, result))
                //  trigger.Reply("Invalid color: \"" + result + "\"");
            }

            if (trigger.Text.Remainder.Length > 100)
                trigger.Reply("Max length 100!");
            else if (trigger.Text.Remainder.Length == 0)
                trigger.Reply("Empty broadcast!");
            else if (trigger.Text.Remainder.Split(' ').Length == 0)
                trigger.Reply("Empty broadcast!");
            else
                GlobalHandler.SendGlobalMessage(trigger.Text.Remainder, yellow);
        }
    }
}