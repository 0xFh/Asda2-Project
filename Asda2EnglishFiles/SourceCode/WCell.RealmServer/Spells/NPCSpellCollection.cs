using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
    /// <summary>NPC spell collection</summary>
    public class NPCSpellCollection : SpellCollection
    {
        /// <summary>Cooldown of NPC spells, if they don't have one</summary>
        public static int DefaultNPCSpellCooldownMillis = 1500;

        private static readonly ObjectPool<NPCSpellCollection> NPCSpellCollectionPool =
            new ObjectPool<NPCSpellCollection>((Func<NPCSpellCollection>) (() => new NPCSpellCollection()));

        protected HashSet<Spell> m_readySpells;
        protected List<NPCSpellCollection.CooldownRemoveTimer> m_cooldowns;

        public static NPCSpellCollection Obtain(NPC npc)
        {
            NPCSpellCollection npcSpellCollection = NPCSpellCollection.NPCSpellCollectionPool.Obtain();
            npcSpellCollection.Initialize((Unit) npc);
            return npcSpellCollection;
        }

        private NPCSpellCollection()
        {
            this.m_readySpells = new HashSet<Spell>();
        }

        protected internal override void Recycle()
        {
            base.Recycle();
            this.m_readySpells.Clear();
            if (this.m_cooldowns != null)
                this.m_cooldowns.Clear();
            NPCSpellCollection.NPCSpellCollectionPool.Recycle(this);
        }

        public NPC OwnerNPC
        {
            get { return this.Owner as NPC; }
        }

        public IEnumerable<Spell> ReadySpells
        {
            get { return (IEnumerable<Spell>) this.m_readySpells; }
        }

        public int ReadyCount
        {
            get { return this.m_readySpells.Count; }
        }

        /// <summary>The max combat of any 1vs1 combat spell</summary>
        public float MaxCombatSpellRange { get; private set; }

        /// <summary>Shuffles all currently ready Spells</summary>
        public void ShuffleReadySpells()
        {
            Utility.Shuffle<Spell>((ICollection<Spell>) this.m_readySpells);
        }

        public Spell GetReadySpell(SpellId spellId)
        {
            foreach (Spell readySpell in this.m_readySpells)
            {
                if (readySpell.SpellId == spellId)
                    return readySpell;
            }

            return (Spell) null;
        }

        public override void AddSpell(Spell spell)
        {
            if (this.m_byId.ContainsKey(spell.SpellId))
                return;
            base.AddSpell(spell);
            this.OnNewSpell(spell);
        }

        private void OnNewSpell(Spell spell)
        {
            if (!spell.IsAreaSpell && !spell.IsAura && spell.HasHarmfulEffects)
                this.MaxCombatSpellRange = Math.Max(this.MaxCombatSpellRange,
                    this.Owner.GetSpellMaxRange(spell, (WorldObject) null));
            this.AddReadySpell(spell);
        }

        /// <summary>
        /// Adds the given spell as ready. Once casted, the spell will be removed.
        /// This can be used to signal a one-time cast of a spell whose priority is to be
        /// compared to the other spells.
        /// </summary>
        public void AddReadySpell(Spell spell)
        {
            if (spell.IsPassive || !this.Contains(spell.Id))
                return;
            this.m_readySpells.Add(spell);
        }

        public override void Clear()
        {
            base.Clear();
            this.m_readySpells.Clear();
        }

        public override bool Remove(Spell spell)
        {
            if (!base.Remove(spell))
                return false;
            this.m_readySpells.Remove(spell);
            if ((double) this.Owner.GetSpellMaxRange(spell, (WorldObject) null) >= (double) this.MaxCombatSpellRange)
            {
                this.MaxCombatSpellRange = 0.0f;
                foreach (Spell spell1 in this.m_byId.Values)
                {
                    if ((double) spell1.Range.MaxDist > (double) this.MaxCombatSpellRange)
                        this.MaxCombatSpellRange = this.Owner.GetSpellMaxRange(spell1, (WorldObject) null);
                }
            }

            return true;
        }

        /// <summary>
        /// When NPC Spells cooldown, they get removed from the list of
        /// ready Spells.
        /// </summary>
        public override void AddCooldown(Spell spell, Item item)
        {
            int num = Math.Max(spell.GetCooldown(this.Owner), spell.CategoryCooldownTime);
            if (num <= 0)
            {
                if (spell.CastDelay != 0U || spell.Durations.Max != 0)
                    return;
                int modifiedInt = this.Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell,
                    NPCSpellCollection.DefaultNPCSpellCooldownMillis);
                this.AddCooldown(spell, modifiedInt);
            }
            else
            {
                int modifiedInt = this.Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell, num);
                this.AddCooldown(spell, modifiedInt);
            }
        }

        public void AddCooldown(Spell spell, DateTime cdTime)
        {
            int milliSecondsInt = (cdTime - DateTime.Now).ToMilliSecondsInt();
            this.AddCooldown(spell, milliSecondsInt);
        }

        private void AddCooldown(Spell spell, int millis)
        {
            if (millis <= 0)
                return;
            this.m_readySpells.Remove(spell);
            NPCSpellCollection.CooldownRemoveTimer cooldownRemoveTimer =
                new NPCSpellCollection.CooldownRemoveTimer(millis, spell);
            this.Owner.CallDelayed(millis,
                (Action<WorldObject>) (o => ((NPCSpellCollection) this.Owner.Spells).AddReadySpell(spell)));
            if (this.m_cooldowns == null)
                this.m_cooldowns = new List<NPCSpellCollection.CooldownRemoveTimer>();
            this.m_cooldowns.Add(cooldownRemoveTimer);
        }

        public override void ClearCooldowns()
        {
            IContextHandler contextHandler = this.Owner.ContextHandler;
            if (contextHandler == null)
                return;
            contextHandler.AddMessage((Action) (() =>
            {
                if (this.m_cooldowns == null)
                    return;
                foreach (NPCSpellCollection.CooldownRemoveTimer cooldown in this.m_cooldowns)
                {
                    this.Owner.RemoveUpdateAction((ObjectUpdateTimer) cooldown);
                    this.AddReadySpell(cooldown.Spell);
                }
            }));
        }

        public override bool IsReady(Spell spell)
        {
            return this.m_readySpells.Contains(spell);
        }

        public override void ClearCooldown(Spell spell, bool alsoCategory = true)
        {
            if (this.m_cooldowns == null)
                return;
            for (int index = 0; index < this.m_cooldowns.Count; ++index)
            {
                NPCSpellCollection.CooldownRemoveTimer cooldown = this.m_cooldowns[index];
                if ((int) cooldown.Spell.Id == (int) spell.Id)
                {
                    this.m_cooldowns.Remove(cooldown);
                    this.AddReadySpell(cooldown.Spell);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the delay until the given spell has cooled down in milliseconds
        /// </summary>
        public int GetRemainingCooldownMillis(Spell spell)
        {
            if (this.m_cooldowns == null)
                return 0;
            NPCSpellCollection.CooldownRemoveTimer cooldownRemoveTimer =
                this.m_cooldowns.Find(
                    (Predicate<NPCSpellCollection.CooldownRemoveTimer>) (cd => (int) cd.Spell.Id == (int) spell.Id));
            if (cooldownRemoveTimer != null)
                return cooldownRemoveTimer.GetDelayUntilNextExecution((WorldObject) this.Owner);
            return 0;
        }

        protected class CooldownRemoveTimer : OneShotObjectUpdateTimer
        {
            public CooldownRemoveTimer(int millis, Spell spell)
                : base(millis, (Action<WorldObject>) null)
            {
                this.Spell = spell;
                this.Callback = new Action<WorldObject>(this.DoRemoveCooldown);
            }

            public Spell Spell { get; set; }

            private void DoRemoveCooldown(WorldObject owner)
            {
                ((NPCSpellCollection) ((Unit) owner).Spells).AddReadySpell(this.Spell);
            }
        }
    }
}