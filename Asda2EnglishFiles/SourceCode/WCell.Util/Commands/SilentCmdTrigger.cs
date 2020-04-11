using WCell.Util.Strings;

namespace WCell.Util.Commands
{
    public class SilentCmdTrigger<C> : CmdTrigger<C> where C : ICmdArgs
    {
        public SilentCmdTrigger(StringStream text, C args)
        {
            this.m_text = text;
            this.Args = args;
        }

        public override void Reply(string text)
        {
        }

        public override void ReplyFormat(string text)
        {
        }
    }
}