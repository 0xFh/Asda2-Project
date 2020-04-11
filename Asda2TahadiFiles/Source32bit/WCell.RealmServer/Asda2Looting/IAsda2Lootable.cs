using WCell.Constants.Looting;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Asda2Looting
{
    public interface IAsda2Lootable
    {
        /// <summary>The Loot that is currently lootable</summary>
        Asda2Loot Loot { get; set; }

        bool UseGroupLoot { get; }

        /// <summary>The amount of money that can be looted.</summary>
        uint LootMoney { get; }

        /// <summary>
        /// The LootId for the given <see cref="T:WCell.Constants.Looting.LootEntryType" /> of this Lootable object.
        /// </summary>
        uint GetLootId(Asda2LootEntryType type);

        /// <summary>
        /// Is called after this Lootable has been looted empty or its Loot expired
        /// </summary>
        void OnFinishedLooting();
    }
}