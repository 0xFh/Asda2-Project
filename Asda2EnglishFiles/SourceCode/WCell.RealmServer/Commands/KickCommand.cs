using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class KickCommand : RealmServerCommand
    {
        protected KickCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Kick", "Boot");
            this.EnglishParamInfo = "[-n <name>][-d <seconds>] [<reason>]";
            this.EnglishDescription =
                "Kicks your current target with an optional delay in seconds (default: 20 - can be 0) and an optional reason.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger is IngameCmdTrigger && trigger.Args.Target == trigger.Args.Character.Target)
            {
                trigger.Reply("You cannot kick yourself.");
            }
            else
            {
                Character character = trigger.Args.Character.Target as Character;
                string str = trigger.Text.NextModifiers();
                if (character == null)
                {
                    if (!str.Contains("n") || !trigger.Text.HasNext)
                    {
                        trigger.Reply(RealmLangKey.CmdKickMustProvideName);
                        return;
                    }

                    string name = trigger.Text.NextWord();
                    character = World.GetCharacter(name, false);
                    if (character == null)
                    {
                        trigger.Reply(RealmLangKey.PlayerNotOnline, (object) name);
                        return;
                    }
                }

                int num = Character.DefaultLogoutDelayMillis;
                if (str.Contains("d"))
                    num = trigger.Text.NextInt(num) * 1000;
                string reason = trigger.Text.Remainder.Trim();
                character.Kick((INamed) trigger.Args.User, reason, num);
            }
        }
    }
}