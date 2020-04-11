using WCell.Util.Strings;

namespace WCell.Util.Commands
{
    public class NestedCmdTrigger<C> : CmdTrigger<C> where C : ICmdArgs
    {
        private readonly CmdTrigger<C> m_Trigger;

        public NestedCmdTrigger(CmdTrigger<C> trigger, C args)
            : this(trigger, args, trigger.Text)
        {
        }

        public NestedCmdTrigger(CmdTrigger<C> trigger, C args, StringStream text)
        {
            this.Args = args;
            this.m_text = text;
            this.selectedCmd = trigger.selectedCmd;
            this.m_Trigger = trigger;
        }

        public CmdTrigger<C> Trigger
        {
            get { return this.m_Trigger; }
        }

        public override void Reply(string text)
        {
            this.m_Trigger.Reply(text);
        }

        public override void ReplyFormat(string text)
        {
            this.m_Trigger.ReplyFormat(text);
        }
    }
}