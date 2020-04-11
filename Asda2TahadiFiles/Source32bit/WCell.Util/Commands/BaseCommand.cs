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

    protected IDictionary<string, SubCommand> m_subCommands =
      new Dictionary<string, SubCommand>(
        StringComparer.InvariantCultureIgnoreCase);

    protected HashSet<SubCommand> m_subCommandSet = new HashSet<SubCommand>();

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
        while(baseCommand is SubCommand)
          baseCommand = baseCommand.m_parentCmd;
        return (Command<C>) baseCommand;
      }
    }

    /// <summary>
    /// The parent of this SubCommand (can be a further SubCommand or a Command)
    /// </summary>
    public BaseCommand<C> ParentCmd
    {
      get { return m_parentCmd; }
    }

    public IDictionary<string, SubCommand> SubCommandsByAlias
    {
      get { return m_subCommands; }
    }

    public HashSet<SubCommand> SubCommands
    {
      get { return m_subCommandSet; }
    }

    /// <summary>
    /// Indicates whether or not this command is enabled.
    /// If false, Commands.ReactTo will not trigger this Command'str Process method.
    /// Alternatively you can Add/Remove this Command to/from Commands.CommandsByAlias to control whether or not
    /// certain Commands should or should not be used.
    /// </summary>
    public bool Enabled
    {
      get { return m_enabled; }
      set { m_enabled = value; }
    }

    public virtual string Name
    {
      get
      {
        string str = GetType().Name;
        int length = str.IndexOf("Command");
        if(length >= 0)
          str = str.Substring(0, length);
        return str;
      }
    }

    /// <summary>A human-readable list of expected parameters</summary>
    public string EnglishParamInfo
    {
      get { return m_englishParamInfo; }
      set { m_englishParamInfo = value; }
    }

    /// <summary>Describes the command itself.</summary>
    public string EnglishDescription
    {
      get { return m_EnglishDescription; }
      set { m_EnglishDescription = value; }
    }

    internal void DoInit()
    {
      Initialize();
      if(Aliases.Length == 0)
        throw new Exception("Command has no Aliases: " + this);
      foreach(string aliase in Aliases)
      {
        if(aliase.Contains(" "))
          throw new Exception("Command-Alias \"" + aliase + "\" must not contain spaces in " + this);
        if(aliase.Length == 0)
          throw new Exception("Command has empty Alias: " + this);
      }
    }

    protected abstract void Initialize();

    protected void Init(params string[] aliases)
    {
      m_enabled = true;
      m_englishParamInfo = "";
      Aliases = aliases;
      AddSubCmds();
    }

    public virtual string GetDescription(CmdTrigger<C> trigger)
    {
      return EnglishDescription;
    }

    public virtual string GetParamInfo(CmdTrigger<C> trigger)
    {
      return m_englishParamInfo;
    }

    public string CreateInfo(CmdTrigger<C> trigger)
    {
      return CreateUsage(trigger) + " (" + GetDescription(trigger) + ")";
    }

    /// <summary>Returns a simple usage string</summary>
    public string CreateUsage()
    {
      return CreateUsage(GetParamInfo(null));
    }

    public string CreateUsage(CmdTrigger<C> trigger)
    {
      return CreateUsage(GetParamInfo(trigger));
    }

    public virtual string CreateUsage(string paramInfo)
    {
      paramInfo = Aliases.ToString("|") + " " + paramInfo;
      if(m_parentCmd != null)
        paramInfo = m_parentCmd.CreateUsage(paramInfo);
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
      return null;
    }

    protected void TriggerSubCommand(CmdTrigger<C> trigger)
    {
      string key = trigger.Text.NextWord();
      SubCommand subCommand;
      if(m_subCommands.TryGetValue(key, out subCommand))
      {
        if(!RootCmd.MayTrigger(trigger, subCommand, false))
          return;
        subCommand.Process(trigger);
      }
      else
      {
        trigger.Reply("SubCommand not found: " + key);
        trigger.Text.Skip(trigger.Text.Length);
        mgr.DisplayCmd(trigger, this, false, false);
      }
    }

    protected internal void AddSubCmds()
    {
      Type[] nestedTypes = GetType().GetNestedTypes(BindingFlags.Public);
      if(nestedTypes.Length <= 0)
        return;
      foreach(Type type in nestedTypes)
      {
        if(type.IsSubclassOf(typeof(SubCommand)) && !type.IsAbstract)
        {
          ConstructorInfo constructor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new Type[0], new ParameterModifier[0]);
          if(constructor == null)
            throw new ArgumentException(type.FullName + " lacks parameterless constructor.");
          AddSubCmd(constructor.Invoke(null) as SubCommand);
        }
      }
    }

    public void AddSubCmd(SubCommand cmd)
    {
      cmd.m_parentCmd = this;
      cmd.Initialize();
      foreach(string aliase in cmd.Aliases)
      {
        m_subCommands[aliase] = cmd;
        m_subCommandSet.Add(cmd);
      }
    }

    /// <summary>
    /// Creates a default string of all aliases and all subcommands
    /// </summary>
    protected virtual string CreateGroupUsageString()
    {
      return Aliases.ToString("|") + " " +
             m_subCommands.Keys.ToString("|");
    }

    /// <summary>Returns the direct SubCommand with the given alias.</summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public SubCommand SelectSubCommand(string alias)
    {
      SubCommand subCommand;
      m_subCommands.TryGetValue(alias, out subCommand);
      return subCommand;
    }

    /// <summary>
    /// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
    /// </summary>
    public SubCommand SelectSubCommand(StringStream cmdString)
    {
      SubCommand subCommand = SelectSubCommand(cmdString.NextWord());
      if(subCommand != null && subCommand.SubCommands.Count > 0 && cmdString.HasNext)
        return subCommand.SelectSubCommand(cmdString);
      return subCommand;
    }

    /// <summary>
    /// Returns the SubCommand, following the given set of aliases through the SubCommand structure.
    /// </summary>
    public void GetSubCommands(StringStream cmdString, List<BaseCommand<C>> list)
    {
      string str = cmdString.NextWord();
      IEnumerable<SubCommand> source = m_subCommandSet.Where(
        comd =>
          comd.Aliases.Where(alias =>
            alias.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > -1).Count() > 0);
      if(cmdString.HasNext && source.Count() == 1)
      {
        source.First().GetSubCommands(cmdString, list);
      }
      else
      {
        foreach(SubCommand subCommand in source)
          list.Add(subCommand);
      }
    }

    public override string ToString()
    {
      return Name + "-Command";
    }

    protected internal void FailNotify(CmdTrigger<C> trigger, Exception ex)
    {
      log.Warn(ex);
      OnFail(trigger, ex);
    }

    /// <summary>Is triggered when the processing throws an Exception.</summary>
    protected virtual void OnFail(CmdTrigger<C> trigger, Exception ex)
    {
      trigger.Reply("Command failed: ");
      foreach(string allMessage in ex.GetAllMessages())
        trigger.Reply(allMessage);
    }

    public abstract class SubCommand : BaseCommand<C>
    {
    }
  }
}