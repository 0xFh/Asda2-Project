using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Triggers a spell on this Effect's targets</summary>
  public class TriggerSpellEffectHandler : SpellEffectHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    public TriggerSpellEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      if(Effect.TriggerSpell == null)
        log.Warn("Tried to cast Spell \"{0}\" which has invalid TriggerSpellId {1}",
          Effect.Spell, Effect.TriggerSpellId);
      else
        TriggerSpell(Effect.TriggerSpell);
    }

    protected void TriggerSpell(Spell triggerSpell)
    {
      m_cast.Trigger(triggerSpell, Effect,
        triggerSpell.Effects.Length != 1 || m_targets == null
          ? null
          : m_targets.ToArray());
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
    }
  }
}