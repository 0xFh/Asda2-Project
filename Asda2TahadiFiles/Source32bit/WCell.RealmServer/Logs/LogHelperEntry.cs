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
      _title = title;
      _sourceType = sourceType;
      _triggerId = triggerId;
    }

    public LogHelperEntry AddAttribute(string title, double value, string message = "")
    {
      _attributes.Add(new LogAttribute
      {
        Message = message,
        Title = title,
        Value = value
      });
      return this;
    }

    public LogHelperEntry Write()
    {
      Log.Write(_sourceType, _triggerId, _title, _attributes,
        _referenceEntries, SetRecord);
      return this;
    }

    private void SetRecord(LogEntryRecord record)
    {
      Record = record;
    }

    public LogHelperEntry AddItemAttributes(Asda2Item item, string itemName = "")
    {
      if(item == null)
        return this;
      AddAttribute("item_id", item.ItemId, item.Name + itemName);
      AddAttribute("is_soulbound", item.IsSoulbound ? 1.0 : 0.0, itemName);
      AddAttribute("inventory_type", (double) item.InventoryType,
        itemName + " " + item.InventoryType);
      AddAttribute("slot", item.Slot, itemName);
      if(item.Template.IsStackable)
        AddAttribute("amount", item.Amount, itemName);
      if(item.Enchant != 0)
        AddAttribute("enchant", item.Enchant, itemName);
      if(item.Soul1Id != 0)
        AddAttribute("sowel_1_id", item.Soul1Id, itemName);
      if(item.Soul2Id != 0)
        AddAttribute("sowel_2_id", item.Soul2Id, itemName);
      if(item.Soul3Id != 0)
        AddAttribute("sowel_3_id", item.Soul3Id, itemName);
      if(item.Soul4Id != 0)
        AddAttribute("sowel_4_id", item.Soul4Id, itemName);
      if(item.Parametr1Type != Asda2ItemBonusType.None)
        AddAttribute("parametr_1_type", (double) item.Parametr1Type,
          itemName + " " + item.Parametr1Type);
      if(item.Parametr1Value != 0)
        AddAttribute("parametr_1_value", item.Parametr1Value, itemName);
      if(item.Parametr2Type != Asda2ItemBonusType.None)
        AddAttribute("parametr_2_type", (double) item.Parametr2Type,
          itemName + " " + item.Parametr2Type);
      if(item.Parametr2Value != 0)
        AddAttribute("parametr_2_value", item.Parametr2Value, itemName);
      if(item.Parametr3Type != Asda2ItemBonusType.None)
        AddAttribute("parametr_3_type", (double) item.Parametr3Type,
          itemName + " " + item.Parametr3Type);
      if(item.Parametr3Value != 0)
        AddAttribute("parametr_3_value", item.Parametr3Value, itemName);
      if(item.Parametr4Type != Asda2ItemBonusType.None)
        AddAttribute("parametr_4_type", (double) item.Parametr4Type,
          itemName + " " + item.Parametr4Type);
      if(item.Parametr4Value != 0)
        AddAttribute("parametr_4_value", item.Parametr4Value, itemName);
      if(item.Parametr5Type != Asda2ItemBonusType.None)
        AddAttribute("parametr_5_type", (double) item.Parametr5Type,
          itemName + " " + item.Parametr5Type);
      if(item.Parametr5Value != 0)
        AddAttribute("parametr_5_value", item.Parametr5Value, itemName);
      return this;
    }

    public LogHelperEntry AddReference(LogHelperEntry lgDelete)
    {
      if(lgDelete != null)
        _referenceEntries.Add(lgDelete);
      return this;
    }
  }
}