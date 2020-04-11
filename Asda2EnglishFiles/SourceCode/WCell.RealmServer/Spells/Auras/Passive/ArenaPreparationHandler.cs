using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Passive
{
    public class ArenaPreparationHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.UnitFlags |= UnitFlags.Preparation;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                               UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled |
                               UnitFlags.Flag_0x10 | UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 |
                               UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat |
                               UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 |
                               UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat |
                               UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared |
                               UnitFlags.Possessed | UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                               UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                               UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
        }
    }
}