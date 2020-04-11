using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Privileges;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
  public class RealmServerCmdArgs : ICmdArgs
  {
    protected bool m_dbl;
    protected IUser m_user;
    protected Character m_chr;
    protected IGenericChatTarget m_chatTarget;
    private Unit m_target;

    public RealmServerCmdArgs(RealmServerCmdArgs args)
    {
      m_user = args.m_user;
      m_chr = args.Character;
      m_chatTarget = args.m_chatTarget;
      Role = args.Role;
      m_dbl = args.m_dbl;
      InitArgs();
    }

    public RealmServerCmdArgs(IUser user, bool dbl, IGenericChatTarget chatTarget)
    {
      m_user = user;
      m_chr = m_user as Character;
      m_chatTarget = chatTarget;
      Role = m_user != null ? m_user.Role : Singleton<PrivilegeMgr>.Instance.HighestRole;
      m_dbl = dbl;
      InitArgs();
    }

    private void InitArgs()
    {
      Context = m_chr;
      SetTarget();
    }

    /// <summary>Whether there was a double trigger-prefix</summary>
    public bool Double
    {
      get { return m_dbl; }
      set
      {
        m_dbl = value;
        SetTarget();
      }
    }

    /// <summary>
    /// The context for the Command.
    /// This is the Character's Context by default.
    /// Make sure to treat all Object involved properly.
    /// </summary>
    public IContextHandler Context { get; set; }

    /// <summary>Only available if IngameOnly = true</summary>
    public Unit Target
    {
      get { return m_target; }
      set { m_target = value; }
    }

    public WorldObject SelectedUnitOrGO
    {
      get
      {
        Character character = Character;
        if(character == null)
          return null;
        return character.Target ?? (WorldObject) character.ExtraInfo.SelectedGO;
      }
    }

    /// <summary>
    /// The Channel, Group, Guild or person that the Command was directed to (if any)
    /// </summary>
    public IGenericChatTarget ChatTarget
    {
      get { return m_chatTarget; }
    }

    public bool HasCharacter
    {
      get { return Character != null; }
    }

    /// <summary>
    /// The Character who is performing this or null (if not ingame-triggered)
    /// </summary>
    public Character Character
    {
      get
      {
        if(m_chr == null || !m_chr.IsInWorld)
          return null;
        return m_chr;
      }
      set
      {
        m_chr = value;
        Context = value;
        SetTarget();
      }
    }

    /// <summary>
    /// The User who triggered the Command (might be null if used from Console etc)
    /// </summary>
    public IUser User
    {
      get { return m_user; }
      set { m_user = value; }
    }

    /// <summary>The triggering User or null (if not ingame-triggered)</summary>
    public RoleGroup Role { get; private set; }

    /// <summary>The GO that is currently selected by the User</summary>
    public GameObject SelectedGO
    {
      get
      {
        if(!(m_user is Character))
          return null;
        return GOSelectMgr.Instance[Character];
      }
    }

    /// <summary>
    /// The TicketHandler triggering this Trigger (or null if there is none).
    /// </summary>
    public virtual ITicketHandler TicketHandler
    {
      get { return m_user as ITicketHandler; }
    }

    public T GetTarget<T>() where T : Unit
    {
      Unit target = Target;
      if(target is T)
        return (T) target;
      if(Character != null)
        return (T) Character.Target;
      return default(T);
    }

    private void SetTarget()
    {
      if(Character == null)
        return;
      if(Double)
        m_target = Character.Target;
      else
        m_target = Character;
    }

    /// <summary>
    /// Whether this the supplied arguments match the specified target criteria.
    /// </summary>
    public bool CheckArgs(Command<RealmServerCmdArgs> cmd)
    {
      if(!(cmd is RealmServerCommand))
        return true;
      RealmServerCommand realmServerCommand = (RealmServerCommand) cmd;
      Unit target = Target;
      ObjectTypeCustom targetTypes = realmServerCommand.TargetTypes;
      if(realmServerCommand.RequiresCharacter && m_chr == null)
        return false;
      if(targetTypes == ObjectTypeCustom.None)
        return true;
      if(target != null)
        return targetTypes.HasAnyFlag(target.CustomType);
      return false;
    }

    /// <summary>
    /// If the trigger has anymore text, it will get the Character whose name matches the next word, else
    /// takes the current Target.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="playerName"></param>
    /// <returns></returns>
    public Character GetCharArgumentOrTarget(CmdTrigger<RealmServerCmdArgs> trigger, string playerName)
    {
      Character character;
      if(playerName.Length > 0)
      {
        character = World.GetCharacter(playerName, false);
        if(character == null)
        {
          trigger.Reply("Character {0} does not exist or is offline.", (object) playerName);
          return null;
        }
      }
      else
        character = (Character) trigger.Args.Target;

      return character;
    }
  }
}