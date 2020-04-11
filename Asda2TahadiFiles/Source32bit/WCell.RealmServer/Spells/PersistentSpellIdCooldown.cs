using Castle.ActiveRecord;
using System;

namespace WCell.RealmServer.Spells
{
  [ActiveRecord("SpellIdCooldown", Access = PropertyAccess.Property)]
  public class PersistentSpellIdCooldown : ActiveRecordBase<PersistentSpellIdCooldown>, ISpellIdCooldown,
    IConsistentCooldown, ICooldown
  {
    [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _spellId;

    [Field("ItemId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _itemId;

    [Field("CharId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int _charId;

    public static PersistentSpellIdCooldown[] LoadIdCooldownsFor(uint lowId)
    {
      return FindAllByProperty("_charId", (int) lowId);
    }

    [PrimaryKey(PrimaryKeyType.Increment)]
    private long Id { get; set; }

    public uint CharId
    {
      get { return (uint) _charId; }
      set { _charId = (int) value; }
    }

    [Property]
    public DateTime Until { get; set; }

    public uint SpellId
    {
      get { return (uint) _spellId; }
      set { _spellId = (int) value; }
    }

    public uint ItemId
    {
      get { return (uint) _itemId; }
      set { _itemId = (int) value; }
    }

    public IConsistentCooldown AsConsistent()
    {
      return this;
    }
  }
}