namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ParameterizedPeriodicHealHandler : PeriodicHealHandler
  {
    public int TotalHeal { get; set; }

    public ParameterizedPeriodicHealHandler(int totalDmg = 0)
    {
      TotalHeal = totalDmg;
    }

    protected override void Apply()
    {
      BaseEffectValue = TotalHeal / (m_aura.TicksLeft + 1);
      TotalHeal -= BaseEffectValue;
    }
  }
}