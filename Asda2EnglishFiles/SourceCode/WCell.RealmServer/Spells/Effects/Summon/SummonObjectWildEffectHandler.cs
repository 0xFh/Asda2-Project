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
            GOEntry entry = GOMgr.GetEntry((GOEntryId) this.Effect.MiscValue, true);
            Unit casterUnit = this.m_cast.CasterUnit;
            if (entry == null)
                return;
            if ((double) this.Cast.TargetLoc.X != 0.0)
            {
                WorldLocation worldLocation = new WorldLocation(casterUnit.Map, this.Cast.TargetLoc, 1U);
                this.go = entry.Spawn((IWorldLocation) worldLocation);
            }
            else
                this.go = entry.Spawn((IWorldLocation) casterUnit, (Unit) null);

            this.go.State = GameObjectState.Enabled;
            this.go.Orientation = casterUnit.Orientation;
            this.go.ScaleX = 1f;
        }
    }
}