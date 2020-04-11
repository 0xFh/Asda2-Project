using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public class SpellTriggerInfo
    {
        [NotPersistent] public Spell Spell;
        private SpellId m_SpellId;
        public uint QuestId;

        public SpellId SpellId
        {
            get { return this.m_SpellId; }
            set
            {
                this.m_SpellId = value;
                if (value == SpellId.None)
                    return;
                this.Spell = SpellHandler.Get(value);
            }
        }
    }
}