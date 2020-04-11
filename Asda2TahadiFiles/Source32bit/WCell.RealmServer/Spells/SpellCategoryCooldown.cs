using System;

namespace WCell.RealmServer.Spells
{
  public class SpellCategoryCooldown : ISpellCategoryCooldown, ICooldown
  {
    public DateTime Until { get; set; }

    public uint SpellId { get; set; }

    public uint CategoryId { get; set; }

    public uint ItemId { get; set; }

    public IConsistentCooldown AsConsistent()
    {
      return new PersistentSpellCategoryCooldown
      {
        Until = Until,
        CategoryId = CategoryId,
        ItemId = ItemId
      };
    }
  }
}