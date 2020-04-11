using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class RestoreManaPercentEffectHandler : SpellEffectHandler
  {
    public RestoreManaPercentEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      if(((Unit) target).PowerType != PowerType.Mana)
        return;
      int num = (int) (((Unit) target).MaxPower * CalcEffectValue() / 100.0);
      ((Unit) target).Energize(num, m_cast.CasterUnit, Effect);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}