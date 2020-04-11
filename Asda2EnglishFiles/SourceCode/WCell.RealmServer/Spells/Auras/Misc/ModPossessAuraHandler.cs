using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class ModPossessAuraHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterRef, Unit target,
            ref SpellFailedReason failReason)
        {
            Unit unit = creatingCast.CasterReference.Object as Unit;
            if (unit == null)
                failReason = SpellFailedReason.BadTargets;
            else if (unit.Charm != null)
                failReason = SpellFailedReason.AlreadyHaveCharm;
            else if (target.HasMaster)
                failReason = SpellFailedReason.TooManySockets;
            else if (unit.HasMaster)
            {
                failReason = SpellFailedReason.Possessed;
            }
            else
            {
                if (!(unit is Character) || ((Character) unit).ActivePet == null)
                    return;
                failReason = SpellFailedReason.AlreadyHaveSummon;
            }
        }

        protected override void Apply()
        {
            Unit casterUnit = this.m_aura.CasterUnit;
            if (casterUnit == null)
                return;
            Unit owner = this.m_aura.Auras.Owner;
            casterUnit.Charm = owner;
            owner.Charmer = casterUnit;
            int duration = this.m_aura.Duration;
            Character character = casterUnit as Character;
            if (character == null)
                return;
            character.Possess(duration, owner, true, true);
        }

        protected override void Remove(bool cancelled)
        {
            Unit casterUnit = this.m_aura.CasterUnit;
            Unit owner = this.m_aura.Auras.Owner;
            casterUnit.Charm = (Unit) null;
            owner.Charmer = (Unit) null;
            Character character = casterUnit as Character;
            if (character == null)
                return;
            character.UnPossess(owner);
        }
    }
}