using NLog;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Handlers;

namespace WCell.RealmServer.Spells.Effects
{
    public class SummonObjectEffectHandler : SpellEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public GameObject GO;
        private Unit firstTarget;

        public SummonObjectEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (this.m_targets != null && this.m_targets.Count > 0)
                this.firstTarget = (Unit) this.m_targets[0];
            return SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            GOEntryId miscValue = (GOEntryId) this.Effect.MiscValue;
            GOEntry entry = GOMgr.GetEntry(miscValue, true);
            Unit casterUnit = this.m_cast.CasterUnit;
            if (entry != null)
            {
                this.GO = entry.Spawn((IWorldLocation) casterUnit, casterUnit);
                this.GO.State = GameObjectState.Enabled;
                this.GO.Orientation = casterUnit.Orientation;
                this.GO.ScaleX = 1f;
                this.GO.Faction = casterUnit.Faction;
                this.GO.CreatedBy = casterUnit.EntityId;
                if (this.GO.Handler is SummoningRitualHandler)
                    ((SummoningRitualHandler) this.GO.Handler).Target = this.firstTarget;
                if (this.m_cast.IsChanneling)
                {
                    this.m_cast.CasterUnit.ChannelObject = (WorldObject) this.GO;
                }
                else
                {
                    if (this.Effect.Spell.Durations.Min <= 0)
                        return;
                    this.GO.RemainingDecayDelayMillis = this.Effect.Spell.Durations.Random();
                }
            }
            else
                SummonObjectEffectHandler.log.Error("Summoning Spell {0} refers to invalid Object: {1} ({2})",
                    (object) this.Effect.Spell, (object) miscValue, (object) miscValue);
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}