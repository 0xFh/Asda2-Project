using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class HealMaxHealthEffectHandler : SpellEffectHandler
  {
    public HealMaxHealthEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason InitializeTarget(WorldObject target)
    {
      return SpellFailedReason.Ok;
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      Unit unit = (Unit) target;
      unit.Heal((m_cast.CasterUnit ?? unit).MaxHealth, m_cast.CasterUnit, Effect);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}