using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Targeting
{
    public delegate void TargetAdder(SpellTargetCollection targets, TargetFilter filter,
        ref SpellFailedReason failReason);
}