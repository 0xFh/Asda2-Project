using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Util.Strings;

namespace WCell.Util.Commands
{
    /// <summary>
    /// An abstract base class, for Command and SubCommand.
    /// Can hold SubCommands which again can hold further SubCommands.
    /// </summary>
    public abstract class BaseCommand<C> where C : ICmdArgs
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected IDictionary<string, BaseCommand<C>.SubCommand> m_subCommands =
            (IDictionary<string, BaseCommand<C>.SubCommand>) new Dictionary<string, BaseCommand<C>.SubCommand>(
                (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        protected HashSet<BaseCommand<C>.SubCommand> m_subCommandSet = new HashSet<BaseCommand<C>.SubCommand>();

        /// <summary>
        /// All Aliases which can trigger the Process method of this Command.
        /// </summary>
        public string[] Aliases;

        protected string m_englishParamInfo;
        protected string m_EnglishDescription;
        protected bool m_enabled;
        protected internal BaseCommand<C> m_parentCmd;
        protected internal CommandMgr<C> mgr;

        /// <summary>
        /// The actual Command itself to which this SubCommand (and maybe its ancestors) belongs
        /// </summary>
        public Command<C> RootCmd
        {
            get
            {
                BaseCommand<C> baseCommand = this;
                while (baseCommand is BaseCommand<C>.SubCommand)
                    baseCommand = baseCommand.m_parentCmd;
                return (Command<C>) baseCommand;
            }
        }

        /// <summary>
        /// The parent of this SubCommand (can be a further SubCommand or a Command)
        /// </summary>
        public BaseCommand<C> ParentCmd
        {
            get { return this.m_parentCmd; }
        }

        public IDictionary<string, BaseCommand<C>.SubCommand> SubCommandsByAlias
        {
            get { return this.m_subCommands; }
        }

        public HashSet<BaseCommand<C>.SubCommand> SubCommands
        {
            get { return this.m_subCommandSet; }
        }

        /// <summary>
        /// Indicates whether or not this command is enabled.
        /// If false, Commands.ReactTo will not trigger this Command'str Process method.
        /// Alternatively you can Add/Remove this Command to/from Commands.CommandsByAlias to control whether or not
        /// certain Commands should or should not be used.
        /// </summary>
        public bool Enabled
        {
            get { return this.m_enabled; }
            set { this.m_enabled = value; }
        }

        public virtual string Name
        {
            get
            {
                string str = this.GetType().Name;
                int length = str.IndexOf("Command");
                if (length >= 0)
                    str = str.Substring(0, length);
                return str;
            }
        }

        /// <summary>A human-readable list of expected parameters</summary>
        public string EnglishParamInfo
        {
            get { return this.m_englishParamInfo; }
            set { this.m_englishParamInfo = value; }
        }

        /// <summary>Describes the command itself.</summary>
        public string EnglishDescription
        {
            get { return this.m_EnglishDescription; }
            set { this.m_EnglishDescription = value; }
        }

        internal void DoInit()
        {
            this.Initialize();
            if (this.Aliases.Length == 0)
                throw new Exception("Command has no Aliases: " + (object) this);
            foreach (string aliase in this.Aliases)
            {
                if (aliase.Contains(" "))
                    throw new Exception("Command-Alias \"" + aliase + "\" must not contain spaces in " + (object) this);
                if (aliase.Length == 0)
                    throw new Exception("Command has empty Alias: " + (object) this);
            }
        }

        protected abstract void Initialize();

        protected void Init(params string[] aliases)
        {
            this.m_enabled = true;
            this.m_englishParamInfo = "";
            this.Aliases = aliases;
            this.AddSubCmds();
        }

        public virtual string GetDescription(CmdTrigger<C> trigger)
        {
            return this.EnglishDescription;
        }

        public virtual string GetParamInfo(CmdTrigger<C> trigger)
        {
            return this.m_englishParamInfo;
        }

        public string CreateInfo(CmdTrigger<C> trigger)
        {
            return this.CreateUsage(trigger) + " (" + this.GetDescription(trigger) + ")";
        }

        /// <summary>Returns a simple usage string</summary>
        public string CreateUsage()
        {
            return this.CreateUsage(this.GetParamInfo((CmdTrigger<C>) null));
        }

        public string CreateUsage(CmdTrigger<C> trigger)
        {
            return this.CreateUsage(this.GetParamInfo(trigger));
        }

        public virtual string CreateUsage(string paramInfo)
        {
            paramInfo = ((IEnumerable<string>) this.Aliases).ToString<string>("|") + " " + paramInfo;
            if (this.m_parentCmd != null)
                paramInfo = this.m_parentCmd.CreateUsage(paramInfo);
            return paramInfo;
        }

        /// <summary>
        /// Is called when the command is triggered (case-insensitive).
        /// </summary>
        public abstract void Process(CmdTrigger<C> trigger);

        /// <summary>Processes a command that yields an object to return</summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public virtual object Eval(CmdTrigger<C> trigger)
        {
            return (object) null;
        }

        protected void TriggerSubCommand(CmdTrigger<C> trigger)
        {
            string key = trigger.Text.NextWord();
            BaseCommand<C>.SubCommand subCommand;
            if (this.m_subCommands.TryGetValue(key, out subCommand))
            {
                if (!this.RootCmd.MayTrigger(trigger, (BaseCommand<C>) subCommand, false))
                    return;
                subCommand.Process(trigger);
            }
            else
            {
                trigger.Reply("SubCommand not found: " + key);
                trigger.Text.Skip(trigger.Text.Length);
                this.mgr.DisplayCmd(trigger, this, false, false);
            }
        }

        protected internal void AddSubCmds()
        {
            Type[] nestedTypes = this.GetType().GetNestedTypes(BindingFlags.Public);
            if (nestedTypes.Length <= 0)
                return;
            foreach (Type type in nestedTypes)
            {
                if (type.IsSubclassOf(typeof(BaseCommand<C>.SubCommand)) && !type.IsAbstract)
                {
                    ConstructorInfo constructor = type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null,
                        new Type[0], new ParameterModifier[0]);
                    if (constructor == (ConstructorInfo) null)
                        throw new ArgumentException(type.FullName + " lacks parameterless constructor.");
                    this.AddSubCmd(constructor.Invoke((object[]) null) as BaseCommand<C>.SubCommand);
                }
            }
        }

        public void AddSubCmd(BaseCommand<C>.SubCommand cmd)
        {
            cmd.m_parentCmd = this;
            cmd.Initialize();
            foreach (string aliase in cmd.Aliases)
            {
                this.m_subCommands[aliase] = cmd;
                this.m_subCommandSet.Add(cmd);
            }
        }

        /// <summary>
        /// Creates a default string of all aliases and all subcommands
        /// </summary>
        protected virtual string CreateGroupUsageString()
        {
            return ((IEnumerable<string>) this.Aliases).ToString<string>("|") + " " +
                   this.m_subCommands.Keys.ToString<string>("|");
        }

        /// <summary>Returns the direct SubCommand with the given alias.</summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public BaseCommand<C>.SubCommand SelectSubCommand(string alias)
        {
            BaseCommand<C>.SubCommand subCommand;
            this.m_subCommands.TryGetValue(alias, out subCommand);
            return subCommand;
        }

        /// <summary>
        /// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
        /// </summary>
        public BaseCommand<C>.SubCommand SelectSubCommand(StringStream cmdString)
        {
            BaseCommand<C>.SubCommand subCommand = this.SelectSubCommand(cmdString.NextWord());
            if (subCommand != null && subCommand.SubCommands.Count > 0 && cmdString.HasNext)
                return subCommand.SelectSubCommand(cmdString);
            return subCommand;
        }

        /// <summary>
        /// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
        /// </summary>
        public void GetSubCommands(StringStream cmdString, List<BaseCommand<C>> list)
        {
            string str = cmdString.NextWord();
            IEnumerable<BaseCommand<C>.SubCommand> source = this.m_subCommandSet.Where<BaseCommand<C>.SubCommand>(
                (Func<BaseCommand<C>.SubCommand, bool>) (comd =>
                    ((IEnumerable<string>) comd.Aliases).Where<string>((Func<string, bool>) (alias =>
                        alias.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > -1)).Count<string>() > 0));
            if (cmdString.HasNext && source.Count<BaseCommand<C>.SubCommand>() == 1)
            {
                source.First<BaseCommand<C>.SubCommand>().GetSubCommands(cmdString, list);
            }
            else
            {
                foreach (BaseCommand<C>.SubCommand subCommand in source)
                    list.Add((BaseCommand<C>) subCommand);
            }
        }

        public override string ToString()
        {
            return this.Name + "-Command";
        }

        protected internal void FailNotify(CmdTrigger<C> trigger, Exception ex)
        {
            BaseCommand<C>.log.Warn((object) ex);
            this.OnFail(trigger, ex);
        }

        /// <summary>Is triggered when the processing throws an Exception.</summary>
        protected virtual void OnFail(CmdTrigger<C> trigger, Exception ex)
        {
            trigger.Reply("Command failed: ");
            foreach (string allMessage in ex.GetAllMessages())
                trigger.Reply(allMessage);
        }

        public abstract class SubCommand : BaseCommand<C>
        {
        }
    }
}