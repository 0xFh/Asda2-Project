using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class SchoolDamageEffectHandler : SpellEffectHandler
  {
    public SchoolDamageEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      ((Unit) target).DealSpellDamage(m_cast.CasterUnit, Effect, CalcDamageValue(), true, true,
        false, true);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}