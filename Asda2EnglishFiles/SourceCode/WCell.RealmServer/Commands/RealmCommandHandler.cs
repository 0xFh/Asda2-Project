using System;
using System.IO;
using System.Linq;
using System.Threading;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Strings;
using WCell.Util.Variables;

namespace WCell.RealmServer.Commands
{
    public class RealmCommandHandler : CommandMgr<RealmServerCmdArgs>
    {
        public static readonly RealmCommandHandler Instance = new RealmCommandHandler();

        /// <summary>
        /// A directory containing a list of autoexec files, containing autoexec files similar to this:
        /// charname.txt
        /// Every file will be executed when the Character with the given name logs in.
        /// </summary>
        [Variable("AutoExecDir")] public static string AutoExecDir = "../RealmServerAutoExec/";

        /// <summary>
        /// The file (within the Content dir) containing a set of commands to be executed upon startup
        /// </summary>
        [Variable("AutoExecStartupFile")] public static string AutoExecStartupFile = "_Startup.txt";

        /// <summary>
        /// The file (within the Content dir) containing a set of commands to be executed on everyone on login
        /// </summary>
        [Variable("AutoExecAllCharsFile")] public static string AutoExecAllCharsFile = "AllChars.txt";

        /// <summary>
        /// The file (within the Content dir) containing a set of commands to be executed on everyone's first login
        /// </summary>
        [Variable("AutoExecAllCharsFirstLoginFile")]
        public static string AutoExecAllCharsFirstLoginFile = "AllCharsFirstLogin.txt";

        /// <summary>
        /// Sets the default command-prefixes that trigger commands when using chat
        /// </summary>
        public static string CommandPrefixes = ".#![";

        /// <summary>Used for dynamic method calls</summary>
        public static char ExecCommandPrefix = '@';

        /// <summary>Used for selecting commands</summary>
        public static char SelectCommandPrefix = ':';

        public override string ExecFileDir
        {
            get { return Path.GetDirectoryName(ServerApp<WCell.RealmServer.RealmServer>.EntryLocation); }
        }

        /// <summary>
        /// Clears the CommandsByAlias, invokes an instance of every Class that is inherited from Command and adds it
        /// to the CommandsByAlias and the List.
        /// Is automatically called when an instance of IrcClient is created in order to find all Commands.
        /// </summary>
        [WCell.Core.Initialization.Initialization(InitializationPass.Fourth, "Initialize Commands")]
        public static void Initialize()
        {
            if (RealmCommandHandler.Instance.TriggerValidator != null)
                return;
            char ch;
            if (RealmCommandHandler.CommandPrefixes.Contains<char>(ch = RealmCommandHandler.ExecCommandPrefix) ||
                RealmCommandHandler.CommandPrefixes.Contains<char>(ch = RealmCommandHandler.SelectCommandPrefix))
                throw new ArgumentException("Invalid Command-prefix may not be used as Command-prefix: " + (object) ch);
            RealmCommandHandler.Instance.TriggerValidator =
                (CommandMgr<RealmServerCmdArgs>.TriggerValidationHandler) ((trigger, cmd, silent) =>
                {
                    Command<RealmServerCmdArgs> rootCmd = cmd.RootCmd;
                    if (rootCmd is CommandMgr<RealmServerCmdArgs>.HelpCommand)
                        return true;
                    if (!trigger.Args.Role.MayUse(rootCmd))
                    {
                        if (!silent)
                            trigger.Reply(RealmLangKey.MustNotUseCommand, (object) cmd.Name);
                        return false;
                    }

                    if (rootCmd is RealmServerCommand && !trigger.Args.CheckArgs(rootCmd))
                    {
                        if (!silent)
                            RealmCommandHandler.OnInvalidArguments(trigger, (RealmServerCommand) rootCmd);
                        return false;
                    }

                    if (!(trigger.Args.Target is Character) ||
                        !(((Character) trigger.Args.Target).Account.Role > trigger.Args.Role))
                        return true;
                    if (!silent)
                        trigger.Reply("Insufficient privileges.");
                    return false;
                });
            RealmCommandHandler.Instance.AddCmdsOfAsm(typeof(RealmCommandHandler).Assembly);
            RealmCommandHandler.Instance.UnknownCommand +=
                (CommandMgr<RealmServerCmdArgs>.UnknownCommandHandler) (trigger =>
                    trigger.Reply("Unknown Command \"" + trigger.Alias + "\" - Type ? for help."));
        }

