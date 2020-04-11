using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class EnergizePctEffectHandler : EnergizeEffectHandler
  {
    public EnergizePctEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      if((PowerType) Effect.MiscValue != ((Unit) target).PowerType)
        return;
      int num = (m_cast.CasterUnit.MaxPower * CalcEffectValue() + 50) / 100;
      ((Unit) target).Energize(num, m_cast.CasterUnit, Effect);
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Object; }
    }
  }
}