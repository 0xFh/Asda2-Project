using System;
using WCell.Constants;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Adds damage absorption.
    /// 
    /// There are two kinds of absorbtions:
    /// 1. 100% absorbtion, up until a max is absorbed (usually)
    /// 2. Less than 100% absorption until time runs out (or max is absorbed -&gt; Needs customization, since its usually different each time)
    /// </summary>
    public class SchoolAbsorbHandler : AttackEventEffectHandler
    {
        public int RemainingValue;

        protected override void Apply()
        {
            if (this.SpellEffect.MiscValueC == 0)
                this.RemainingValue = this.SpellEffect.MiscValue;
            else if (this.SpellEffect.MiscValueB == 1)
                this.RemainingValue = (int) ((double) this.SpellEffect.MiscValue *
                                             (double) this.m_aura.CasterUnit.RandomMagicDamage / 100.0);
            base.Apply();
        }

        public override void OnDefend(DamageAction action)
        {
            this.RemainingValue = action.Absorb(this.RemainingValue, (DamageSchoolMask) this.m_spellEffect.MiscValueC);
            if (this.RemainingValue > 0)
                return;
            this.Owner.AddMessage(new Action(this.m_aura.Cancel));
        }
    }
}