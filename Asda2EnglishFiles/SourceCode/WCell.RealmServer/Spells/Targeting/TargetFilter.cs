using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Targeting
{
    public delegate void TargetFilter(SpellEffectHandler effectHandler, WorldObject target,
        ref SpellFailedReason failReason);
}