using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.Strings;

namespace WCell.RealmServer.Commands
{
    public class BufferedCommandTrigger : DefaultCmdTrigger
    {
        public readonly BufferedCommandResponse Response = new BufferedCommandResponse();

        public BufferedCommandTrigger()
        {
        }

        public BufferedCommandTrigger(string text)
            : base(text)
        {
        }

        public BufferedCommandTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand,
            RealmServerCmdArgs args)
            : base(text, selectedCommand, args)
        {
        }

        public BufferedCommandTrigger(StringStream args, BaseCommand<RealmServerCmdArgs> selectedCommand)
            : base(args, selectedCommand)
        {
        }

        public BufferedCommandTrigger(StringStream args)
            : base(args)
        {
        }

        public BufferedCommandTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand)
            : base(text, selectedCommand)
        {
        }

        public override void Reply(string text)
        {
            this.Response.Replies.Add(text);
        }

        public override void ReplyFormat(string text)
        {
            this.Response.Replies.Add(text);
        }
    }
}