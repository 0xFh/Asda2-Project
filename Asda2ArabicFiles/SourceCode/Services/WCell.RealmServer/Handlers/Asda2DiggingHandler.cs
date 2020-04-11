using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Handlers
{
    public class Asda2DigMgr
    {
        public static Dictionary<byte, MineTableRecord> MapDiggingTemplates = new Dictionary<byte, MineTableRecord>();
        public static Dictionary<byte, MineTableRecord> PremiumMapDiggingTemplates = new Dictionary<byte, MineTableRecord>();
        public static void ProcessDig(IRealmClient client)
        {
            if (client.ActiveCharacter == null)
                return;
            var showel = client.ActiveCharacter.MainWeapon as Asda2Item;
            if (showel == null)
                return;
            var oilItem = client.ActiveCharacter.Asda2Inventory.Equipment[10];

            var isUseOil = oilItem != null && oilItem.Category == Asda2ItemCategory.DigOil;
            if (isUseOil)
                oilItem.Amount--;
            var chance = CharacterFormulas.CalculateDiggingChance(showel.Template.ValueOnUse, (byte)(client.ActiveCharacter.SoulmateRecord == null ? 0 : client.ActiveCharacter.SoulmateRecord.FriendShipPoints), client.ActiveCharacter.Asda2Luck);
            var rnd = Utility.Random(0, 100000);
            if (rnd > chance && (isUseOil ? client.ActiveCharacter.MapId < (MapId)PremiumMapDiggingTemplates.Count : client.ActiveCharacter.MapId < (MapId)MapDiggingTemplates.Count))
            {
                //dig ok
                Asda2DiggingHandler.SendDigEndedResponse(client, true, oilItem);
                var templ = isUseOil
                                 ? PremiumMapDiggingTemplates[(byte)client.ActiveCharacter.MapId]
                                 : MapDiggingTemplates[(byte)client.ActiveCharacter.MapId];
                var itemId = templ.GetRandomItem();
                var loot = new Asda2NPCLoot();
                var itemTempl = Asda2ItemMgr.GetTemplate(itemId) ?? Asda2ItemMgr.GetTemplate(20622);
                loot.Items = new[] { new Asda2LootItem(itemTempl, 1, 0) { Loot = loot } };
                loot.Lootable = client.ActiveCharacter;
                loot.Looters.Add(new Asda2LooterEntry(client.ActiveCharacter));
                loot.MonstrId = 22222;
                client.ActiveCharacter.Map.SpawnLoot(loot);
                client.ActiveCharacter.GainXp(CharacterFormulas.CalcDiggingExp(client.ActiveCharacter.Level, templ.MinLevel), "digging");
                client.ActiveCharacter.GuildPoints += CharacterFormulas.DiggingGuildPoints;

                Asda2TitleChecker.OnSuccessDig(client.ActiveCharacter, itemId, itemTempl.Quality, client);
            }
            else
            {
                // dig fail
                Asda2DiggingHandler.SendDigEndedResponse(client, false, oilItem);
            }

            client.ActiveCharacter.IsDigging = false;
            client.ActiveCharacter.Stunned--;
        }

        [Initialization(InitializationPass.Tenth, ("Digging system."))]
        public static void Init()
        {
            ContentMgr.Load<MineTableRecord>();
        }

    }
    [DataHolder]
    public class MineTableRecord : IDataHolder
    {
        public int Id { get; set; }
        public int MapId { get; set; }
        public int IsPremium { get; set; }
        public int DigTime { get; set; }
        public int MinLevel { get; set; }
        [Persistent(Length = 50)]
        public int[] ItemIds { get; set; }
        [Persistent(Length = 50)]
        public int[] Chances { get; set; }
        public void FinalizeDataHolder()
        {
            if (IsPremium == 1)
                Asda2DigMgr.PremiumMapDiggingTemplates.Add((byte)MapId, this);
            else
                Asda2DigMgr.MapDiggingTemplates.Add((byte)MapId, this);
        }
        public int GetRandomItem()
        {
            var rnd = Utility.Random(0, 100000);
            var curChance = 0;
            for (int i = 0; i < 50; i++)
            {
                curChance += Chances[i];
                if (curChance >= rnd)
                    return ItemIds[i];
            }
            return 20622;
        }
    }
    public static class Asda2DiggingHandler
    {
        [PacketHandler(RealmServerOpCode.StartDig)]//5428
        public static void StartDigRequest(IRealmClient client, RealmPacketIn packet)
        {

            //var accId = packet.ReadInt32();//default : 340701Len : 4
            if (client.ActiveCharacter.IsDigging)
            {
                client.ActiveCharacter.SendSystemMessage("You already digging.");
                SendStartDigResponseResponse(client, Asda2DigResult.DiggingFail);
                return;
            }
            if (client.ActiveCharacter.IsInCombat || client.ActiveCharacter.IsMoving)
            {
                client.ActiveCharacter.SendSystemMessage("You can't dig while moving or fighting.");
                SendStartDigResponseResponse(client, Asda2DigResult.DiggingFail);
                return;
            }
            var showel = client.ActiveCharacter.MainWeapon as Asda2Item;
            if (showel == null || showel.Category != Asda2ItemCategory.Showel)
            {
                SendStartDigResponseResponse(client, Asda2DigResult.YouHaveNoShowel);
                return;
            }
            var oilItem = client.ActiveCharacter.Asda2Inventory.Equipment[10];
            var isUseOil = oilItem != null && oilItem.Category == Asda2ItemCategory.DigOil;
            if (!(isUseOil ? client.ActiveCharacter.MapId < (MapId)Asda2DigMgr.PremiumMapDiggingTemplates.Count : client.ActiveCharacter.MapId < (MapId)Asda2DigMgr.MapDiggingTemplates.Count))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to dig in unknown location : " + client.ActiveCharacter.MapId, 10);
                SendStartDigResponseResponse(client, Asda2DigResult.DiggingFail);
                return;
            }

            if (isUseOil)
            {
                Asda2TitleChecker.OnUseAutoFishDig(client.ActiveCharacter);
            }
            var templ = isUseOil
                                 ? Asda2DigMgr.PremiumMapDiggingTemplates[(byte)client.ActiveCharacter.MapId]
                                 : Asda2DigMgr.MapDiggingTemplates[(byte)client.ActiveCharacter.MapId];
            if (templ.MinLevel > client.ActiveCharacter.Level)
            {
                SendStartDigResponseResponse(client, Asda2DigResult.YouUnableToDigInThisLocationDueLowLevel);
                return;
            }
            client.ActiveCharacter.CancelAllActions();
            client.ActiveCharacter.IsDigging = true;
            client.ActiveCharacter.Stunned++;
            client.ActiveCharacter.Map.CallDelayed(6000, () => Asda2DigMgr.ProcessDig(client));
            Asda2CharacterHandler.SendEmoteResponse(client.ActiveCharacter, 110, 0, 0, 0);
            SendStartDigResponseResponse(client, Asda2DigResult.Ok);

        }

        public static void SendStartDigResponseResponse(IRealmClient client, Asda2DigResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.StartDigResponse))//5429
            {
                packet.WriteByte((byte)status);//{status}default value : 3 Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendDigEndedResponse(IRealmClient client, bool success, Asda2Item item = null)
        {
            if (client == null || client.ActiveCharacter == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.DigEnded))//5431
            {
                packet.WriteByte(success ? 1 : 0);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 32 Len : 2
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                client.ActiveCharacter.SendPacketToArea(packet);
            }
        }

    }
    public enum Asda2DigResult
    {
        DiggingFail = 0,
        Ok = 1,
        YouHaveNoShowel = 2,
        YouCantDoItInTown = 3,
        YouCanDigAfterEquipingAshowel = 4,
        TheDurabilitiOfYourShowelIs0 = 5,
        OilEnded = 6,
        YouUnableToDigInThisLocationDueLowLevel = 7,

    }
}