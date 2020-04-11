using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class MirrorImageHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character playerOwner = this.Owner.PlayerOwner;
            Unit owner = this.Owner;
            if (playerOwner == null)
                return;
            owner.DisplayId = playerOwner.DisplayId;
            owner.UnitFlags2 |= UnitFlags2.MirrorImage;
        }

        protected override void Remove(bool cancelled)
        {
            Character playerOwner = this.Owner.PlayerOwner;
            Unit owner = this.Owner;
            if (playerOwner == null)
                return;
            owner.DisplayId = owner.NativeDisplayId;
            owner.UnitFlags2 ^= UnitFlags2.MirrorImage;
        }
    }
}