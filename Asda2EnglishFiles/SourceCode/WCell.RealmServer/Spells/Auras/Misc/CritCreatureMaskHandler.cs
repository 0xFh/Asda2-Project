using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>Always crits any creature of the given types</summary>
    public class CritCreatureMaskHandler : AttackEventEffectHandler
    {
        public CreatureMask Mask { get; set; }

        public CritCreatureMaskHandler(CreatureMask mask)
        {
            this.Mask = mask;
        }

        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            if (!(action.Victim is NPC) || !((NPC) action.Victim).CheckCreatureType(this.Mask))
                return;
            action.IsCritical = true;
            action.SetCriticalDamage();
        }

        public override void OnDefend(DamageAction action)
        {
        }
    }
}