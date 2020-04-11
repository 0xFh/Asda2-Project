using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2_Items
{
    public class Asda2CraftingHandler
    {
        private static readonly byte[] stab28 = new byte[41]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab11 = new byte[580]
        {
            (byte) 0,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly List<Asda2Item> NullList = new List<Asda2Item>();

        [PacketHandler(RealmServerOpCode.LearnRecipe)]
        public static void LearnRecipeRequest(IRealmClient client, RealmPacketIn packet)
        {
            short slot = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.LearnRecipe(slot);
        }

        public static void SendRecipeLeadnedResponse(IRealmClient client, bool success, short recipeId,
            Asda2Item recipeItem)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RecipeLeadned))
            {
                packet.WriteByte(success ? 1 : 0);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(recipeItem == null ? 0 : recipeItem.ItemId);
                packet.WriteByte(2);
                packet.WriteInt16(recipeItem == null ? 0 : (int) recipeItem.Slot);
                packet.WriteInt16(recipeItem == null ? 0 : (recipeItem.IsDeleted ? -1 : 0));
                packet.WriteInt32(recipeItem == null ? 0 : recipeItem.Amount);
                packet.WriteByte(0);
                packet.WriteInt16(recipeItem == null ? 0 : recipeItem.Amount);
                packet.WriteSkip(Asda2CraftingHandler.stab28);
                packet.WriteInt16(recipeId);
                client.Send(packet, true);
            }
        }

        public static void SendLeanedRecipesResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.LeanedRecipes))
            {
                packet.WriteInt16(client.ActiveCharacter.LearnedRecipesCount);
                packet.WriteInt16(1000);
                for (int index = 0; index < 576; ++index)
                    packet.WriteByte(client.ActiveCharacter.LearnedRecipes.GetBit(index) ? 1 : 0);
                packet.WriteInt32(0);
                packet.WriteByte(client.ActiveCharacter.Record.CraftingLevel);
                packet.WriteInt32(0);
                packet.WriteInt16((short) client.ActiveCharacter.Record.CraftingExp);
                packet.WriteInt16(0);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.Craft)]
        public static void CraftRequest(IRealmClient client, RealmPacketIn packet)
        {
            short recId = packet.ReadInt16();
            List<Asda2Item> materials;
            Asda2Item craftedItem = client.ActiveCharacter.Asda2Inventory.TryCraftItem(recId, out materials);
            if (craftedItem != null)
            {
                int num = (int) (byte) craftedItem.Template.Quality + 1;
                if (!craftedItem.IsArmor && !craftedItem.IsWeapon && !craftedItem.IsAccessory)
                    num = 1;
                Asda2CraftingHandler.SendCraftedResponse(client, true, (byte) num, craftedItem, materials);
            }
            else
                Asda2CraftingHandler.SendCraftedResponse(client, false, (byte) 0, (Asda2Item) null,
                    Asda2CraftingHandler.NullList);
        }

        public static void SendCraftedResponse(IRealmClient client, bool sucess, byte craftTimes, Asda2Item craftedItem,
            List<Asda2Item> craftMaterials)
        {
            if (craftedItem != null)
            {
                AchievementProgressRecord progressRecord1 =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(108U);
                switch (++progressRecord1.Counter)
                {
                    case 500:
                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Crafty267);
                        break;
                    case 1000:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Crafty267);
                        break;
                }

                progressRecord1.SaveAndFlush();
                if (craftedItem.Template.Quality == Asda2ItemQuality.Purple)
                {
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(111U);
                    switch (++progressRecord2.Counter)
                    {
                        case 5:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rare270);
                            break;
                        case 10:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Rare270);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                }

                if (craftedItem.Template.Quality == Asda2ItemQuality.Green)
                {
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(112U);
                    switch (++progressRecord2.Counter)
                    {
                        case 1:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Hero271);
                            break;
                        case 3:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Hero271);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                }

                if (craftedItem.IsWeapon)
                {
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(113U);
                    switch (++progressRecord2.Counter)
                    {
                        case 25:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Weapon272);
                            break;
                        case 50:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Weapon272);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                }

                if (craftedItem.IsArmor)
                {
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(114U);
                    switch (++progressRecord2.Counter)
                    {
                        case 25:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Armor273);
                            break;
                        case 50:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Armor273);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                }

                if (craftedItem.Template.EquipmentSlot == Asda2EquipmentSlots.LeftRing ||
                    craftedItem.Template.EquipmentSlot == Asda2EquipmentSlots.RightRing ||
                    craftedItem.Template.EquipmentSlot == Asda2EquipmentSlots.Nackles)
                {
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(115U);
                    switch (++progressRecord2.Counter)
                    {
                        case 25:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Jewel274);
                            break;
                        case 50:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Jewel274);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                }
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.Crafted))
            {
                packet.WriteByte(sucess ? 1 : 0);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteByte(client.ActiveCharacter.Record.CraftingLevel);
                packet.WriteInt32(0);
                packet.WriteInt16((short) client.ActiveCharacter.Record.CraftingExp);
                packet.WriteInt16(craftTimes);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, craftedItem, false);
                for (int index = 0; index < 7; ++index)
                {
                    Asda2Item asda2Item = craftMaterials.Count <= index ? (Asda2Item) null : craftMaterials[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, false);
            }
        }
    }
}