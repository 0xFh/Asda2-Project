using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// Item-bound spell, contains information on charges, cooldown, trigger etc
    /// </summary>
    public class ItemSpell
    {
        public static readonly ItemSpell[] EmptyArray = new ItemSpell[0];
        [NotPersistent] public Spell Spell;
        public SpellId Id;

        /// <summary>The index of this spell within the Template</summary>
        [NotPersistent] public uint Index;

        public ItemSpellTrigger Trigger;
        public short Charges;

        /// <summary>SpellCategory.dbc</summary>
        public uint CategoryId;

        public int Cooldown;
        public int CategoryCooldown;
        [NotPersistent] public bool HasCharges;

        public void FinalizeAfterLoad()
        {
            this.Spell = SpellHandler.Get(this.Id);
            this.HasCharges = this.Charges != (short) 0;
        }

        public override string ToString()
        {
            return this.ToString(true);
        }

        public string ToString(bool inclTrigger)
        {
            List<string> collection = new List<string>(5);
            if (this.Charges != (short) 0)
                collection.Add("Charges: " + (object) this.Charges);
            if (this.Cooldown > 0)
                collection.Add("Cooldown: " + (object) this.Cooldown);
            if (this.CategoryId > 0U)
                collection.Add("CategoryId: " + (object) this.CategoryId);
            if (this.CategoryCooldown > 0)
                collection.Add("CategoryCooldown: " + (object) this.CategoryCooldown);
            string str = string.Format(((int) this.Id).ToString() + " (" + (object) this.Id + ")" +
                                       (collection.Count > 0
                                           ? (object) (" - " + collection.ToString<string>(", "))
                                           : (object) ""));
            if (inclTrigger)
                str = str + "[" + (object) this.Trigger + "]";
            return str;
        }
    }
}