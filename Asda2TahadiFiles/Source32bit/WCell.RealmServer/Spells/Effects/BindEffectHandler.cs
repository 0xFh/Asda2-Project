using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class BindEffectHandler : SpellEffectHandler
  {
    public BindEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      WorldZoneLocation worldZoneLocation =
        Effect.ImplicitTargetA == ImplicitSpellTargetType.TeleportLocation ||
        Effect.ImplicitTargetB == ImplicitSpellTargetType.TeleportLocation
          ? new WorldZoneLocation(m_cast.TargetMap, m_cast.TargetLoc,
            m_cast.TargetMap.GetZone(m_cast.TargetLoc.X, m_cast.TargetLoc.Y).Template)
          : new WorldZoneLocation(target.Map, target.Position, target.ZoneTemplate);
      ((Character) target).BindTo(target, worldZoneLocation);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Player; }
    }
  }
}