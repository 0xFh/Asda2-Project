using System;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOChestEntry : GOEntry, IGOLootableEntry
    {
        /// <summary>The Id of the Loot that can be looted from this Chest</summary>
        public uint LootId
        {
            get { return (uint) this.Fields[1]; }
            set { }
        }

        /// <summary>
        /// Minimum number of consecutive times this object can be opened.
        /// </summary>
        public int MinRestock
        {
            get { return this.Fields[4]; }
            set { }
        }

        /// <summary>
        /// Maximum number of consecutive times this object can be opened.
        /// </summary>
        public int MaxRestock
        {
            get { return this.Fields[5]; }
            set { }
        }

        /// <summary>The time it takes until the chest restocks its loot.</summary>
        public int ChestRestockTime
        {
            get { return this.Fields[2]; }
        }

        /// <summary>
        /// The event-id of a Quest to be triggered upon looting this kind of GO
        ///  </summary>
        public int LootedEventId
        {
            get { return this.Fields[6]; }
        }

        /// <summary>The Id of the quest required to be active</summary>
        public override uint QuestId
        {
            get { return (uint) this.Fields[8]; }
        }

        /// <summary>
        /// The minimum level a character can be in order to open this chest.
        /// </summary>
        public int MinLevel
        {
            get { return this.Fields[9]; }
        }

        /// <summary>
        /// Possibly, don't trigger a restock event unless the chest is looted completely (?)
        /// </summary>
        public bool LeaveLoot
        {
            get { return this.Fields[11] > 0; }
        }

        /// <summary>
        /// Possibly, whether this chest can be looted during combat (?)
        /// </summary>
        public bool NotInCombat
        {
            get { return this.Fields[12] != 0; }
        }

        /// <summary>
        /// Possibly whether or not to log the looting of this chest (?)
        /// </summary>
        public bool LogLoot
        {
            get { return this.Fields[13] != 0; }
        }

        /// <summary>
        /// The Id of a text object to display upon opening this chest.
        /// </summary>
        public int OpenTextId
        {
            get { return this.Fields[14]; }
        }

        public int FloatingTooltip
        {
            get { return this.Fields[16]; }
        }

        protected internal override void InitEntry()
        {
            this.Lock = LockEntry.Entries.Get<LockEntry>((uint) this.Fields[0]);
            this.IsConsumable = this.Fields[3] > 0 || this.Flags.HasFlag((Enum) GameObjectFlags.ConditionalInteraction);
            this.LinkedTrapId = (uint) this.Fields[7];
            this.LosOk = this.Fields[10] > 0;
            this.UseGroupLoot = this.Fields[15] > 0;
        }
    }
}