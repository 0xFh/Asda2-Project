using System;
using System.Collections.Generic;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Logs
{
    public class LogHelperEntry
    {
        private readonly List<LogAttribute> _attributes = new List<LogAttribute>();
        private readonly List<LogHelperEntry> _referenceEntries = new List<LogHelperEntry>();
        private readonly string _title;
        private readonly LogSourceType _sourceType;
        private readonly uint _triggerId;
        public LogEntryRecord Record;

        public LogHelperEntry(string title, LogSourceType sourceType, uint triggerId)
        {
            this._title = title;
            this._sourceType = sourceType;
            this._triggerId = triggerId;
        }

        public LogHelperEntry AddAttribute(string title, double value, string message = "")
        {
            this._attributes.Add(new LogAttribute()
            {
                Message = message,
                Title = title,
                Value = value
            });
            return this;
        }

        public LogHelperEntry Write()
        {
            Log.Write(this._sourceType, this._triggerId, this._title, (IEnumerable<LogAttribute>) this._attributes,
                this._referenceEntries, new Action<LogEntryRecord>(this.SetRecord));
            return this;
        }

        private void SetRecord(LogEntryRecord record)
        {
            this.Record = record;
        }

        public LogHelperEntry AddItemAttributes(Asda2Item item, string itemName = "")
        {
            if (item == null)
                return this;
            this.AddAttribute("item_id", (double) item.ItemId, item.Name + itemName);
            this.AddAttribute("is_soulbound", item.IsSoulbound ? 1.0 : 0.0, itemName);
            this.AddAttribute("inventory_type", (double) item.InventoryType,
                itemName + " " + item.InventoryType.ToString());
            this.AddAttribute("slot", (double) item.Slot, itemName);
            if (item.Template.IsStackable)
                this.AddAttribute("amount", (double) item.Amount, itemName);
            if (item.Enchant != (byte) 0)
                this.AddAttribute("enchant", (double) item.Enchant, itemName);
            if (item.Soul1Id != 0)
                this.AddAttribute("sowel_1_id", (double) item.Soul1Id, itemName);
            if (item.Soul2Id != 0)
                this.AddAttribute("sowel_2_id", (double) item.Soul2Id, itemName);
            if (item.Soul3Id != 0)
                this.AddAttribute("sowel_3_id", (double) item.Soul3Id, itemName);
            if (item.Soul4Id != 0)
                this.AddAttribute("sowel_4_id", (double) item.Soul4Id, itemName);
            if (item.Parametr1Type != Asda2ItemBonusType.None)
                this.AddAttribute("parametr_1_type", (double) item.Parametr1Type,
                    itemName + " " + item.Parametr1Type.ToString());
            if (item.Parametr1Value != (short) 0)
                this.AddAttribute("parametr_1_value", (double) item.Parametr1Value, itemName);
            if (item.Parametr2Type != Asda2ItemBonusType.None)
                this.AddAttribute("parametr_2_type", (double) item.Parametr2Type,
                    itemName + " " + item.Parametr2Type.ToString());
            if (item.Parametr2Value != (short) 0)
                this.AddAttribute("parametr_2_value", (double) item.Parametr2Value, itemName);
            if (item.Parametr3Type != Asda2ItemBonusType.None)
                this.AddAttribute("parametr_3_type", (double) item.Parametr3Type,
                    itemName + " " + item.Parametr3Type.ToString());
            if (item.Parametr3Value != (short) 0)
                this.AddAttribute("parametr_3_value", (double) item.Parametr3Value, itemName);
            if (item.Parametr4Type != Asda2ItemBonusType.None)
                this.AddAttribute("parametr_4_type", (double) item.Parametr4Type,
                    itemName + " " + item.Parametr4Type.ToString());
            if (item.Parametr4Value != (short) 0)
                this.AddAttribute("parametr_4_value", (double) item.Parametr4Value, itemName);
            if (item.Parametr5Type != Asda2ItemBonusType.None)
                this.AddAttribute("parametr_5_type", (double) item.Parametr5Type,
                    itemName + " " + item.Parametr5Type.ToString());
            if (item.Parametr5Value != (short) 0)
                this.AddAttribute("parametr_5_value", (double) item.Parametr5Value, itemName);
            return this;
        }

        public LogHelperEntry AddReference(LogHelperEntry lgDelete)
        {
            if (lgDelete != null)
                this._referenceEntries.Add(lgDelete);
            return this;
        }
    }
}