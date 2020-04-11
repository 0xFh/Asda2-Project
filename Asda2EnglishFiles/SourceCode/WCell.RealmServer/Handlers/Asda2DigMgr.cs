using System.Collections.Generic;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public class Asda2DigMgr
    {
        public static Dictionary<byte, MineTableRecord> MapDiggingTemplates = new Dictionary<byte, MineTableRecord>();

        public static Dictionary<byte, MineTableRecord> PremiumMapDiggingTemplates =
            new Dictionary<byte, MineTableRecord>();

        public static void ProcessDig(IRealmClient client)
        {
            if (client.ActiveCharacter == null)
                return;
            Asda2Item mainWeapon = client.ActiveCharacter.MainWeapon as Asda2Item;
            if (mainWeapon == null)
                return;
            Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[10];
            bool flag = asda2Item != null && asda2Item.Category == Asda2ItemCategory.DigOil;
            if (flag)
                --asda2Item.Amount;
            if (flag)
            {
                AchievementProgressRecord progressRecord1 =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(92U);
                AchievementProgressRecord progressRecord2 =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(91U);
                ++progressRecord1.Counter;
                if (progressRecord1.Counter >= 1000U || progressRecord2.Counter >= 1000U)
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Automatic225);
                progressRecord1.SaveAndFlush();
            }

            if (Utility.Random(0, 100000) > CharacterFormulas.CalculateDiggingChance(mainWeapon.Template.ValueOnUse,
                    client.ActiveCharacter.SoulmateRecord == null
                        ? (byte) 0
                        : client.ActiveCharacter.SoulmateRecord.FriendShipPoints, client.ActiveCharacter.Asda2Luck) &&
                (flag
                    ? (client.ActiveCharacter.MapId < (MapId) Asda2DigMgr.PremiumMapDiggingTemplates.Count ? 1 : 0)
                    : (client.ActiveCharacter.MapId < (MapId) Asda2DigMgr.MapDiggingTemplates.Count ? 1 : 0)) != 0)
            {
                Asda2DiggingHandler.SendDigEndedResponse(client, true, asda2Item);
                MineTableRecord mineTableRecord =
                    flag
                        ? Asda2DigMgr.PremiumMapDiggingTemplates[(byte) client.ActiveCharacter.MapId]
                        : Asda2DigMgr.MapDiggingTemplates[(byte) client.ActiveCharacter.MapId];
                int randomItem = mineTableRecord.GetRandomItem();
                Asda2NPCLoot asda2NpcLoot = new Asda2NPCLoot();
                Asda2ItemTemplate templ = Asda2ItemMgr.GetTemplate(randomItem) ?? Asda2ItemMgr.GetTemplate(20622);
                asda2NpcLoot.Items = new Asda2LootItem[1]
                {
                    new Asda2LootItem(templ, 1, 0U)
                    {
                        Loot = (Asda2Loot) asda2NpcLoot
                    }
                };
                asda2NpcLoot.Lootable = (IAsda2Lootable) client.ActiveCharacter;
                asda2NpcLoot.Looters.Add(new Asda2LooterEntry(client.ActiveCharacter));
                asda2NpcLoot.MonstrId = new short?((short) 22222);
                if ((int) templ.ItemId >= 33542 && 33601 <= (int) templ.ItemId)
                {
                    AchievementProgressRecord progressRecord =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(125U);
                    switch (++progressRecord.Counter)
                    {
                        case 250:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Astrological292);
                            break;
                        case 500:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Astrological292);
                            break;
                    }

                    progressRecord.SaveAndFlush();
                }

                if (templ.ItemId == Asda2ItemId.TreasureBox31407 || templ.ItemId == Asda2ItemId.GoldenTreasureBox31408)
                {
                    AchievementProgressRecord progressRecord1 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(126U);
                    switch (++progressRecord1.Counter)
                    {
                        case 25:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Treasure293);
                            break;
                        case 50:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Treasure293);
                            break;
                    }

                    progressRecord1.SaveAndFlush();
                    if (templ.ItemId == Asda2ItemId.GoldenTreasureBox31408)
                    {
                        AchievementProgressRecord progressRecord2 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord((uint) sbyte.MaxValue);
                        switch (++progressRecord2.Counter)
                        {
                            case 389:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Lucky295);
                                break;
                            case 777:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Lucky295);
                                break;
                        }

                        progressRecord2.SaveAndFlush();
                    }
                }

                client.ActiveCharacter.Map.SpawnLoot((Asda2Loot) asda2NpcLoot);
                client.ActiveCharacter.GainXp(
                    CharacterFormulas.CalcDiggingExp(client.ActiveCharacter.Level, mineTableRecord.MinLevel), "digging",
                    false);
                client.ActiveCharacter.GuildPoints += CharacterFormulas.DiggingGuildPoints;
            }
            else
                Asda2DiggingHandler.SendDigEndedResponse(client, false, asda2Item);

            client.ActiveCharacter.IsDigging = false;
            --client.ActiveCharacter.Stunned;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Digging system.")]
        public static void Init()
        {
            ContentMgr.Load<MineTableRecord>();
        }
    }
}