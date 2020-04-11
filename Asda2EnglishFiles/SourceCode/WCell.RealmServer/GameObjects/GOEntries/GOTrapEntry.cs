using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOTrapEntry : GOEntry, ISpellParameters
    {
        public int Open
        {
            get { return this.Fields[0]; }
        }

        public int Level
        {
            get { return this.Fields[1]; }
        }

        /// <summary>
        /// The explosion radius of this Trap in yards (Assume default if 0)
        /// </summary>
        public int Radius
        {
            get { return this.Fields[2]; }
            set { }
        }

        public SpellId SpellId
        {
            get { return (SpellId) this.Fields[3]; }
        }

        public Spell Spell { get; set; }

        /// <summary>
        /// Probably maximum charges (trap disappears after all charges have been used)
        /// </summary>
        public int MaxCharges
        {
            get { return this.Fields[4]; }
        }

        public int Amplitude
        {
            get { return this.Fields[5] * 1000; }
            set { }
        }

        public int AutoClose
        {
            get { return this.Fields[6]; }
        }

        /// <summary>Trigger-delay in seconds</summary>
        public int StartDelay
        {
            get { return this.Fields[7] * 1000; }
            set { }
        }

        public int ServerOnly
        {
            get { return this.Fields[8]; }
        }

        /// <summary>Whether this trap is stealthed</summary>
        public bool Stealthed
        {
            get { return this.Fields[9] > 0; }
        }

        public int Large
        {
            get { return this.Fields[10]; }
        }

        public int StealthAffected
        {
            get { return this.Fields[11]; }
        }

        public int OpenTextID
        {
            get { return this.Fields[12]; }
        }

        public int CloseTextID
        {
            get { return this.Fields[13]; }
        }

        public int IgnoreTotems
        {
            get { return this.Fields[14]; }
        }

        protected internal override void InitEntry()
        {
            if (this.Radius < 1)
                this.Radius = 5;
            this.Spell = SpellHandler.Get(this.SpellId);
        }

        protected internal override void InitGO(GameObject trap)
        {
            trap.Level = this.Level;
            trap.IsStealthed = this.Stealthed;
            if (trap.HasAreaAuras)
                return;
            if (this.Spell != null)
            {
                AreaAura areaAura = new AreaAura((WorldObject) trap, this.Spell, (ISpellParameters) this);
            }

            trap.m_IsTrap = true;
        }
    }
}