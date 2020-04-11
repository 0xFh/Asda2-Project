using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class CriticalBlockPctHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
        }

        public override void OnDefend(DamageAction action)
        {
            if (action.Blocked <= 0 || this.EffectValue <= Utility.Random(1, 101))
                return;
            action.Blocked *= 2;
        }
    }
}