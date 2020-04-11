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
      if(m_targets != null && m_targets.Count > 0)
        firstTarget = (Unit) m_targets[0];
      return SpellFailedReason.Ok;
    }

    public override void Apply()
    {
      GOEntryId miscValue = (GOEntryId) Effect.MiscValue;
      GOEntry entry = GOMgr.GetEntry(miscValue, true);
      Unit casterUnit = m_cast.CasterUnit;
      if(entry != null)
      {
        GO = entry.Spawn(casterUnit, casterUnit);
        GO.State = GameObjectState.Enabled;
        GO.Orientation = casterUnit.Orientation;
        GO.ScaleX = 1f;
        GO.Faction = casterUnit.Faction;
        GO.CreatedBy = casterUnit.EntityId;
        if(GO.Handler is SummoningRitualHandler)
          ((SummoningRitualHandler) GO.Handler).Target = firstTarget;
        if(m_cast.IsChanneling)
        {
          m_cast.CasterUnit.ChannelObject = GO;
        }
        else
        {
          if(Effect.Spell.Durations.Min <= 0)
            return;
          GO.RemainingDecayDelayMillis = Effect.Spell.Durations.Random();
        }
      }
      else
        log.Error("Summoning Spell {0} refers to invalid Object: {1} ({2})",
          Effect.Spell, miscValue, miscValue);
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}