        private static void OnInvalidArguments(CmdTrigger<RealmServerCmdArgs> trigger, RealmServerCommand cmd)
        {
            trigger.Reply("Invalid command arguments - Required target-type: " + (object) cmd.TargetTypes +
                          " - Context required: " + (object) cmd.GetRequiresContext());
        }

        public static BufferedCommandResponse ExecuteBufferedCommand(string cmd)
        {
            BufferedCommandTrigger bufferedCommandTrigger = new BufferedCommandTrigger(new StringStream(cmd));
            RealmCommandHandler.Instance.Execute((CmdTrigger<RealmServerCmdArgs>) bufferedCommandTrigger);
            return bufferedCommandTrigger.Response;
        }

        /// <summary>
        /// Removes the next char if it's a Command Prefix, and
        /// sets dbl = true, if it is double.
        /// </summary>
        public static bool ConsumeCommandPrefix(StringStream str, out bool dbl)
        {
            char c = str.PeekChar();
            if (RealmCommandHandler.IsCommandPrefix(c))
            {
                ++str.Position;
                dbl = str.ConsumeNext(c);
                return true;
            }

            dbl = false;
            return false;
        }

        /// <summary>Whether the given character is a command prefix</summary>
        public static bool IsCommandPrefix(char c)
        {
            return RealmCommandHandler.CommandPrefixes.Contains<char>(c);
        }

        public override bool Execute(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return this.Execute(trigger, true);
        }

        public bool Execute(CmdTrigger<RealmServerCmdArgs> trigger, bool checkForCall)
        {
            if (checkForCall && trigger.Text.ConsumeNext(RealmCommandHandler.ExecCommandPrefix))
                return RealmCommandHandler.Call(trigger);
            return base.Execute(trigger);
        }

        public override BaseCommand<RealmServerCmdArgs> GetCommand(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Text.ConsumeNext(RealmCommandHandler.ExecCommandPrefix))
                return (BaseCommand<RealmServerCmdArgs>) WCell.RealmServer.Commands.CallCommand.Instance;
            return base.GetCommand(trigger);
        }

        /// <summary>Executes the trigger in Context</summary>
        public void ExecuteInContext(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            this.ExecuteInContext(trigger, (Action<CmdTrigger<RealmServerCmdArgs>>) null,
                (Action<CmdTrigger<RealmServerCmdArgs>>) null);
        }

        /// <summary>Executes the trigger in Context</summary>
        public void ExecuteInContext(CmdTrigger<RealmServerCmdArgs> trigger,
            Action<CmdTrigger<RealmServerCmdArgs>> doneCallback, Action<CmdTrigger<RealmServerCmdArgs>> failCalback)
        {
            BaseCommand<RealmServerCmdArgs> cmd = this.GetCommand(trigger);
            if (cmd == null)
                return;
            if (cmd.GetRequiresContext())
            {
                if (trigger.Args.Context == null)
                    RealmCommandHandler.OnInvalidArguments(trigger, (RealmServerCommand) cmd.RootCmd);
                else
                    trigger.Args.Context.ExecuteInContext((Action) (() =>
                        this.Execute(trigger, cmd, doneCallback, failCalback)));
            }
            else
                this.Execute(trigger, cmd, doneCallback, failCalback);
        }

        private void Execute(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd,
            Action<CmdTrigger<RealmServerCmdArgs>> doneCallback, Action<CmdTrigger<RealmServerCmdArgs>> failCalback)
        {
            if (this.Execute(trigger, cmd, false))
                doneCallback(trigger);
            else
                failCalback(trigger);
        }

        /// <summary>
        /// Calls <code>return Execute(new CmdTrigger(text));</code>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool Execute(StringStream text)
        {
            DefaultCmdTrigger trigger = new DefaultCmdTrigger(text);
            if (!trigger.InitTrigger())
                return true;
            return RealmCommandHandler.Instance.Execute((CmdTrigger<RealmServerCmdArgs>) trigger);
        }

