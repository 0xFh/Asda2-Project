using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class InfoCommand : RealmServerCommand
    {
        protected InfoCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("Info", "I", "Address", "Addr");
            this.EnglishParamInfo = "[-l]";
            this.EnglishDescription = "Gives some server info. -l lists all players (if not too many).";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string str = trigger.Text.NextModifiers();
            int num1 = trigger.Text.NextInt(1);
            int num2 = trigger.Text.NextInt(250);
            int num3 = 0;
            if (str == "l")
            {
                int num4 = 0;
                foreach (Character allCharacter in World.GetAllCharacters())
                {
                    if (allCharacter.Level >= num1 && num2 >= allCharacter.Level)
                    {
                        ++num4;
                        if (allCharacter.Client.IsConnected)
                            ++num3;
                        trigger.Reply("{0}. {1} from {2} [{3}][{4}]", (object) num4, (object) allCharacter.Name,
                            (object) (Asda2FactionId) allCharacter.Asda2FactionId, (object) allCharacter.Level,
                            allCharacter.Client.IsConnected ? (object) "+" : (object) "-");
                    }
                }
            }

            trigger.Reply("Server has been running for {0}.",
                (object) ServerApp<WCell.RealmServer.RealmServer>.RunTime.Format());
            trigger.Reply("There are {0}[{1}] players online.", (object) World.CharacterCount, (object) num3);
        }
    }
}