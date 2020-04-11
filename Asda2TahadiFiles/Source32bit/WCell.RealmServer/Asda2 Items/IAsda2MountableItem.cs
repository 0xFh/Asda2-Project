using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    public interface IAsda2MountableItem
    {
        Asda2ItemTemplate Template { get; }

        bool IsEquipped { get; }

        Asda2InventoryError CheckEquip(Character owner);
    }
}