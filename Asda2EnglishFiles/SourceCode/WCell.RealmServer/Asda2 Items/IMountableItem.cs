using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
    /// <summary>ItemTemplate or Item</summary>
    public interface IMountableItem
    {
        ItemTemplate Template { get; }

        ItemEnchantment[] Enchantments { get; }

        bool IsEquipped { get; }

        InventoryError CheckEquip(Character owner);
    }
}