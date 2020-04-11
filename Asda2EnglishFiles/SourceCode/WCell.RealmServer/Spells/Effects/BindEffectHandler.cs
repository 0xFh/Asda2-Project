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
                this.Effect.ImplicitTargetA == ImplicitSpellTargetType.TeleportLocation ||
                this.Effect.ImplicitTargetB == ImplicitSpellTargetType.TeleportLocation
                    ? new WorldZoneLocation(this.m_cast.TargetMap, this.m_cast.TargetLoc,
                        this.m_cast.TargetMap.GetZone(this.m_cast.TargetLoc.X, this.m_cast.TargetLoc.Y).Template)
                    : new WorldZoneLocation(target.Map, target.Position, target.ZoneTemplate);
            ((Character) target).BindTo(target, (IWorldZoneLocation) worldZoneLocation);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}