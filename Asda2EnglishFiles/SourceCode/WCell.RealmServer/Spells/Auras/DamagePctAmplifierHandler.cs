﻿using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras
{
    public class DamagePctAmplifierHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
        }

        public override void OnDefend(DamageAction action)
        {
            if (!this.m_spellEffect.Spell.SchoolMask.HasAnyFlag(action.UsedSchool))
                return;
            action.ModDamagePercent(this.EffectValue);
        }
    }
}