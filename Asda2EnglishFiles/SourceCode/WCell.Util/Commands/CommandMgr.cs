using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WCell.Util.DynamicAccess;
using WCell.Util.Strings;
using WCell.Util.Toolshed;

namespace WCell.Util.Commands
{
    /// <summary>Command provider class</summary>
    public abstract class CommandMgr<C> where C : ICmdArgs
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>Includes Subcommands</summary>
        internal readonly IDictionary<long, BaseCommand<C>> allCommandsByType =
            (IDictionary<long, BaseCommand<C>>) new Dictionary<long, BaseCommand<C>>();

        public const char VariableCharacter = '$';

        /// <summary>
        /// Validates whether a the given command may be triggered (checks for privileges etc)
        /// </summary>
        public CommandMgr<C>.TriggerValidationHandler TriggerValidator;

        private readonly IDictionary<string, Command<C>> commandsByAlias;
        private readonly IDictionary<string, Command<C>> commandsByName;

        /// <summary>
        /// Is triggered whenever an unknown command has been used
        /// </summary>
        public event CommandMgr<C>.UnknownCommandHandler UnknownCommand;

        public CommandMgr()
        {
            this.commandsByAlias =
                (IDictionary<string, Command<C>>) new Dictionary<string, Command<C>>(
                    (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
            this.commandsByName =
                (IDictionary<string, Command<C>>) new Dictionary<string, Command<C>>(
                    (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
            this.Add((Command<C>) new CommandMgr<C>.HelpCommand(this));
            this.Add((Command<C>) new CommandMgr<C>.ExecFileCommand(this));
        }

        public abstract string ExecFileDir { get; }

        /// <summary>
        /// Executes a specific Command with parameters.
        /// 
        /// Interprets the first word as alias, takes all enabled Commands with the specific alias out of the
        /// CommandsByAlias-map and triggers the specific Process() method on all of them.
        /// If the processing of the command raises an Exception, the fail events are triggered.
        /// </summary>
        /// <returns>True if at least one Command was triggered, otherwise false.</returns>
        public virtual bool Execute(CmdTrigger<C> trigger)
        {
            BaseCommand<C> command = this.GetCommand(trigger);
            if (command != null)
                return this.Trigger(trigger, command);
            return false;
        }

        public bool TriggerAll(CmdTrigger<C> trigger, List<Command<C>> commands)
        {
            foreach (Command<C> command in commands)
            {
                if (!this.Trigger(trigger, (BaseCommand<C>) command))
                    return false;
            }

            return true;
        }

        /// <summary>Call Eval on specified command</summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public virtual object EvalNext(CmdTrigger<C> trigger, object deflt)
        {
            if (!trigger.Text.ConsumeNext('('))
                return deflt;
            string text = trigger.Text.NextWord(")");
            if (!trigger.Text.HasNext && !trigger.Text.String.EndsWith(")"))
                return (object) null;
            NestedCmdTrigger<C> nestedCmdTrigger = trigger.Nest(text);
            BaseCommand<C> command = this.GetCommand((CmdTrigger<C>) nestedCmdTrigger);
            if (command != null)
                return this.Eval((CmdTrigger<C>) nestedCmdTrigger, command);
            return (object) false;
        }

        public virtual BaseCommand<C> GetCommand(CmdTrigger<C> trigger)
        {
            int position = trigger.Text.Position;
            BaseCommand<C> baseCommand;
            string alias;
            if (trigger.selectedCmd != null &&
                (BaseCommand<C>.SubCommand) (baseCommand =
                    (BaseCommand<C>) trigger.selectedCmd.SelectSubCommand(trigger.Text)) != null)
            {
                alias = trigger.selectedCmd.Aliases[0];
            }
            else
            {
                trigger.Text.Position = position;
                alias = trigger.Text.NextWord();
                baseCommand = (BaseCommand<C>) this.Get(alias);
            }

            trigger.Alias = alias;
            if (baseCommand != null)
                return baseCommand;
            CommandMgr<C>.UnknownCommandHandler unknownCommand = this.UnknownCommand;
            if (unknownCommand != null)
                unknownCommand(trigger);
            return (BaseCommand<C>) null;
        }

        public bool Trigger(CmdTrigger<C> trigger, BaseCommand<C> cmd)
        {
            return this.Execute(trigger, cmd, false);
        }

        /// <summary>Lets the given CmdTrigger trigger the given Command.</summary>
        /// <param name="trigger"></param>
        /// <param name="cmd"></param>
        /// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
        /// <returns></returns>
        public virtual bool Execute(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silentFail)
        {
            if (cmd.Enabled)
            {
                Command<C> rootCmd = cmd.RootCmd;
                if (!rootCmd.MayTrigger(trigger, cmd, silentFail) || !this.TriggerValidator(trigger, cmd, silentFail))
                    return false;
                trigger.cmd = cmd;
                try
                {
                    cmd.Process(trigger);
                    rootCmd.ExecutedNotify(trigger);
                }
                catch (Exception ex)
                {
                    rootCmd.FailNotify(trigger, ex);
                }

                return true;
            }

            trigger.Reply("Command is disabled: " + (object) cmd);
            return false;
        }

        /// <summary>Lets the given CmdTrigger trigger the given Command.</summary>
        /// <param name="trigger"></param>
        /// <param name="cmd"></param>
        /// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
        /// <returns></returns>
        public virtual object Eval(CmdTrigger<C> trigger, BaseCommand<C> cmd)
        {
            return this.Eval(trigger, cmd, false);
        }

        /// <summary>Lets the given CmdTrigger trigger the given Command.</summary>
        /// <param name="trigger"></param>
        /// <param name="cmd"></param>
        /// <param name="silentFail">Will not reply if it failed due to target restrictions or privileges etc</param>
        /// <returns></returns>
        public virtual object Eval(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silentFail)
        {
            if (cmd.Enabled)
            {
                Command<C> rootCmd = cmd.RootCmd;
                if (!rootCmd.MayTrigger(trigger, cmd, silentFail) || !this.TriggerValidator(trigger, cmd, silentFail))
                    return (object) false;
                trigger.cmd = cmd;
                try
                {
                    object obj = cmd.Eval(trigger);
                    rootCmd.ExecutedNotify(trigger);
                    return obj;
                }
                catch (Exception ex)
                {
                    rootCmd.FailNotify(trigger, ex);
                }

                return (object) true;
            }

            trigger.Reply("Command is disabled: " + (object) cmd);
            return (object) false;
        }

        /// <summary>Returns the Command with the given Name.</summary>
        public Command<C> this[string name]
        {
            get
            {
                Command<C> command;
                this.commandsByName.TryGetValue(name, out command);
                return command;
            }
        }

        /// <summary>
        /// The Table of all Commands which exists for the use of the ReactTo() method
        /// (Filled by the Initialize() method).
        /// The keys are all possible aliases of all commands and the values are ArrayLists of Commands
        /// which are associated with the specific alias.
        /// The aliases are stored case-insensitively.
        /// Use the Remove(Command) and Add(Command) methods to manipulate this CommandsByAlias.
        /// </summary>
        public IDictionary<string, Command<C>> CommandsByAlias
        {
            get { return this.commandsByAlias; }
        }

        public ICollection<Command<C>> Commands
        {
            get { return this.commandsByName.Values; }
        }

        /// <summary>Adds a Command to the CommandsByAlias.</summary>
        public void Add(Command<C> cmd)
        {
            cmd.DoInit();
            this.commandsByName.Add(cmd.Name, cmd);
            foreach (string aliase in cmd.Aliases)
            {
                if (aliase.Contains<char>('$'))
                {
                    CommandMgr<C>.log.Error("Command alias \"{0}\" contained invalid character (Command: {1})",
                        (object) aliase, (object) cmd);
                }
                else
                {
                    if (this.commandsByAlias.ContainsKey(aliase))
                        CommandMgr<C>.log.Warn(
                            "Overriding alias \"{0}\" because it was used by more than 1 Command: \"{1}\" and \"{2}\"",
                            (object) aliase, (object) this.commandsByAlias[aliase], (object) cmd);
                    this.commandsByAlias[aliase] = cmd;
                }
            }

            this.allCommandsByType.Add(cmd.GetType().TypeHandle.Value.ToInt64(), (BaseCommand<C>) cmd);
            cmd.mgr = this;
            foreach (BaseCommand<C>.SubCommand subCommand in cmd.SubCommands)
            {
                long int64 = subCommand.GetType().TypeHandle.Value.ToInt64();
                if (this.allCommandsByType.ContainsKey(int64))
                    throw new InvalidOperationException("Tried to add 2 Commands of the same Type: " +
                                                        (object) subCommand.GetType());
                this.allCommandsByType.Add(int64, (BaseCommand<C>) subCommand);
                subCommand.mgr = this;
            }
        }

        /// <summary>
        /// Adds a Command-instance of the specific type, if it is a Command-type
        /// </summary>
        public void Add(Type cmdType)
        {
            if (!cmdType.IsSubclassOf(typeof(Command<C>)) || cmdType.IsAbstract)
                return;
            ConstructorInfo constructor = cmdType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, new Type[0],
                new ParameterModifier[0]);
            if (constructor == (ConstructorInfo) null)
                throw new ArgumentException(cmdType.FullName + " lacks parameterless constructor.");
            this.Add((Command<C>) constructor.Invoke(new object[0]));
        }

        /// <summary>Finds and adds all Commands of the given Assembly</summary>
        public void AddCmdsOfAsm(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
                this.Add(type);
        }

        /// <summary>Removes a Command.</summary>
        public void Remove(Command<C> cmd)
        {
            this.commandsByName.Remove(cmd.Name);
            foreach (string aliase in cmd.Aliases)
                this.commandsByAlias.Remove(aliase);
            IDictionary<long, BaseCommand<C>> allCommandsByType1 = this.allCommandsByType;
            RuntimeTypeHandle typeHandle = cmd.GetType().TypeHandle;
            long int64_1 = typeHandle.Value.ToInt64();
            allCommandsByType1.Remove(int64_1);
            foreach (BaseCommand<C>.SubCommand subCommand in cmd.SubCommands)
            {
                IDictionary<long, BaseCommand<C>> allCommandsByType2 = this.allCommandsByType;
                typeHandle = subCommand.GetType().TypeHandle;
                long int64_2 = typeHandle.Value.ToInt64();
                allCommandsByType2.Remove(int64_2);
            }
        }

        /// <summary>Returns all Commands with the given Alias</summary>
        public Command<C> Get(string alias)
        {
            Command<C> command;
            this.commandsByAlias.TryGetValue(alias, out command);
            return command;
        }

        public T Get<T>() where T : BaseCommand<C>
        {
            BaseCommand<C> baseCommand;
            this.allCommandsByType.TryGetValue(typeof(T).TypeHandle.Value.ToInt64(), out baseCommand);
            return (T) baseCommand;
        }

        public List<BaseCommand<C>> GetCommands(string cmdString)
        {
            List<BaseCommand<C>> list = new List<BaseCommand<C>>();
            this.GetCommands(new StringStream(cmdString), list);
            return list;
        }

        public void GetCommands(StringStream cmdString, List<BaseCommand<C>> list)
        {
            string str = cmdString.NextWord();
            IEnumerable<Command<C>> source = this.commandsByName.Values.Where<Command<C>>(
                (Func<Command<C>, bool>) (comd =>
                    ((IEnumerable<string>) comd.Aliases).Where<string>((Func<string, bool>) (alias =>
                        alias.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > -1)).Count<string>() > 0));
            if (cmdString.HasNext && source.Count<Command<C>>() == 1)
            {
                source.First<Command<C>>().GetSubCommands(cmdString, list);
            }
            else
            {
                foreach (Command<C> command in source)
                    list.Add((BaseCommand<C>) command);
            }
        }

        public bool MayDisplay(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool ignoreRestrictions)
        {
            return cmd.Enabled && (ignoreRestrictions ||
                                   cmd.RootCmd.MayTrigger(trigger, cmd, true) &&
                                   this.TriggerValidator(trigger, cmd, true));
        }

        public BaseCommand<C> SelectCommand(string cmdString)
        {
            return this.SelectCommand(new StringStream(cmdString));
        }

        public BaseCommand<C> SelectCommand(StringStream cmdString)
        {
            Command<C> command = this.Get(cmdString.NextWord());
            if (command != null && cmdString.HasNext)
                return (BaseCommand<C>) command.SelectSubCommand(cmdString);
            return (BaseCommand<C>) command;
        }

        /// <summary>
        /// Removes all Commands of the specific Type from the CommandsByAlias.
        /// </summary>
        /// <returns>True if any commands have been removed, otherwise false.</returns>
        public void AddDefaultCallCommand(ToolMgr mgr)
        {
            this.Add((Command<C>) new CommandMgr<C>.CallCommand(mgr));
        }

        /// <summary>
        /// Gives help
        /// TODO: Localization
        /// </summary>
        public void TriggerHelp(CmdTrigger<C> trigger)
        {
            this.TriggerHelp(trigger, false);
        }

        /// <summary>
        /// Gives help
        /// TODO: Localization
        /// </summary>
        public void TriggerHelp(CmdTrigger<C> trigger, bool ignoreRestrictions)
        {
            if (trigger.Text.HasNext)
            {
                string remainder = trigger.Text.Remainder;
                List<BaseCommand<C>> commands = this.GetCommands(remainder);
                int count = commands.Count;
                foreach (BaseCommand<C> cmd in commands)
                {
                    if (this.MayDisplay(trigger, cmd, ignoreRestrictions))
                        this.DisplayCmd(trigger, cmd, ignoreRestrictions, true);
                    else
                        --count;
                }

                if (count != 0)
                    return;
                trigger.ReplyFormat("Did not find any Command that matches '{0}'.", (object) remainder);
            }
            else
            {
                int num = 0;
                foreach (Command<C> command in (IEnumerable<Command<C>>) this.Commands)
                {
                    if (this.MayDisplay(trigger, (BaseCommand<C>) command, ignoreRestrictions))
                        ++num;
                }

                trigger.ReplyFormat("Use: ? <Alias> [<subalias> [<subalias> ...]] for help on a certain command.");
                trigger.Reply("All {0} available commands:", (object) num);
                foreach (Command<C> command in (IEnumerable<Command<C>>) this.Commands)
                {
                    if (this.MayDisplay(trigger, (BaseCommand<C>) command, ignoreRestrictions))
                        trigger.Reply(command.CreateUsage(trigger));
                }
            }
        }

        public void DisplayCmd(CmdTrigger<C> trigger, BaseCommand<C> cmd)
        {
            this.DisplayCmd(trigger, cmd, false, true);
        }

        public void DisplayCmd(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool ignoreRestrictions, bool detail)
        {
            trigger.Reply(string.Format("{0}{1}", (object) cmd.CreateUsage(trigger),
                detail ? (object) (" (" + cmd.GetDescription(trigger) + ")") : (object) ""));
            if (cmd.SubCommands.Count <= 0)
                return;
            trigger.Reply("All SubCommands:");
            foreach (BaseCommand<C>.SubCommand subCommand in cmd.SubCommands)
            {
                if (this.MayDisplay(trigger, (BaseCommand<C>) subCommand, ignoreRestrictions))
                    this.DisplayCmd(trigger, (BaseCommand<C>) subCommand, ignoreRestrictions, detail);
            }
        }

        public void ExecFile(string filename, C args)
        {
            CommandMgr<C>.ConsoleCmdTrigger consoleCmdTrigger = new CommandMgr<C>.ConsoleCmdTrigger(args);
            this.ExecFile(filename, (CmdTrigger<C>) consoleCmdTrigger, (Func<CmdTrigger<C>, int, bool>) null);
        }

        public void ExecFile(string filename, C args, Func<CmdTrigger<C>, int, bool> cmdValidator)
        {
            CommandMgr<C>.ConsoleCmdTrigger consoleCmdTrigger = new CommandMgr<C>.ConsoleCmdTrigger(args);
            this.ExecFile(filename, (CmdTrigger<C>) consoleCmdTrigger, cmdValidator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="trigger"></param>
        public virtual void ExecFile(string filename, CmdTrigger<C> trigger)
        {
            this.ExecFile(filename, trigger, (Func<CmdTrigger<C>, int, bool>) null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="trigger"></param>
        /// <param name="cmdValidator">Validates whether the given trigger may execute. Second parameter is line no.</param>
        public void ExecFile(string filename, CmdTrigger<C> trigger, Func<CmdTrigger<C>, int, bool> cmdValidator)
        {
            int num = 0;
            using (StreamReader streamReader = new StreamReader(filename))
            {
                string str;
                while ((str = streamReader.ReadLine()) != null)
                {
                    ++num;
                    string s = str.Trim();
                    if (!s.StartsWith("#") && s.Length > 0)
                    {
                        StringStream stringStream = new StringStream(s);
                        trigger.Text = stringStream;
                        if ((cmdValidator == null || cmdValidator(trigger, num)) && !this.Execute(trigger))
                            trigger.Reply("Could not execute Command from file \"{0}\" (line {1}): \"{2}\"",
                                (object) filename, (object) num, (object) s);
                    }
                }
            }
        }

        /// <summary>Validates whether a command may be triggered.</summary>
        /// <param name="trigger"></param>
        /// <param name="cmd"></param>
        /// <param name="silent">Whether there should be no output during this check</param>
        /// <returns>Whether the given command may be triggered by the given trigger</returns>
        public delegate bool TriggerValidationHandler(CmdTrigger<C> trigger, BaseCommand<C> cmd, bool silent);

        public delegate void UnknownCommandHandler(CmdTrigger<C> trigger);

        /// <summary>Default trigger for Console-interaction</summary>
        public class ConsoleCmdTrigger : CmdTrigger<C>
        {
            public ConsoleCmdTrigger(string text, C args)
                : this(new StringStream(text), args)
            {
            }

            public ConsoleCmdTrigger(StringStream text, C args)
                : base(text, args)
            {
            }

            public ConsoleCmdTrigger(C args)
                : base((StringStream) null, args)
            {
            }

            public override void Reply(string text)
            {
                Console.WriteLine(text);
            }

            public override void ReplyFormat(string text)
            {
                Console.WriteLine(text);
            }
        }

        public class CallCommand : Command<C>
        {
            public readonly ToolMgr ToolMgr;

            public CallCommand(ToolMgr toolMgr)
            {
                this.ToolMgr = toolMgr;
            }

            protected override void Initialize()
            {
                this.Init("Call", nameof(C), "@");
                this.EnglishParamInfo = "(-l [<wildmatch>]|-<i>)|<methodname>[ <arg0> [<arg1> [...]]]";
                this.EnglishDescription =
                    "Calls any static method or custom function with the given arguments. Either use the name or the index of the function.";
            }

            public override void Process(CmdTrigger<C> trigger)
            {
                StringStream text = trigger.Text;
                if (!text.HasNext)
                {
                    trigger.Reply("Invalid arguments - " + this.CreateInfo(trigger));
                }
                else
                {
                    IExecutable executable1;
                    if (text.ConsumeNext('-'))
                    {
                        if (!text.HasNext)
                        {
                            trigger.Reply("Invalid arguments - " + this.CreateInfo(trigger));
                            return;
                        }

                        if (char.ToLower(text.Remainder[0]) == 'l')
                        {
                            ++text.Position;
                            string[] strArray = text.Remainder.Split(new char[1]
                            {
                                ' '
                            }, StringSplitOptions.RemoveEmptyEntries);
                            ToolMgr toolMgr = ((CommandMgr<C>.CallCommand) this.RootCmd).ToolMgr;
                            trigger.Reply("Callable functions ({0}):", (object) toolMgr.Executables.Count);
                            for (int index = 0; index < toolMgr.ExecutableList.Count; ++index)
                            {
                                IExecutable executable2 = toolMgr.ExecutableList[index];
                                if (strArray.Length != 0)
                                {
                                    bool flag = false;
                                    foreach (string str in strArray)
                                    {
                                        if (executable2.Name.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) >
                                            -1)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }

                                    if (!flag)
                                        continue;
                                }

                                trigger.Reply(" {0}: {1}", (object) index, (object) executable2);
                            }

                            return;
                        }

                        uint num = text.NextUInt(uint.MaxValue);
                        executable1 = (long) num >= (long) this.ToolMgr.ExecutableList.Count
                            ? (IExecutable) null
                            : this.ToolMgr.ExecutableList[(int) num];
                    }
                    else
                        executable1 = this.ToolMgr.Get(text.NextWord());

                    if (executable1 == null)
                    {
                        trigger.Reply("Could not find specified Executable.");
                    }
                    else
                    {
                        int length = executable1.ParameterTypes.Length;
                        object[] objArray = new object[length];
                        object obj = (object) null;
                        for (int index = 0; index < length; ++index)
                        {
                            Type parameterType = executable1.ParameterTypes[index];
                            StringParser.Parse(index == length - 1 ? text.Remainder : text.NextWord(), parameterType,
                                ref obj);
                            objArray[index] = obj;
                        }

                        executable1.Exec(objArray);
                    }
                }
            }
        }

        /// <summary>
        /// TODO: Use localized strings
        /// The help command is special since it generates output.
        /// This output needs to be shown in the GUI if used from commandline and
        /// sent to the requester if executed remotely.
        /// </summary>
        public class HelpCommand : Command<C>
        {
            private readonly CommandMgr<C> m_Mgr;

            public HelpCommand(CommandMgr<C> mgr)
            {
                this.m_Mgr = mgr;
            }

            public CommandMgr<C> Mgr
            {
                get { return this.m_Mgr; }
            }

            protected override void Initialize()
            {
                this.Init("Help", "?");
                this.EnglishParamInfo = "[<part of cmd> [[<part of subcmd>] <part of subcmd> ...]]";
                this.EnglishDescription =
                    "Shows an overview over all Commands or -if specified- the help for a specific Command (and its subcommands).";
            }

            public override void Process(CmdTrigger<C> trigger)
            {
                this.m_Mgr.TriggerHelp(trigger, false);
            }
        }

        /// <summary>
        /// The help command is special since it generates output.
        /// This output needs to be shown in the GUI if used from commandline and
        /// sent to the requester if executed remotely.
        /// </summary>
        public class ExecFileCommand : Command<C>
        {
            private readonly CommandMgr<C> m_Mgr;

            public ExecFileCommand(CommandMgr<C> mgr)
            {
                this.m_Mgr = mgr;
            }

            public CommandMgr<C> Mgr
            {
                get { return this.m_Mgr; }
            }

            protected override void Initialize()
            {
                this.Init("ExecFile");
                this.EnglishParamInfo = "<filename>";
                this.EnglishDescription = "Executes the given file.";
            }

            public override void Process(CmdTrigger<C> trigger)
            {
                if (!trigger.Text.HasNext)
                {
                    trigger.Reply("No file was specified.");
                }
                else
                {
                    string str = trigger.Text.NextWord();
                    if (!Path.IsPathRooted(str))
                        str = Path.Combine(this.m_Mgr.ExecFileDir, str);
                    if (File.Exists(str))
                        this.m_Mgr.ExecFile(str, trigger);
                    else
                        trigger.Reply("File to execute does not exist: " + str);
                }
            }
        }
    }
}