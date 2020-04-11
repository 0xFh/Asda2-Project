using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Trainers
{
    /// <summary>Represents everything an NPC trainer has to offer</summary>
    [Serializable]
    public class TrainerEntry
    {
        /// <summary>
        /// The major type of trainer this is (Class, Profession, Secondary Skill, etc.)
        /// </summary>
        public TrainerType TrainerType = TrainerType.NotATrainer;

        [NotPersistent] public readonly IDictionary<SpellId, TrainerSpellEntry> Spells =
            (IDictionary<SpellId, TrainerSpellEntry>) new Dictionary<SpellId, TrainerSpellEntry>(20);

        /// <summary>
        /// TODO: Text dislayed in the upper panel of the client's trainer list menu.
        /// </summary>
        public string Message = "Hello!";

        [NotPersistent] public ClassMask ClassMask;
        [NotPersistent] public RaceMask RaceMask;

        /// <summary>
        /// The required profession or secondary skill that the character must have to learn from this Trainer
        /// </summary>
        public SpellId RequiredSpellId;

        private int lastIndex;

        public void Sort(IComparer<TrainerSpellEntry> comparer)
        {
        }

        public void AddSpell(TrainerSpellEntry entry)
        {
            if (this.Spells.Count == 0)
                this.SetupTrainer(entry.Spell);
            if (this.Spells.ContainsKey(entry.SpellId))
                return;
            this.Spells.Add(entry.SpellId, entry);
            entry.Index = this.lastIndex++;
        }

        /// <summary>Determine Trainer-information, based on first Spell</summary>
        /// <param name="spell"></param>
        private void SetupTrainer(Spell spell)
        {
            if (spell.Ability == null)
                return;
            this.RaceMask = spell.Ability.RaceMask;
            this.ClassMask = spell.Ability.ClassMask;
        }

        /// <summary>
        /// Whether this NPC can train the character in their specialty.
        /// </summary>
        /// <returns>True if able to train.</returns>
        public bool CanTrain(Character chr)
        {
            if (this.RequiredSpellId != SpellId.None && !chr.Spells.Contains(this.RequiredSpellId) ||
                this.RaceMask != ~RaceMask.AllRaces1 && !this.RaceMask.HasAnyFlag(chr.RaceMask))
                return false;
            if (this.ClassMask != ClassMask.None)
                return this.ClassMask.HasAnyFlag(chr.ClassMask);
            return true;
        }

        /// <summary>
        /// Returns the TrainerSpellEntry from SpellsForSale with the given spellId, else null.
        /// </summary>
        public TrainerSpellEntry GetSpellEntry(SpellId spellId)
        {
            TrainerSpellEntry trainerSpellEntry;
            this.Spells.TryGetValue(spellId, out trainerSpellEntry);
            return trainerSpellEntry;
        }
    }
}