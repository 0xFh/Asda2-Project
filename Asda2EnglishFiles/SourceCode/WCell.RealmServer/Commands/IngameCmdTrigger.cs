using WCell.RealmServer.Chat;
using WCell.RealmServer.Misc;
using WCell.Util.Strings;

namespace WCell.RealmServer.Commands
{
    /// <summary>Represents a trigger for commands through ingame chat</summary>
    public class IngameCmdTrigger : RealmServerCmdTrigger
    {
        public IngameCmdTrigger(StringStream text, IUser user, IGenericChatTarget target, bool dbl)
            : base(text, new RealmServerCmdArgs(user, dbl, target))
        {
        }

        public IngameCmdTrigger(RealmServerCmdArgs args)
            : base(args)
        {
        }

        public override void Reply(string txt)
        {
            this.Args.Character.SendSystemMessage(txt);
        }

        public override void ReplyFormat(string txt)
        {
            this.Args.Character.SendSystemMessage(txt);
        }
    }
}