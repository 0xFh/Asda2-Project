using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Targeting
{
    public delegate int TargetEvaluator(SpellEffectHandler effectHandler, WorldObject target);
}