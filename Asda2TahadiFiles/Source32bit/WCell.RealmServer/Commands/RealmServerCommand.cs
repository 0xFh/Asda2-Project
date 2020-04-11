using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public abstract class RealmServerCommand : Command<RealmServerCmdArgs>
  {
    public TranslatableItem Description;
    public TranslatableItem ParamInfo;

    /// <summary>The status that is required by a Command by default</summary>
    public virtual RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    /// <summary>
    /// The kind of target that is required for this command
    /// (Target is set to the command-calling User, if he/she has none selected or not doubled the Command-Prefix).
    /// </summary>
    public virtual ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.None; }
    }

    public virtual bool RequiresContext
    {
      get
      {
        if(!RequiresCharacter)
          return TargetTypes != ObjectTypeCustom.None;
        return true;
      }
    }

    /// <summary>
    /// Whether the Character argument needs to be supplied by the trigger's Args
    /// </summary>
    public virtual bool RequiresCharacter
    {
      get { return false; }
    }

    /// <summary>
    /// Whether the command-user must be of equal or higher rank of the target.
    /// Used to prevent staff members of lower ranks to perform any kind
    /// of unwanted commands on staff members of higher ranks.
    /// </summary>
    public virtual bool RequiresEqualOrHigherRank
    {
      get { return true; }
    }

    public void ProcessNth(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      uint n = trigger.Text.NextUInt(1U);
      string key = trigger.Text.NextWord();
      BaseCommand<RealmServerCmdArgs>.SubCommand subCommand;
      if(m_subCommands.TryGetValue(key, out subCommand))
      {
        trigger.Args = new RealmServerNCmdArgs(trigger.Args, n);
        if(!MayTrigger(trigger, subCommand, false))
          return;
        subCommand.Process(trigger);
      }
      else
      {
        trigger.Reply(RealmLangKey.SubCommandNotFound, (object) key);
        trigger.Text.Skip(trigger.Text.Length);
        mgr.DisplayCmd(trigger, this);
      }
    }

    public override bool MayTrigger(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> cmd,
      bool silent)
    {
      if(!(cmd is SubCommand) || trigger.Args.User == null || trigger.Args.Role.Status >=
         ((SubCommand) cmd).DefaultRequiredStatus)
        return true;
      if(!silent)
        trigger.Reply(RealmLangKey.MustNotUseCommand, (object) cmd.Name);
      return false;
    }

    public override string GetDescription(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      if(Description == null)
        return base.GetDescription(trigger);
      return trigger.Translate(Description);
    }

    public override string GetParamInfo(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      if(ParamInfo == null)
        return base.GetParamInfo(trigger);
      return trigger.Translate(ParamInfo);
    }

    public abstract class SubCommand : BaseCommand<RealmServerCmdArgs>.SubCommand
    {
      public TranslatableItem Description;
      public TranslatableItem ParamInfo;

      public virtual RoleStatus DefaultRequiredStatus
      {
        get { return ((RealmServerCommand) m_parentCmd).RequiredStatusDefault; }
      }

      public override string GetDescription(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(Description == null)
          return base.GetDescription(trigger);
        return trigger.Translate(Description);
      }

      public override string GetParamInfo(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(ParamInfo == null)
          return base.GetParamInfo(trigger);
        return trigger.Translate(ParamInfo);
      }
    }
  }
}