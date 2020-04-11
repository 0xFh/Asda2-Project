using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class SpellLine : ISpellGroup, IEnumerable<Spell>, IEnumerable
    {
        private readonly List<Spell> Spells;
        private readonly Spell m_firstSpell;
        public readonly SpellLineId LineId;
        private int count;

        public SpellLine(SpellLineId id, params Spell[] spells)
        {
            this.LineId = id;
            this.AuraUID = (uint) id;
            this.Spells = new List<Spell>();
            if (spells.Length <= 0)
                return;
            this.m_firstSpell = spells[0];
            for (int index = 0; index < spells.Length; ++index)
            {
                Spell spell = spells[index];
                if (spell != null)
                    this.AddSpell(spell);
            }
        }

        public string Name
        {
            get { return this.LineId.ToString(); }
        }

        public ClassId ClassId
        {
            get { return this.m_firstSpell.ClassId; }
        }

        public int SpellCount
        {
            get { return this.count; }
        }

        public Spell FirstRank
        {
            get { return this.m_firstSpell; }
        }

        /// <summary>The spell with the highest rank in this line</summary>
        public Spell HighestRank
        {
            get
            {
                Spell spell = this.m_firstSpell;
                while (spell.NextRank != null)
                    spell = spell.NextRank;
                return spell;
            }
        }

        public uint AuraUID { get; internal set; }

        internal void AddSpell(Spell spell)
        {
            if (spell == null)
                throw new ArgumentNullException(nameof(spell));
            this.Spells.Add(spell);
            spell.Line = this;
            ++this.count;
        }

        public Spell GetRank(int rank)
        {
            Spell spell = this.m_firstSpell;
            while (spell.Rank != rank)
                spell = spell.NextRank;
            return spell;
        }

        public IEnumerator<Spell> GetEnumerator()
        {
            return (IEnumerator<Spell>) this.Spells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            SpellLine spellLine = obj as SpellLine;
            if (spellLine != null)
                return this.LineId.Equals((object) spellLine.LineId);
            return false;
        }

        public override int GetHashCode()
        {
            return this.LineId.GetHashCode();
        }

        public Spell BaseSpell
        {
            get { return this.FirstRank; }
        }
    }
}