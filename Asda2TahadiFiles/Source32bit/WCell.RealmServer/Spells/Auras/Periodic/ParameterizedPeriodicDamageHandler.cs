namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ParameterizedPeriodicDamageHandler : PeriodicDamageHandler
  {
    public int TotalDamage { get; set; }

    public ParameterizedPeriodicDamageHandler()
      : this(0)
    {
    }

    public ParameterizedPeriodicDamageHandler(int totalDmg)
    {
      TotalDamage = totalDmg;
    }

    protected override void Apply()
    {
      BaseEffectValue = TotalDamage / (m_aura.TicksLeft + 1);
      TotalDamage -= BaseEffectValue;
    }
  }
}