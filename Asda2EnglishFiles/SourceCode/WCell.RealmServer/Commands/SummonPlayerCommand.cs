using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SummonPlayerCommand : RealmServerCommand
    {
        protected SummonPlayerCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("Summon");
            this.EnglishParamInfo = "[-aq] <name>";
            this.EnglishDescription =
                "Summons the Player with the given name. -q will queries Player before teleporting (can be denied). -a switch will use the account name instead of the Char-name.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("You need to specify the name of the Player to be summoned.");
            }
            else
            {
                string str = trigger.Text.NextModifiers();
                bool flag = str.Contains("q");
                string name = trigger.Text.NextWord();
                Character character;
                if (str.Contains("a"))
                {
                    RealmAccount loggedInAccount =
                        ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(name);
                    character = loggedInAccount == null ? (Character) null : loggedInAccount.ActiveCharacter;
                }
                else
                    character = World.GetCharacter(name, false);

                if (character == null)
                    trigger.Reply(RealmLangKey.CmdSummonPlayerNotOnline, (object) name);
                else if (flag || character.Role > trigger.Args.Character.Role)
                    character.StartSummon((ISummoner) trigger.Args.Character);
                else
                    character.TeleportTo(trigger.Args.Target.Map, trigger.Args.Target.Position);
            }
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}