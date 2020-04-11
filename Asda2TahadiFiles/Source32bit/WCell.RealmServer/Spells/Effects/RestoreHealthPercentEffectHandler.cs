using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class RestoreHealthPercentEffectHandler : SpellEffectHandler
  {
    public RestoreHealthPercentEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      ((Unit) target).HealPercent(CalcDamageValue(), m_cast.CasterUnit, Effect);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}