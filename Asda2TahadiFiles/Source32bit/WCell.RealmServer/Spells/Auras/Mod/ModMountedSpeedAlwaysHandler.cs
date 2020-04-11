using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Same as ModIncreaseMountedSpeed, it seems</summary>
  public class ModMountedSpeedAlwaysHandler : AuraEffectHandler
  {
    private float val;

    protected override void Apply()
    {
      m_aura.Auras.Owner.MountSpeedMod = UnitUpdates.GetMultiMod(val = EffectValue / 100f,
        m_aura.Auras.Owner.MountSpeedMod);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.MountSpeedMod =
        UnitUpdates.GetMultiMod(-val, m_aura.Auras.Owner.MountSpeedMod);
    }
  }
}