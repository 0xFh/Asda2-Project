using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Formulas
{
    /// <summary>
    /// RageGenerator
    /// <see href="http://www.wowwiki.com/Formulas:Rage_generation"></see>
    /// </summary>
    public static class RageGenerator
    {
        public static RageGenerator.RageCalculator GenerateAttackerRage =
            new RageGenerator.RageCalculator(RageGenerator.GenerateDefaultAttackerRage);

        public static RageGenerator.RageCalculator GenerateTargetRage =
            new RageGenerator.RageCalculator(RageGenerator.GenerateDefaultVictimRage);

        /// <summary>Rage for the attacker of an AttackAction</summary>
        public static void GenerateDefaultAttackerRage(DamageAction action)
        {
            Unit attacker = action.Attacker;
            if (!action.IsWeaponAttack)
                return;
            double num1 = 3.5;
            if (action.IsCritical)
                num1 *= 2.0;
            double num2 = num1 * (double) action.Weapon.AttackTime;
            int level = attacker.Level;
            float num3 = (float) (0.00920000020414591 * (double) level * (double) level +
                                  3.23000001907349 * (double) level + 4.26999998092651);
            double num4 = (double) (15 * action.ActualDamage) / (4.0 * (double) num3) + num2 / 2000.0;
            float num5 = (float) (15 * action.ActualDamage) / num3;
            double num6 = num4;
            if (num4 <= (double) num5)
                num6 = (double) num5;
            attacker.Power += (int) num6 * 10;
        }

        /// <summary>Rage for the victim of an AttackAction</summary>
        public static void GenerateDefaultVictimRage(DamageAction action)
        {
            Unit victim = action.Victim;
            int level = victim.Level;
            int num = (int) (0.0092 * (double) level * (double) level + 3.23000001907349 * (double) level +
                             4.26999998092651);
            victim.Power += 2 * action.ActualDamage / num * 10;
        }

        public delegate void RageCalculator(DamageAction action);
    }
}