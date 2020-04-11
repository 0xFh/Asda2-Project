using WCell.Constants.Spells;

namespace WCell.RealmServer.Misc
{
  /// <summary>Default implementation for IProcHandler</summary>
  public class ProcHandlerTemplate
  {
    protected int m_stackCount;

    protected ProcHandlerTemplate()
    {
    }

    public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction,
      uint procChance = 100, int stackCount = 0)
    {
      ProcTriggerFlags = triggerFlags;
      ProcHitFlags = hitFlags;
      ProcChance = procChance;
      Validator = null;
      ProcAction = procAction;
      m_stackCount = stackCount;
    }

    public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction,
      ProcValidator validator = null, uint procChance = 100, int stackCount = 0)
    {
      ProcTriggerFlags = triggerFlags;
      ProcHitFlags = hitFlags;
      ProcChance = procChance;
      Validator = validator;
      ProcAction = procAction;
      m_stackCount = stackCount;
    }

    public ProcValidator Validator { get; set; }

    public ProcCallback ProcAction { get; set; }

    /// <summary>The amount of times that this Aura has been applied</summary>
    public int StackCount
    {
      get { return m_stackCount; }
      set { m_stackCount = value; }
    }

    public ProcTriggerFlags ProcTriggerFlags { get; set; }

    public ProcHitFlags ProcHitFlags { get; set; }

    /// <summary>Chance to proc in %</summary>
    public uint ProcChance { get; set; }

    /// <summary>In Milliseconds</summary>
    public int MinProcDelay { get; set; }
  }
}