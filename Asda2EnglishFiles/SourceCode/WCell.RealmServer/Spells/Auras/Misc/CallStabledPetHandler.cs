using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Spells.Auras
{
    public class CallStabledPetHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
            if (target.Class == ClassId.THS)
                return;
            failReason = SpellFailedReason.BadTargets;
        }

        protected override void Apply()
        {
            if (!(this.m_aura.Owner is Character))
                return;
            Character owner = this.m_aura.Owner as Character;
            PetHandler.SendStabledPetsList((IPacketReceiver) owner, (Unit) owner, (byte) owner.StableSlotCount,
                owner.StabledPetRecords);
        }
    }
}