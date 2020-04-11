using System;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    public class BanChatCommand : RealmServerCommand
    {
        protected BanChatCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("banchat");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string name = trigger.Text.NextWord();
            int num = trigger.Text.NextInt(60);
            string str = trigger.Text.NextQuotedString();
            Character character = World.GetCharacter(name, false);
            if (character == null)
            {
                trigger.Reply("character not founded");
            }
            else
            {
                character.ChatBanned = true;
                character.BanChatTill = new DateTime?(DateTime.Now.AddMinutes((double) num));
                World.BroadcastMsg("Ban system",
                    string.Format("{0} chat is banned by {1} for {2} minutes. Reason : {3}.", (object) name,
                        (object) trigger.Args.Character.Name, (object) num, (object) str), Color.Red);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}