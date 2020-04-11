using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>
  /// Either just heals or trades one Rejuvenation or Regrowth for a lot of healing
  /// </summary>
  public class HealEffectHandler : SpellEffectHandler
  {
    public HealEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      ((Unit) target).Heal(
        m_cast.CasterUnit.AddHealingModsToAction(
          (int) ((m_cast.CasterUnit.RandomMagicDamage / 500.0 + 1.0) *
                 Effect.MiscValue), Effect, DamageSchool.Magical), m_cast.CasterUnit,
        Effect);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}