        /// <summary>
        /// Calls <code>return Execute(new CmdTrigger(text));</code>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool Execute(string text)
        {
            DefaultCmdTrigger trigger = new DefaultCmdTrigger(text);
            if (!trigger.InitTrigger())
                return true;
            return RealmCommandHandler.Instance.Execute((CmdTrigger<RealmServerCmdArgs>) trigger);
        }

        public override bool Execute(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd,
            bool silentFail)
        {
            if (!cmd.RootCmd.GetRequiresContext() || trigger.Args.Context != null && trigger.Args.Context.IsInContext)
                return base.Execute(trigger, cmd, silentFail);
            trigger.Reply("Command requires different context: {0}", (object) cmd.RootCmd);
            return false;
        }

        public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd,
            bool silentFail)
        {
            if (!cmd.RootCmd.GetRequiresContext() || trigger.Args.Context != null && trigger.Args.Context.IsInContext)
                return base.Eval(trigger, cmd, silentFail);
            trigger.Reply("Command requires different context: {0}", (object) cmd.RootCmd);
            return (object) null;
        }

        /// <summary>Default Command-Handling method</summary>
        /// <returns>Whether the given msg triggered a command</returns>
        public static bool HandleCommand(IUser user, string msg, IGenericChatTarget target)
        {
            if (msg.Length > 0 && user.Role.Commands.Count > 0)
            {
                char ch1;
                bool isCall;
                if (!(isCall = !RealmCommandHandler.IsCommandPrefix(ch1 = msg[0])) ||
                    (int) ch1 == (int) RealmCommandHandler.ExecCommandPrefix)
                {
                    if (msg.Length != 2 || msg[1] != '?')
                    {
                        bool flag = false;
                        foreach (char ch2 in msg)
                        {
                            if (ch2 >= 'A')
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                            return false;
                    }

                    bool dbl = false;
                    int startIndex = 1;
                    if ((int) msg[1] == (int) ch1)
                    {
                        if (!user.Role.CanUseCommandsOnOthers)
                        {
                            user.SendMessage("You are not allowed to use Commands on others.");
                            return true;
                        }

                        if (user.Target == null)
                        {
                            user.SendMessage("Invalid target.");
                            return true;
                        }

                        dbl = true;
                        ++startIndex;
                    }

                    IngameCmdTrigger trigger =
                        new IngameCmdTrigger(new StringStream(msg.Substring(startIndex)), user, target, dbl);
                    if (trigger.InitTrigger())
                    {
                        if (trigger.Args.Context != null)
                            trigger.Args.Context.ExecuteInContext((Action) (() =>
                            {
                                if (!isCall)
                                    RealmCommandHandler.Instance.Execute((CmdTrigger<RealmServerCmdArgs>) trigger,
                                        false);
                                else
                                    RealmCommandHandler.Call((CmdTrigger<RealmServerCmdArgs>) trigger);
                            }));
                        else if (!isCall)
                            RealmCommandHandler.Instance.Execute((CmdTrigger<RealmServerCmdArgs>) trigger, false);
                        else
                            RealmCommandHandler.Call((CmdTrigger<RealmServerCmdArgs>) trigger);
                    }

                    return true;
                }

                if ((int) ch1 == (int) RealmCommandHandler.SelectCommandPrefix && user.Role.IsStaff)
                    return RealmCommandHandler.SelectCommand(user, msg.Substring(1));
            }

            return false;
        }

        /// <summary>
        /// Tries to select the corresponding command for the given User
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cmdString"></param>
        /// <returns>Whether Command was selected</returns>
        public static bool SelectCommand(IUser user, string cmdString)
        {
            if (cmdString.Length > 1)
            {
                BaseCommand<RealmServerCmdArgs> baseCommand = user.SelectedCommand == null
                    ? RealmCommandHandler.Instance.SelectCommand(cmdString)
                    : (BaseCommand<RealmServerCmdArgs>) user.SelectedCommand.SelectSubCommand(cmdString);
                if (baseCommand != null && user.Role.Commands.Contains(baseCommand.RootCmd))
                {
                    if (baseCommand.SubCommands.Count == 0)
                    {
                        user.SendMessage("Invalid Command selection - Command does not have SubCommands: " +
                                         (object) baseCommand);
                    }
                    else
                    {
                        user.SelectedCommand = baseCommand;
                        user.SendMessage("Selected: " + baseCommand.Name);
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool Call(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            trigger.Alias = nameof(Call);
            return RealmCommandHandler.Instance.Trigger(trigger,
                (BaseCommand<RealmServerCmdArgs>) WCell.RealmServer.Commands.CallCommand.Instance);
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth)]
        public static void OnStartup()
        {
            ServerApp<WCell.RealmServer.RealmServer>.Started += new Action(RealmCommandHandler.AutoexecStartup);
        }

        public static void AutoexecStartup()
        {
            RealmServerCmdArgs args = new RealmServerCmdArgs((IUser) null, false, (IGenericChatTarget) null);
            string file = RealmCommandHandler.AutoExecDir + RealmCommandHandler.AutoExecStartupFile;
            if (!File.Exists(file))
                return;
            ThreadPool.QueueUserWorkItem(
                (WaitCallback) (stateInfo => RealmCommandHandler.Instance.ExecFile(file, args)));
        }

        public override void ExecFile(string filename, CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Character != null)
            {
                if (File.Exists(filename))
                    RealmCommandHandler.ExecFileFor(filename, trigger.Args.Character, trigger);
                else
                    trigger.Reply("File to execute does not exist: " + filename);
            }
            else
                base.ExecFile(filename, trigger);
        }

        public static void ExecFileFor(Character user)
        {
            if (RealmCommandHandler.AutoExecDir == null)
                return;
            RealmCommandHandler.ExecFileFor(
                Path.Combine(RealmCommandHandler.AutoExecDir, "Chars/" + user.Account.Name + ".txt"), user);
        }

        public static void ExecFirstLoginFileFor(Character user)
        {
            RealmCommandHandler.ExecFileFor(
                RealmCommandHandler.AutoExecDir + RealmCommandHandler.AutoExecAllCharsFirstLoginFile, user);
        }

        public static void ExecAllCharsFileFor(Character user)
        {
            RealmCommandHandler.ExecFileFor(RealmCommandHandler.AutoExecDir + RealmCommandHandler.AutoExecAllCharsFile,
                user);
        }

        public static void ExecFileFor(string file, Character user)
        {
            RealmCommandHandler.ExecFileFor(file, user,
                (CmdTrigger<RealmServerCmdArgs>) new IngameCmdTrigger(new RealmServerCmdArgs((IUser) user, false,
                    (IGenericChatTarget) null)));
        }

        public static void ExecFileFor(string file, Character user, CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!File.Exists(file))
                return;
            bool mayExec = true;
            Func<CmdTrigger<RealmServerCmdArgs>, int, bool> cmdValidator =
                (Func<CmdTrigger<RealmServerCmdArgs>, int, bool>) ((trig, line) =>
                {
                    StringStream text = trigger.Text;
                    if (text.ConsumeNext('+'))
                    {
                        string str1 = text.NextWord();
                        if (str1 == "char" || str1 == "name")
                        {
                            string str2 = text.NextWord();
                            mayExec = str2.Length <= 0 ||
                                      user.Name.IndexOf(str2, StringComparison.InvariantCultureIgnoreCase) > -1;
                            return false;
                        }

                        if (str1 == "class")
                        {
                            string expr = text.Remainder.Trim();
                            long val = 0;
                            object error = (object) null;
                            if (!StringParser.Eval(typeof(ClassMask), ref val, expr, ref error, false))
                            {
                                CommandMgr<RealmServerCmdArgs>.log.Warn(
                                    "Invalid Class restriction in file {0} (line {1}): {2}", (object) file,
                                    (object) line, error);
                            }
                            else
                            {
                                ClassMask otherFlags = (ClassMask) val;
                                mayExec = otherFlags == ClassMask.None || user.ClassMask.HasAnyFlag(otherFlags);
                            }

                            return false;
                        }

                        trig.Reply("Invalid statement in file {0} (line: {1}): " + text.String.Substring(1).Trim(),
                            (object) file, (object) line);
                    }

                    if (mayExec && !trig.InitTrigger())
                        mayExec = false;
                    return mayExec;
                });
            RealmCommandHandler.Instance.ExecFile(file, trigger, cmdValidator);
        }
    }
}