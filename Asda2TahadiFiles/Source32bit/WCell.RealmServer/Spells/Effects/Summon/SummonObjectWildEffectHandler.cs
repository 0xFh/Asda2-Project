using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
  /// <summary>Summons an object without owner</summary>
  public class SummonObjectWildEffectHandler : SpellEffectHandler
  {
    private GameObject go;

    public SummonObjectWildEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      GOEntry entry = GOMgr.GetEntry((GOEntryId) Effect.MiscValue, true);
      Unit casterUnit = m_cast.CasterUnit;
      if(entry == null)
        return;
      if(Cast.TargetLoc.X != 0.0)
      {
        WorldLocation worldLocation = new WorldLocation(casterUnit.Map, Cast.TargetLoc, 1U);
        go = entry.Spawn(worldLocation);
      }
      else
        go = entry.Spawn(casterUnit, null);

      go.State = GameObjectState.Enabled;
      go.Orientation = casterUnit.Orientation;
      go.ScaleX = 1f;
    }
  }
}