using NLog;

namespace WCell.Util.Commands
{
    /// <summary>
    /// Basic Command Class, Inherit your Commands from here. Automatically creates one instance
    /// per IrcClient when the Class is loaded, using the default constructor.
    /// </summary>
    public abstract class Command<C> : BaseCommand<C> where C : ICmdArgs
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public event Command<C>.CommandCallback Executed;

        internal void ExecutedNotify(CmdTrigger<C> trigger)
        {
            if (this.Executed == null)
                return;
            this.Executed(trigger);
        }

        /// <summary>
        /// Determines whether the given command may ever be used in this Context, depending
        /// on the trigger's parameters that the triggerer cannot currently change and
        /// are not already checked globally by the TriggerValidator.
        /// </summary>
        public virtual bool MayTrigger(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silent)
        {
            return true;
        }

        public override void Process(CmdTrigger<C> trigger)
        {
            if (this.m_subCommands == null)
                return;
            this.TriggerSubCommand(trigger);
        }

        public delegate void CommandCallback(CmdTrigger<C> trigger);
    }
}