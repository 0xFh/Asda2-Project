using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModIncreaseSwimSpeedHandler : AuraEffectHandler
  {
    private float val;

    protected override void Apply()
    {
      m_aura.Auras.Owner.SwimSpeedFactor =
        UnitUpdates.GetMultiMod(val = EffectValue / 100f,
          m_aura.Auras.Owner.SwimSpeedFactor);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.SwimSpeedFactor =
        UnitUpdates.GetMultiMod(-val, m_aura.Auras.Owner.SwimSpeedFactor);
    }
  }
}