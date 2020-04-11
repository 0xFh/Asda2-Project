using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
    public class Talent
    {
        public readonly TalentCollection Talents;
        public readonly TalentEntry Entry;
        private int m_rank;

        internal Talent(TalentCollection talents, TalentEntry entry)
        {
            this.Talents = talents;
            this.Entry = entry;
        }

        public Talent(TalentCollection talents, TalentEntry entry, int rank)
        {
            this.m_rank = -1;
            this.Talents = talents;
            this.Entry = entry;
            this.Rank = rank;
        }

        public Spell Spell
        {
            get { return this.Entry.Spells[this.m_rank]; }
        }

        /// <summary>The actual rank, as displayed in the GUI</summary>
        public int ActualRank
        {
            get { return this.Rank + 1; }
            set { this.Rank = value - 1; }
        }

        /// <summary>
        /// Current zero-based rank of this Talent.
        /// The rank displayed in the GUI is Rank+1.
        /// </summary>
        public int Rank
        {
            get { return this.m_rank; }
            set
            {
                int diff;
                if (this.m_rank > value)
                {
                    if (value < -1)
                        value = -1;
                    int delta = this.m_rank - value;
                    this.Talents.UpdateFreeTalentPointsSilently(delta);
                    for (int rank = this.m_rank; rank >= value + 1; --rank)
                        this.Talents.Owner.Spells.Remove(this.Entry.Spells[rank]);
                    if (value < 0)
                        this.Talents.ById.Remove(this.Entry.Id);
                    diff = -delta;
                }
                else
                {
                    if (value <= this.m_rank)
                        return;
                    if (value > this.Entry.MaxRank - 1)
                        value = this.Entry.MaxRank - 1;
                    diff = value - this.m_rank;
                    for (int index = this.m_rank + 1; index <= value; ++index)
                        this.Talents.Owner.Spells.AddSpell(this.Entry.Spells[value]);
                    this.Talents.UpdateFreeTalentPointsSilently(-diff);
                }

                this.Talents.UpdateTreePoint(this.Entry.Tree.TabIndex, diff);
                this.m_rank = value;
            }
        }

        /// <summary>
        /// Sets the rank without sending any packets or doing checks.
        /// Also does not increment spent talent points
        /// </summary>
        internal void SetRankSilently(int rank)
        {
            this.m_rank = rank;
        }

        public void Remove()
        {
            this.Remove(true);
        }

        /// <summary>Removes all ranks of this talent.</summary>
        internal void Remove(bool update)
        {
            this.Rank = -1;
            if (!update)
                return;
            TalentHandler.SendTalentGroupList(this.Talents);
        }
    }
}