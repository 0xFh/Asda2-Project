using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Deals EffectValue in % of Melee AP</summary>
  public class SchoolDamageByAPPctEffectHandler : SchoolDamageEffectHandler
  {
    public SchoolDamageByAPPctEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      int dmg = (m_cast.CasterUnit.TotalMeleeAP * CalcDamageValue() + 50) / 100;
      ((Unit) target).DealSpellDamage(m_cast.CasterUnit, Effect, dmg, true, true, false, true);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}