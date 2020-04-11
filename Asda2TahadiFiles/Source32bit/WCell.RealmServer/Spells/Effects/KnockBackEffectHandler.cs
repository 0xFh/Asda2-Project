using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Knocks all targets back</summary>
  public class KnockBackEffectHandler : SpellEffectHandler
  {
    public KnockBackEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      MovementHandler.SendKnockBack(m_cast.CasterObject, target, Effect.MiscValue / 10f,
        CalcEffectValue() / 10f);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}