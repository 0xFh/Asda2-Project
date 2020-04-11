using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    public class UnBanChatCommand : RealmServerCommand
    {
        protected UnBanChatCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("unbanchat");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string name = trigger.Text.NextWord();
            Character character = World.GetCharacter(name, false);
            if (character == null)
            {
                trigger.Reply("character not founded");
            }
            else
            {
                character.ChatBanned = false;
                World.BroadcastMsg("Ban system",
                    string.Format("{0} chat is unbanned by {1}.", (object) name, (object) trigger.Args.Character.Name),
                    Color.Red);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}