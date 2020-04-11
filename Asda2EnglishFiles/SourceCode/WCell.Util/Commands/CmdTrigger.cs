using WCell.Util.Strings;

namespace WCell.Util.Commands
{
    /// <summary>
    /// CmdTriggers trigger Commands. There are different kinds of triggers which are handled differently,
    /// according to where they came from.
    /// 
    /// </summary>
    /// 
    ///             TODO: Have a reply-stream.
    public abstract class CmdTrigger<C> : ITriggerer where C : ICmdArgs
    {
        protected StringStream m_text;

        /// <summary>The alias that has been used to trigger this command.</summary>
        public string Alias;

        protected internal BaseCommand<C> cmd;
        protected internal BaseCommand<C> selectedCmd;
        public C Args;

        protected CmdTrigger()
        {
        }

        protected CmdTrigger(StringStream text, C args)
        {
            this.m_text = text;
            this.Args = args;
        }

        protected CmdTrigger(C args)
        {
            this.Args = args;
        }

        protected CmdTrigger(StringStream text, BaseCommand<C> selectedCmd, C args)
        {
            this.m_text = text;
            this.selectedCmd = selectedCmd;
            this.Args = args;
        }

        /// <summary>
        /// That command that has been triggered or null if the command for this <code>Alias</code> could
        /// not be found.
        /// </summary>
        public BaseCommand<C> Command
        {
            get { return this.cmd; }
        }

        /// <summary>
        /// That command that was selected when triggering this Trigger.
        /// </summary>
        public BaseCommand<C> SelectedCommand
        {
            get { return this.selectedCmd; }
            set { this.selectedCmd = value; }
        }

        /// <summary>
        /// A <code>StringStream</code> which contains the supplied arguments.
        /// </summary>
        public StringStream Text
        {
            get { return this.m_text; }
            set { this.m_text = value; }
        }

        /// <summary>Replies accordingly with the given text.</summary>
        public abstract void Reply(string text);

        /// <summary>Replies accordingly with the given formatted text.</summary>
        public abstract void ReplyFormat(string text);

        public void Reply(string format, params object[] args)
        {
            this.Reply(string.Format(format, args));
        }

        public void ReplyFormat(string format, params object[] args)
        {
            this.ReplyFormat(string.Format(format, args));
        }

        public T EvalNext<T>(T deflt)
        {
            object obj = this.cmd.mgr.EvalNext(this, (object) deflt);
            if (obj is T)
                return (T) obj;
            return default(T);
        }

        public NestedCmdTrigger<C> Nest(C args)
        {
            return new NestedCmdTrigger<C>(this, args);
        }

        public NestedCmdTrigger<C> Nest(string text)
        {
            return this.Nest(new StringStream(text));
        }

        public NestedCmdTrigger<C> Nest(StringStream text)
        {
            return new NestedCmdTrigger<C>(this, this.Args, text);
        }

        public SilentCmdTrigger<C> Silent(C args)
        {
            return new SilentCmdTrigger<C>(this.m_text, args);
        }
    }
}