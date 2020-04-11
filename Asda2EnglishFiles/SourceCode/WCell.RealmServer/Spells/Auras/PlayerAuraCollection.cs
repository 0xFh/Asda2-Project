using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// AuraCollection for Character objects.
    /// Contains a lot of modifications and bookkeeping that is not required for NPCs.
    /// </summary>
    public class PlayerAuraCollection : AuraCollection
    {
        /// <summary>Flat modifiers of spells</summary>
        internal readonly List<AddModifierEffectHandler> SpellModifiersFlat = new List<AddModifierEffectHandler>(5);

        /// <summary>Percent modifiers of spells</summary>
        internal readonly List<AddModifierEffectHandler> SpellModifiersPct = new List<AddModifierEffectHandler>(5);

        /// <summary>
        /// Mask of spells that are allowed to crit hit, although they are not allowed to, by default
        /// </summary>
        internal readonly uint[] CriticalStrikeEnabledMask = new uint[3];

        /// <summary>
        /// Set of Auras that are only applied when certain items are equipped
        /// </summary>
        private List<Aura> itemRestrictedAuras;

        /// <summary>
        /// Set of Auras that are only applied in certain shapeshift forms
        /// </summary>
        private List<Aura> shapeshiftRestrictedAuras;

        /// <summary>
        /// Set of Auras that are only applied in certain AuraStates
        /// </summary>
        private List<Aura> auraStateRestrictedAuras;

        /// <summary>
        /// Set of Auras which have effects that depend on other Auras
        /// </summary>
        private List<Aura> aurasWithAuraDependentEffects;

        /// <summary>
        /// Amount of currently added modifiers that require charges.
        /// If &gt; 0, will iterate over modifiers and remove charges after SpellCasts.
        /// </summary>
        public int ModifierWithChargesCount { get; protected set; }

        public PlayerAuraCollection(Character owner)
            : base((Unit) owner)
        {
        }

        public override void AddAura(Aura aura, bool start)
        {
            base.AddAura(aura, start);
            this.OnAuraAddedOrRemoved();
            if (aura.Spell.IsPassive)
            {
                if (aura.Spell.HasItemRequirements)
                    this.ItemRestrictedAuras.Add(aura);
                if (aura.Spell.IsModalShapeshiftDependentAura)
                    this.ShapeshiftRestrictedAuras.Add(aura);
                if (aura.Spell.RequiredCasterAuraState != AuraState.None)
                    this.AuraStateRestrictedAuras.Add(aura);
            }

            if (!aura.Spell.HasAuraDependentEffects)
                return;
            this.AurasWithAuraDependentEffects.Add(aura);
        }

        protected internal override void Remove(Aura aura)
        {
            base.Remove(aura);
            this.OnAuraAddedOrRemoved();
            if (aura.Spell.IsPassive)
            {
                if (aura.Spell.HasItemRequirements)
                    this.ItemRestrictedAuras.Remove(aura);
                if (aura.Spell.IsModalShapeshiftDependentAura)
                    this.ShapeshiftRestrictedAuras.Remove(aura);
                if (aura.Spell.RequiredCasterAuraState != AuraState.None)
                    this.AuraStateRestrictedAuras.Remove(aura);
            }

            if (!aura.Spell.HasAuraDependentEffects)
                return;
            this.AurasWithAuraDependentEffects.Remove(aura);
        }

        public void AddSpellModifierPercent(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
                ++this.ModifierWithChargesCount;
            this.SpellModifiersPct.Add(modifier);
            this.OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character) this.m_owner, modifier.SpellEffect, true);
        }

        public void AddSpellModifierFlat(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
                ++this.ModifierWithChargesCount;
            this.SpellModifiersFlat.Add(modifier);
            this.OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character) this.m_owner, modifier.SpellEffect, false);
        }

        public void RemoveSpellModifierPercent(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
                --this.ModifierWithChargesCount;
            this.OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character) this.m_owner, modifier.SpellEffect, true);
            this.SpellModifiersPct.Remove(modifier);
        }

        public void RemoveSpellModifierFlat(AddModifierEffectHandler modifier)
        {
            if (modifier.Charges > 0)
                --this.ModifierWithChargesCount;
            this.OnModifierChange(modifier);
            AuraHandler.SendModifierUpdate((Character) this.m_owner, modifier.SpellEffect, false);
            this.SpellModifiersFlat.Remove(modifier);
        }

        private void OnModifierChange(AddModifierEffectHandler modifier)
        {
            foreach (Aura aura in this.Owner.Auras)
            {
                if (aura.IsActivated && !aura.Spell.IsEnhancer && modifier.SpellEffect.MatchesSpell(aura.Spell))
                    aura.ReApplyNonPeriodicEffects();
            }
        }

        /// <summary>
        /// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
        /// </summary>
        public override int GetModifiedInt(SpellModifierType type, Spell spell, int value)
        {
            int modifierFlat = this.GetModifierFlat(type, spell);
            int modifierPercent = this.GetModifierPercent(type, spell);
            return ((value + modifierFlat) * (100 + modifierPercent) + 50) / 100;
        }

        /// <summary>
        /// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
        /// </summary>
        public override int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
        {
            int modifierFlat = this.GetModifierFlat(type, spell);
            int modifierPercent = this.GetModifierPercent(type, spell);
            return ((value - modifierFlat) * (100 - modifierPercent) + 50) / 100;
        }

        /// <summary>
        /// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
        /// </summary>
        public override float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
        {
            int modifierFlat = this.GetModifierFlat(type, spell);
            int modifierPercent = this.GetModifierPercent(type, spell);
            return (float) (((double) value + (double) modifierFlat) * (1.0 + (double) modifierPercent / 100.0));
        }

        /// <summary>
        /// Returns the percent modifier (through certain talents) of the given type for the given spell
        /// </summary>
        public int GetModifierPercent(SpellModifierType type, Spell spell)
        {
            int num = 0;
            for (int index = 0; index < this.SpellModifiersPct.Count; ++index)
            {
                AddModifierEffectHandler modifierEffectHandler = this.SpellModifiersPct[index];
                if ((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == type &&
                    modifierEffectHandler.SpellEffect.MatchesSpell(spell))
                    num += modifierEffectHandler.SpellEffect.ValueMin;
            }

            return num;
        }

        /// <summary>
        /// Returns the flat modifier (through certain talents) of the given type for the given spell
        /// </summary>
        public int GetModifierFlat(SpellModifierType type, Spell spell)
        {
            int num = 0;
            for (int index = 0; index < this.SpellModifiersFlat.Count; ++index)
            {
                AddModifierEffectHandler modifierEffectHandler = this.SpellModifiersFlat[index];
                if ((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == type &&
                    modifierEffectHandler.SpellEffect.MatchesSpell(spell))
                    num += modifierEffectHandler.SpellEffect.ValueMin;
            }

            return num;
        }

        public override void OnCasted(SpellCast cast)
        {
            Spell spell = cast.Spell;
            if (this.ModifierWithChargesCount <= 0)
                return;
            List<IAura> auraList = (List<IAura>) null;
            foreach (AddModifierEffectHandler modifierEffectHandler in this.SpellModifiersFlat)
            {
                SpellEffect spellEffect = modifierEffectHandler.SpellEffect;
                if (spellEffect.MatchesSpell(spell) && cast.Spell != spellEffect.Spell &&
                    (cast.TriggerEffect == null || cast.TriggerEffect.Spell != spellEffect.Spell) &&
                    modifierEffectHandler.Charges > 0)
                {
                    --modifierEffectHandler.Charges;
                    if (modifierEffectHandler.Charges < 1)
                    {
                        if (auraList == null)
                            auraList = SpellCast.AuraListPool.Obtain();
                        auraList.Add((IAura) modifierEffectHandler.Aura);
                    }
                }
            }

            foreach (AddModifierEffectHandler modifierEffectHandler in this.SpellModifiersPct)
            {
                SpellEffect spellEffect = modifierEffectHandler.SpellEffect;
                if (spellEffect.MatchesSpell(spell) && cast.Spell != spellEffect.Spell &&
                    (cast.TriggerEffect == null || cast.TriggerEffect.Spell != spellEffect.Spell) &&
                    modifierEffectHandler.Charges > 0)
                {
                    --modifierEffectHandler.Charges;
                    if (modifierEffectHandler.Charges < 1)
                    {
                        if (auraList == null)
                            auraList = SpellCast.AuraListPool.Obtain();
                        auraList.Add((IAura) modifierEffectHandler.Aura);
                    }
                }
            }

            if (auraList == null)
                return;
            foreach (IAura aura in auraList)
                aura.Remove(false);
            auraList.Clear();
            SpellCast.AuraListPool.Recycle(auraList);
        }

        private List<Aura> ItemRestrictedAuras
        {
            get
            {
                if (this.itemRestrictedAuras == null)
                    this.itemRestrictedAuras = new List<Aura>(3);
                return this.itemRestrictedAuras;
            }
        }

        internal void OnEquip(Item item)
        {
        }

        internal void OnBeforeUnEquip(Item item)
        {
        }

        private List<Aura> ShapeshiftRestrictedAuras
        {
            get
            {
                if (this.shapeshiftRestrictedAuras == null)
                    this.shapeshiftRestrictedAuras = new List<Aura>(3);
                return this.shapeshiftRestrictedAuras;
            }
        }

        internal void OnShapeshiftFormChanged()
        {
            if (this.shapeshiftRestrictedAuras == null)
                return;
            foreach (Aura shapeshiftRestrictedAura in this.shapeshiftRestrictedAuras)
            {
                if (shapeshiftRestrictedAura.Spell.RequiredShapeshiftMask != ShapeshiftMask.None)
                    shapeshiftRestrictedAura.IsActivated = this.MayActivate(shapeshiftRestrictedAura);
                else if (shapeshiftRestrictedAura.Spell.HasShapeshiftDependentEffects)
                    shapeshiftRestrictedAura.ReEvaluateNonPeriodicHandlerRequirements();
            }
        }

        private List<Aura> AuraStateRestrictedAuras
        {
            get
            {
                if (this.auraStateRestrictedAuras == null)
                    this.auraStateRestrictedAuras = new List<Aura>(2);
                return this.auraStateRestrictedAuras;
            }
        }

        internal void OnAuraStateChanged()
        {
            if (this.auraStateRestrictedAuras == null)
                return;
            foreach (Aura stateRestrictedAura in this.auraStateRestrictedAuras)
                stateRestrictedAura.IsActivated = this.MayActivate(stateRestrictedAura);
        }

        private List<Aura> AurasWithAuraDependentEffects
        {
            get
            {
                if (this.aurasWithAuraDependentEffects == null)
                    this.aurasWithAuraDependentEffects = new List<Aura>(2);
                return this.aurasWithAuraDependentEffects;
            }
        }

        internal void OnAuraAddedOrRemoved()
        {
            if (this.aurasWithAuraDependentEffects == null)
                return;
            foreach (Aura auraDependentEffect in this.aurasWithAuraDependentEffects)
            {
                foreach (AuraEffectHandler handler in auraDependentEffect.Handlers)
                {
                    if (handler.SpellEffect.IsDependentOnOtherAuras)
                        handler.IsActivated = this.MayActivate(handler);
                }
            }
        }

        /// <summary>
        /// Check all restrictions on the given Aura (optionally, exclude item check)
        /// </summary>
        private bool MayActivate(Aura aura, bool inclItemCheck)
        {
            return true;
        }

        protected internal override bool MayActivate(Aura aura)
        {
            if (this.MayActivate(aura, true))
                return true;
            return base.MayActivate(aura);
        }

        protected internal override bool MayActivate(AuraEffectHandler handler)
        {
            SpellEffect spellEffect = handler.SpellEffect;
            if ((spellEffect.RequiredShapeshiftMask == ShapeshiftMask.None ||
                 spellEffect.RequiredShapeshiftMask.HasAnyFlag(this.Owner.ShapeshiftMask)) &&
                (spellEffect.RequiredActivationAuras == null || this.ContainsAny(spellEffect.RequiredActivationAuras)))
                return true;
            return base.MayActivate(handler);
        }

        /// <summary>
        /// Returns wehther the given spell is allowed to crit, if it was not
        /// allowed to crit by default. (Due to Talents that override Spell behavior)
        /// </summary>
        public bool CanSpellCrit(Spell spell)
        {
            return spell.MatchesMask(this.CriticalStrikeEnabledMask);
        }
    }
}