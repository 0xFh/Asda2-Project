using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class HealthLeechEffectHandler : SpellEffectHandler
  {
    public HealthLeechEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      ((Unit) target).LeechHealth(m_cast.CasterUnit, CalcDamageValue(), Effect.ProcValue,
        Effect);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}