using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class TrackResourcesHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      if(target is Character)
        return;
      failReason = SpellFailedReason.TargetNotPlayer;
    }

    protected override void Apply()
    {
      ((Character) m_aura.Auras.Owner).ResourceTracking = (LockMask) (1 << m_spellEffect.MiscValue - 1);
    }

    protected override void Remove(bool cancelled)
    {
      ((Character) m_aura.Auras.Owner).ResourceTracking = LockMask.None;
    }
  }
}