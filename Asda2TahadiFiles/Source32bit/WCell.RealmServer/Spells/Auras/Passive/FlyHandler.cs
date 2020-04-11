using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Enables flying (mostly triggered through flying rides)
  /// </summary>
  public class FlyHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      if(!(target is Character) || ((Character) target).CanFly)
        return;
      failReason = SpellFailedReason.NotHere;
    }

    protected override void Apply()
    {
      ++m_aura.Auras.Owner.Flying;
    }

    protected override void Remove(bool cancelled)
    {
      --m_aura.Auras.Owner.Flying;
    }
  }
}