using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
    internal sealed class SpellHitChecker
    {
        private Spell spell;
        private WorldObject caster;
        private Unit target;

        public void Initialize(Spell spell, WorldObject caster)
        {
            this.spell = spell;
            this.caster = caster;
        }

        public CastMissReason CheckHitAgainstTarget(Unit target)
        {
            this.target = target;
            return this.CheckHitAgainstTarget();
        }

        private CastMissReason CheckHitAgainstTarget()
        {
            if (this.target.IsEvading)
                return CastMissReason.Evade;
            if (this.spell.IsAffectedByInvulnerability ||
                this.target is Character && ((Character) this.target).Role.IsStaff)
            {
                if (this.target.IsInvulnerable)
                    return CastMissReason.Immune_2;
                if (this.spell.IsAffectedByInvulnerability &&
                    ((IEnumerable<DamageSchool>) this.spell.Schools).All<DamageSchool>(
                        new Func<DamageSchool, bool>(this.target.IsImmune)))
                    return CastMissReason.Immune;
            }

            return this.CheckMiss() ? CastMissReason.Miss : CastMissReason.None;
        }

        private bool CheckMiss()
        {
            return (double) this.CalculateHitChanceAgainstTargetInPercentage() < (double) Utility.Random(0, 101);
        }

        private float CalculateHitChanceAgainstTargetInPercentage()
        {
            float min = 0.0f;
            float targetInPercentage = (float) this.CalculateBaseHitChanceAgainstTargetInPercentage();
            if (this.caster is Unit)
            {
                targetInPercentage += (float) (this.caster as Unit).GetHighestSpellHitChanceMod(this.spell.Schools);
                if (this.caster is Character)
                {
                    min = 1f;
                    targetInPercentage += (this.caster as Character).SpellHitChanceFromHitRating;
                }
            }

            return MathUtil.ClampMinMax(targetInPercentage, min, 100f);
        }

        private int CalculateBaseHitChanceAgainstTargetInPercentage()
        {
            int num1 = this.target.Level - this.caster.SharedReference.Level;
            if (num1 < 3)
            {
                int num2 = 96 - num1;
                if (num2 <= 100)
                    return num2;
                return 100;
            }

            int num3 = !(this.target is Character) ? 11 : 7;
            return 94 - (num1 - 2) * num3;
        }
    }
}