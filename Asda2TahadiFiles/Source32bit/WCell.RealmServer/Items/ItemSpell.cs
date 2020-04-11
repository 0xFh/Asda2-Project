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
    [NotPersistent]public Spell Spell;
    public SpellId Id;

    /// <summary>The index of this spell within the Template</summary>
    [NotPersistent]public uint Index;

    public ItemSpellTrigger Trigger;
    public short Charges;

    /// <summary>SpellCategory.dbc</summary>
    public uint CategoryId;

    public int Cooldown;
    public int CategoryCooldown;
    [NotPersistent]public bool HasCharges;

    public void FinalizeAfterLoad()
    {
      Spell = SpellHandler.Get(Id);
      HasCharges = Charges != 0;
    }

    public override string ToString()
    {
      return ToString(true);
    }

    public string ToString(bool inclTrigger)
    {
      List<string> collection = new List<string>(5);
      if(Charges != 0)
        collection.Add("Charges: " + Charges);
      if(Cooldown > 0)
        collection.Add("Cooldown: " + Cooldown);
      if(CategoryId > 0U)
        collection.Add("CategoryId: " + CategoryId);
      if(CategoryCooldown > 0)
        collection.Add("CategoryCooldown: " + CategoryCooldown);
      string str = string.Format(((int) Id) + " (" + Id + ")" +
                                 (collection.Count > 0
                                   ? " - " + collection.ToString(", ")
                                   : (object) ""));
      if(inclTrigger)
        str = str + "[" + Trigger + "]";
      return str;
    }
  }
}