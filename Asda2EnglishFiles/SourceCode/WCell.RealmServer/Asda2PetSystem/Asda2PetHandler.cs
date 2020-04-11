using System;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Asda2PetSystem
{
    public static class Asda2PetHandler
    {
        private static readonly byte[] stub26 = new byte[56]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
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

        private static readonly byte[] unk13 = new byte[3];
        private static readonly byte[] stab25 = new byte[1];

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

        [PacketHandler(RealmServerOpCode.ChangePetName)]
        public static void ChangePetNameRequest(IRealmClient client, RealmPacketIn packet)
        {
            int key = packet.ReadInt32();
            string s = packet.ReadAsdaString(16, Locale.Start);
            if (!Asda2EncodingHelper.IsPrueEnglish(s))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("pet name");
                Asda2PetHandler.SendPetNameChangedResponse(client, Asda2PetNamehangeResult.AbnormalPetInfo,
                    (Asda2PetRecord) null, (Asda2Item) null);
            }
            else
            {
                ++packet.Position;
                short slotInq = packet.ReadInt16();
                if (!client.ActiveCharacter.OwnedPets.ContainsKey(key))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to summon not existing pet.", 20);
                    Asda2PetHandler.SendPetNameChangedResponse(client, Asda2PetNamehangeResult.AbnormalPetInfo,
                        (Asda2PetRecord) null, (Asda2Item) null);
                }
                else
                {
                    Asda2PetRecord ownedPet = client.ActiveCharacter.OwnedPets[key];
                    Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq);
                    if (!ownedPet.CanChangeName)
                    {
                        if (shopShopItem == null)
                        {
                            Asda2PetHandler.SendPetNameChangedResponse(client,
                                Asda2PetNamehangeResult.YouMustHavePremiumItemToChangePetName, ownedPet,
                                (Asda2Item) null);
                            return;
                        }

                        shopShopItem.ModAmount(-1);
                    }

                    ownedPet.Name = s;
                    ownedPet.CanChangeName = false;
                    Asda2PetHandler.SendPetNameChangedResponse(client, Asda2PetNamehangeResult.Ok, ownedPet,
                        shopShopItem);
                    GlobalHandler.UpdateCharacterPetInfoToArea(client.ActiveCharacter);
                }
            }
        }

        public static void SendPetNameChangedResponse(IRealmClient client, Asda2PetNamehangeResult status,
            Asda2PetRecord pet, Asda2Item changeNameItem)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetNameChanged))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(pet == null ? 0 : pet.Guid);
                packet.WriteFixedAsciiString(pet == null ? "" : pet.Name, 16, Locale.Start);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, changeNameItem, false);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.SummonPet)]
        public static void SummonPetRequest(IRealmClient client, RealmPacketIn packet)
        {
            int key = packet.ReadInt32();
            if (!client.ActiveCharacter.OwnedPets.ContainsKey(key))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to summon not existing pet.", 20);
                Asda2PetHandler.SendPetSummonOrUnSummondResponse(client, (Asda2PetRecord) null);
            }
            else
            {
                Asda2PetRecord ownedPet = client.ActiveCharacter.OwnedPets[key];
                if (ownedPet.HungerPrc == (byte) 0)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to summon dead pet.", 50);
                    Asda2PetHandler.SendPetSummonOrUnSummondResponse(client, (Asda2PetRecord) null);
                }
                else if (ownedPet.Template.MinimumUsingLevel > client.ActiveCharacter.Level)
                {
                    client.ActiveCharacter.SendInfoMsg("You cant summon pet with grade higher than your level.");
                    Asda2PetHandler.SendPetSummonOrUnSummondResponse(client, (Asda2PetRecord) null);
                }
                else if (client.ActiveCharacter.Asda2Pet != null)
                {
                    Asda2PetRecord asda2Pet = client.ActiveCharacter.Asda2Pet;
                    client.ActiveCharacter.Asda2Pet.RemoveStatsFromOwner();
                    client.ActiveCharacter.Asda2Pet = (Asda2PetRecord) null;
                    GlobalHandler.UpdateCharacterPetInfoToArea(client.ActiveCharacter);
                    Asda2PetHandler.SendPetSummonOrUnSummondResponse(client, asda2Pet);
                    Asda2CharacterHandler.SendUpdateStatsResponse(client);
                    Asda2CharacterHandler.SendUpdateStatsOneResponse(client);
                }
                else
                {
                    client.ActiveCharacter.Asda2Pet = ownedPet;
                    client.ActiveCharacter.Asda2Pet.AddStatsToOwner();
                    client.ActiveCharacter.LastPetExpGainTime = (uint) (Environment.TickCount + 60000);
                    GlobalHandler.UpdateCharacterPetInfoToArea(client.ActiveCharacter);
                    Asda2CharacterHandler.SendUpdateStatsResponse(client);
                    Asda2CharacterHandler.SendUpdateStatsOneResponse(client);
                    Asda2PetHandler.SendPetSummonOrUnSummondResponse(client, ownedPet);
                }
            }
        }

        public static void SendPetSummonOrUnSummondResponse(IRealmClient client, Asda2PetRecord pet)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetSummonOrUnSummond))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte(pet == null ? 0 : 1);
                packet.WriteInt32(pet == null ? 0 : pet.Guid);
                client.Send(packet, true);
            }
        }

        public static void SendUpdatePetExpResponse(IRealmClient client, Asda2PetRecord pet, bool levelUped = false)
        {
            if (pet == null || client.ActiveCharacter == null)
                return;
            if (pet.Level == (byte) 5)
                client.ActiveCharacter.Map.CallDelayed(500,
                    (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Mature368)));
            if ((int) pet.Level == (int) pet.MaxLevel)
            {
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(171U);
                switch (++progressRecord.Counter)
                {
                    case 5:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Trainer369)));
                        break;
                    case 10:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Trainer369)));
                        break;
                }

                progressRecord.SaveAndFlush();
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdatePetExp))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(pet.Guid);
                packet.WriteInt16(pet.Expirience);
                packet.WriteByte(levelUped ? 1 : 0);
                packet.WriteByte(pet.Level);
                packet.WriteByte(0);
                client.Send(packet, true);
            }
        }

        public static void SendUpdatePetHungerResponse(IRealmClient client, Asda2PetRecord pet)
        {
            if (!client.IsGameServerConnection || client.ActiveCharacter == null || pet == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdatePetHunger))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(pet.Guid);
                packet.WriteByte(pet.HungerPrc);
                packet.WriteInt16((short) pet.Stat1Type);
                packet.WriteInt32(pet.Stat1Value);
                packet.WriteInt16((short) pet.Stat2Type);
                packet.WriteInt32(pet.Stat2Value);
                packet.WriteInt16((short) pet.Stat3Type);
                packet.WriteInt32(pet.Stat3Value);
                client.Send(packet, true);
            }
        }

        public static void SendPetGoesSleepDueStarvationResponse(IRealmClient client, Asda2PetRecord pet)
        {
            if (client == null || client.ActiveCharacter == null || pet == null)
                return;
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(167U);
            switch (++progressRecord.Counter)
            {
                case 5:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Neglected364)));
                    break;
                case 10:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Neglected364)));
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetGoesSleepDueStarvation))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(pet.Guid);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.HatchEgg)]
        public static void HatchEggRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadByte();
            short num2 = packet.ReadInt16();
            int num3 = (int) packet.ReadByte();
            short slotEgg = packet.ReadInt16();
            byte invSupl = packet.ReadByte();
            short slotSupl = packet.ReadInt16();
            HatchEggStatus status = client.ActiveCharacter.Asda2Inventory.HatchEgg(num2, slotEgg, slotSupl);
            if (status != HatchEggStatus.Ok)
                Asda2PetHandler.SendEggHatchedResponse(client, status, (byte) 2, num2, (byte) 2, slotEgg, invSupl,
                    slotSupl);
            else
                Asda2PetHandler.SendEggHatchedResponse(client, status, (byte) 2, num2, (byte) 2, slotEgg, invSupl,
                    slotSupl);
        }

        public static void SendEggHatchedResponse(IRealmClient client, HatchEggStatus status, byte invInq = 0,
            short inqSlot = -1, byte invEgg = 0, short slotEgg = -1, byte invSupl = 0, short slotSupl = -1)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EggHatched))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte((byte) status);
                packet.WriteByte(invInq);
                packet.WriteInt16(inqSlot);
                packet.WriteByte(invEgg);
                packet.WriteInt16(slotEgg);
                packet.WriteByte(invSupl);
                packet.WriteInt16(slotSupl);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, false);
            }
        }

        public static void SendInitPetInfoOnLoginResponse(IRealmClient client, Asda2PetRecord pet)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.InitPetInfoOnLogin))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte(0);
                packet.WriteInt32(pet.Guid);
                packet.WriteInt16(pet.Id);
                packet.WriteFixedAsciiString(pet.Name, 16, Locale.Start);
                packet.WriteByte(pet.HungerPrc);
                packet.WriteByte(pet.Level);
                packet.WriteByte(pet.MaxLevel);
                packet.WriteInt16(pet.Expirience);
                packet.WriteByte(pet.Level);
                packet.WriteInt16((short) pet.Stat1Type);
                packet.WriteInt32(pet.Stat1Value);
                packet.WriteInt16((short) pet.Stat2Type);
                packet.WriteInt32(pet.Stat2Value);
                packet.WriteInt16((short) pet.Stat3Type);
                packet.WriteInt32(pet.Stat3Value);
                packet.WriteByte(client.ActiveCharacter.Asda2Pet == null
                    ? 0
                    : (client.ActiveCharacter.Asda2Pet.Guid == pet.Guid ? 1 : 0));
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ResurectPet)]
        public static void ResurectPetRequest(IRealmClient client, RealmPacketIn packet)
        {
            int key = packet.ReadInt32();
            int num = (int) packet.ReadByte();
            short slotInq = packet.ReadInt16();
            if (!client.ActiveCharacter.OwnedPets.ContainsKey(key))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to resurect not existing pet", 20);
            }
            else
            {
                Asda2Item regularItem = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
                if (regularItem == null)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Trying to resurect pet with not existint resurect item.", 10);
                }
                else
                {
                    Asda2PetRecord ownedPet = client.ActiveCharacter.OwnedPets[key];
                    if (regularItem.Category == Asda2ItemCategory.PetFoodMeat ||
                        regularItem.Category == Asda2ItemCategory.PetFoodVegetable ||
                        regularItem.Category == Asda2ItemCategory.PetFoodOil)
                    {
                        if (client.ActiveCharacter.Asda2Pet != null)
                        {
                            client.ActiveCharacter.Asda2Pet.Feed(regularItem.Template.ValueOnUse / 2);
                            regularItem.ModAmount(-1);
                            Asda2PetHandler.SendPetResurectedResponse(client, ownedPet, regularItem);
                        }
                        else
                            client.ActiveCharacter.SendInfoMsg("You must summon pet to feed.");
                    }
                    else if (regularItem.Category != Asda2ItemCategory.PetResurect)
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to resurect pet with not a resurect item.",
                            50);
                    else if (ownedPet.HungerPrc != (byte) 0)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to resurect alive pet.", 50);
                    }
                    else
                    {
                        ownedPet.HungerPrc = (byte) 5;
                        regularItem.ModAmount(-1);
                        Asda2PetHandler.SendPetResurectedResponse(client, ownedPet, regularItem);
                    }
                }
            }
        }

        public static void SendPetResurectedResponse(IRealmClient client, Asda2PetRecord pet, Asda2Item resurectPetItem)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(169U);
            switch (++progressRecord.Counter)
            {
                case 5:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Veterinarian365)));
                    break;
                case 10:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Veterinarian365)));
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetResurected))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(pet.Guid);
                packet.WriteInt32(resurectPetItem.ItemId);
                packet.WriteByte(2);
                packet.WriteInt16(resurectPetItem.Slot);
                packet.WriteInt16(resurectPetItem.IsDeleted ? -1 : 0);
                packet.WriteInt32(resurectPetItem.Amount);
                packet.WriteByte(0);
                packet.WriteInt16(resurectPetItem.Amount);
                packet.WriteSkip(Asda2PetHandler.stab28);
                packet.WriteByte(1);
                packet.WriteByte(pet.HungerPrc);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt16((short) pet.Stat1Type);
                packet.WriteInt32(pet.Stat1Value);
                packet.WriteInt16((short) pet.Stat2Type);
                packet.WriteInt32(pet.Stat2Value);
                packet.WriteInt16((short) pet.Stat2Type);
                packet.WriteInt32(pet.Stat2Value);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.RemovePet)]
        public static void RemovePetRequest(IRealmClient client, RealmPacketIn packet)
        {
            int key = packet.ReadInt32();
            if (!client.ActiveCharacter.OwnedPets.ContainsKey(key))
                client.ActiveCharacter.YouAreFuckingCheater("Trying delete not existing pet.", 10);
            else if (client.ActiveCharacter.Asda2Pet != null && client.ActiveCharacter.Asda2Pet.Guid == key)
            {
                client.ActiveCharacter.SendInfoMsg("You can't delete summoned pet.");
            }
            else
            {
                Asda2PetRecord ownedPet = client.ActiveCharacter.OwnedPets[key];
                client.ActiveCharacter.OwnedPets.Remove(key);
                Asda2PetHandler.SendPetRemovedResponse(client, ownedPet.Guid);
                ownedPet.DeleteLater();
            }
        }

        public static void SendPetRemovedResponse(IRealmClient client, int petGuid)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(168U);
            switch (++progressRecord.Counter)
            {
                case 5:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Abandoned363)));
                    break;
                case 10:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Abandoned363)));
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetRemoved))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(petGuid);
                packet.WriteByte(1);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.PetSyntes)]
        public static void PetSyntesRequest(IRealmClient client, RealmPacketIn packet)
        {
            bool flag = packet.ReadByte() == (byte) 1;
            int key1 = packet.ReadInt32();
            int key2 = packet.ReadInt32();
            packet.ReadInt32();
            int num1 = (int) packet.ReadByte();
            short slotInq1 = packet.ReadInt16();
            packet.ReadInt32();
            int num2 = (int) packet.ReadByte();
            short slotInq2 = packet.ReadInt16();
            packet.ReadInt32();
            int num3 = (int) packet.ReadByte();
            short slotInq3 = packet.ReadInt16();
            if (!client.ActiveCharacter.OwnedPets.ContainsKey(key1) ||
                !client.ActiveCharacter.OwnedPets.ContainsKey(key2))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tries to sysnes with not existing pets.", 20);
                Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.AbnormalPetInfo,
                    (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
            }
            else if (client.ActiveCharacter.Asda2Pet != null &&
                     (client.ActiveCharacter.Asda2Pet.Guid == key1 || client.ActiveCharacter.Asda2Pet.Guid == key2))
            {
                Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.CantUseCurrentlySummonedPet,
                    (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
            }
            else
            {
                Asda2Item regularItem1 = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq1);
                Asda2Item regularItem2 = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq2);
                Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq3);
                if (regularItem1 == null || regularItem2 == null && flag)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tries to sysnes with not existing rank or class item.",
                        20);
                    Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.AbnormalPetInfo,
                        (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                }
                else if (regularItem1.Category != Asda2ItemCategory.PetSynesisPotion ||
                         regularItem2 != null && regularItem2.Category != Asda2ItemCategory.PetPotion)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tries to sysnes with rank or class potion item with wrong category.", 50);
                    Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.AbnormalPetInfo,
                        (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                }
                else if (shopShopItem != null &&
                         shopShopItem.Category != Asda2ItemCategory.PetSynthesisSupplementOrPetLevelProtection)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tries to sysnes with supliment potion item with wrong category.", 50);
                    Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.AbnormalPetInfo,
                        (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                }
                else if ((int) regularItem1.RequiredLevel > client.ActiveCharacter.Level)
                {
                    Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.IncorrectSuplimentLevel,
                        (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                }
                else
                {
                    Asda2PetRecord ownedPet1 = client.ActiveCharacter.OwnedPets[key1];
                    Asda2PetRecord ownedPet2 = client.ActiveCharacter.OwnedPets[key2];
                    if (ownedPet1.Level < (byte) 5 ||
                        (int) ownedPet2.Level + (shopShopItem == null ? 0 : 2) < (flag ? 5 : 3))
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Tries to sysnes with low level pets.", 50);
                        Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.LowPetLevel,
                            (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                    }
                    else
                    {
                        int rarity = flag
                            ? CharacterFormulas.CalcResultSyntesPetRarity(ownedPet1.Template.Rarity,
                                ownedPet2.Template.Rarity)
                            : CharacterFormulas.CalcResultEvolutionPetRarity(ownedPet1.Template.Rarity,
                                ownedPet2.Template.Rarity);
                        PetTemplate petTemplate = flag
                            ? Asda2PetMgr.PetTemplatesByRankAndRarity[regularItem1.Template.ValueOnUse][rarity].Values
                                .ToArray<PetTemplate>()[
                                    Utility.Random(0,
                                        Asda2PetMgr.PetTemplatesByRankAndRarity[regularItem1.Template.ValueOnUse][
                                            rarity].Count - 1)]
                            : ownedPet1.Template.GetEvolutionTemplate(rarity, regularItem1.Template.ValueOnUse);
                        if (petTemplate == null)
                        {
                            if (flag)
                                client.ActiveCharacter.YouAreFuckingCheater(
                                    string.Format(
                                        "Tries to sysnes, but result pet template was null. Please report to developers. Rarity {0} Rank {1}.",
                                        (object) rarity, (object) regularItem1.Template.ValueOnUse), 0);
                            else
                                client.ActiveCharacter.YouAreFuckingCheater(
                                    string.Format(
                                        "Tries to evalute, but result pet template was null. Please report to developers. Rarity {0} Rank {1}.",
                                        (object) rarity, (object) regularItem1.Template.ValueOnUse), 0);
                            Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.LowPetLevel,
                                (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, 0, 0);
                        }
                        else
                        {
                            Asda2PetRecord resultPet = client.ActiveCharacter.AddAsda2Pet(petTemplate, true);
                            regularItem1.ModAmount(-1);
                            if (regularItem2 != null)
                                regularItem2.ModAmount(-1);
                            if (shopShopItem != null)
                                shopShopItem.ModAmount(-1);
                            client.ActiveCharacter.OwnedPets.Remove(key1);
                            client.ActiveCharacter.OwnedPets.Remove(key2);
                            Asda2PetHandler.SendPetSyntesResultResponse(client, PetSynethisResult.Ok, resultPet,
                                regularItem1, regularItem2, shopShopItem, ownedPet1.Guid, ownedPet2.Guid);
                            ownedPet1.DeleteLater();
                            ownedPet2.DeleteLater();
                        }
                    }
                }
            }
        }

        public static void SendPetSyntesResultResponse(IRealmClient client, PetSynethisResult result,
            Asda2PetRecord resultPet = null, Asda2Item rankPotion = null, Asda2Item classSupliment = null,
            Asda2Item syntesSupliment = null, int removedPet1Guid = 0, int removedPet2Guid = 0)
        {
            client.ActiveCharacter.Map.CallDelayed(500,
                (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Evolved367)));
            Asda2Item[] asda2ItemArray = new Asda2Item[3]
            {
                rankPotion,
                classSupliment,
                syntesSupliment
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetSyntesResult))
            {
                packet.WriteByte((byte) result);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(resultPet == null ? 0 : resultPet.Guid);
                packet.WriteInt16(resultPet == null ? 0 : (int) resultPet.Id);
                packet.WriteFixedAsciiString(resultPet == null ? "" : resultPet.Name, 16, Locale.Start);
                packet.WriteByte(resultPet == null ? 0 : (int) resultPet.HungerPrc);
                packet.WriteByte(resultPet == null ? 0 : (int) resultPet.Level);
                packet.WriteByte(resultPet == null ? 0 : (int) resultPet.MaxLevel);
                packet.WriteSkip(Asda2PetHandler.unk13);
                packet.WriteInt16(resultPet == null ? (byte) 0 : (byte) resultPet.Stat1Type);
                packet.WriteInt32(resultPet == null ? 0 : resultPet.Stat1Value);
                packet.WriteInt16(resultPet == null ? (byte) 0 : (byte) resultPet.Stat2Type);
                packet.WriteInt32(resultPet == null ? 0 : resultPet.Stat2Value);
                packet.WriteInt16(resultPet == null ? (byte) 0 : (byte) resultPet.Stat3Type);
                packet.WriteInt32(resultPet == null ? 0 : resultPet.Stat3Value);
                packet.WriteInt32(removedPet1Guid);
                packet.WriteInt32(removedPet2Guid);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                for (int index = 0; index < 3; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Slot);
                    packet.WriteInt16(asda2Item == null ? -1 : (asda2Item.IsDeleted ? -1 : 0));
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteSkip(Asda2PetHandler.stab25);
                    packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteSkip(Asda2PetHandler.stab28);
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.BreakPetLvlLimit)]
        public static void BreakPetLvlLimitRequest(IRealmClient client, RealmPacketIn packet)
        {
            int key = packet.ReadInt32();
            packet.ReadInt32();
            int num1 = (int) packet.ReadByte();
            short slotInq1 = packet.ReadInt16();
            packet.ReadInt32();
            int num2 = (int) packet.ReadByte();
            short slotInq2 = packet.ReadInt16();
            if (!client.ActiveCharacter.OwnedPets.ContainsKey(key))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to break pet lvl limit not existing pet.", 20);
                Asda2PetHandler.SendPetLevelLimitBreakedResponse(client, PetLimitBreakStatus.AbnormalPetInfo,
                    (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null);
            }
            else
            {
                Asda2Item regularItem = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq1);
                if (regularItem == null)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Trying to break pet lvl limit with not existint break lvl item.", 10);
                    Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                        PetLimitBreakStatus.PetLimitPotionInfoAbnormal, (Asda2PetRecord) null, (Asda2Item) null,
                        (Asda2Item) null);
                }
                else if (regularItem.Category != Asda2ItemCategory.PetLevelBreakPotion)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to resurect pet with not a resurect item.", 50);
                    Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                        PetLimitBreakStatus.PetLimitPotionInfoAbnormal, (Asda2PetRecord) null, (Asda2Item) null,
                        (Asda2Item) null);
                }
                else
                {
                    Asda2PetRecord ownedPet = client.ActiveCharacter.OwnedPets[key];
                    if (regularItem.Template.ValueOnUse != (int) ownedPet.Level)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to break pet level limit wrong level item.",
                            50);
                        Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                            PetLimitBreakStatus.TheLevelOfLimitBreakItemIsTooLow, (Asda2PetRecord) null,
                            (Asda2Item) null, (Asda2Item) null);
                    }
                    else if (!ownedPet.IsMaxExpirience)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Trying to break pet level limit with not 100prc exp.", 50);
                        Asda2PetHandler.SendPetLevelLimitBreakedResponse(client, PetLimitBreakStatus.Not100PrcExp,
                            (Asda2PetRecord) null, (Asda2Item) null, (Asda2Item) null);
                    }
                    else if ((int) ownedPet.Level != (int) ownedPet.MaxLevel)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Trying to break pet level limit with not maxed level.", 50);
                        Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                            PetLimitBreakStatus.TheLevelOfLimitBreakItemIsTooLow, (Asda2PetRecord) null,
                            (Asda2Item) null, (Asda2Item) null);
                    }
                    else if (ownedPet.Level >= (byte) 10)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to break pet level limit more than 10 lvl.",
                            50);
                        Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                            PetLimitBreakStatus.MaximumBreakLvlLimitReached, (Asda2PetRecord) null, (Asda2Item) null,
                            (Asda2Item) null);
                    }
                    else
                    {
                        bool flag = CharacterFormulas.CalcPetLevelBreakSuccess();
                        Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq2);
                        if (shopShopItem != null && shopShopItem.Category != Asda2ItemCategory.PetLevelProtection)
                        {
                            client.ActiveCharacter.YouAreFuckingCheater(
                                "Trying to break pet level limit with incorect category supl.", 50);
                            Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                                PetLimitBreakStatus.MaximumBreakLvlLimitReached, (Asda2PetRecord) null,
                                (Asda2Item) null, (Asda2Item) null);
                        }
                        else
                        {
                            if (flag)
                            {
                                ++ownedPet.MaxLevel;
                                ++ownedPet.Level;
                            }
                            else
                                ownedPet.RemovePrcExp(shopShopItem == null ? 50 : 10);

                            if (shopShopItem != null)
                                shopShopItem.ModAmount(-1);
                            regularItem.ModAmount(-1);
                            Asda2PetHandler.SendPetLevelLimitBreakedResponse(client,
                                flag ? PetLimitBreakStatus.Ok : PetLimitBreakStatus.FailedRedusedBy50, ownedPet,
                                regularItem, shopShopItem);
                        }
                    }
                }
            }
        }

        public static void SendPetLevelLimitBreakedResponse(IRealmClient client, PetLimitBreakStatus status,
            Asda2PetRecord pet = null, Asda2Item lvlBreakItem = null, Asda2Item suplItem = null)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[2]
            {
                lvlBreakItem,
                suplItem
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetLevelLimitBreaked))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(pet == null ? 0 : pet.Guid);
                packet.WriteInt16(pet == null ? 0 : (int) pet.Id);
                packet.WriteFixedAsciiString(pet == null ? "" : pet.Name, 16, Locale.Start);
                packet.WriteByte(pet == null ? 0 : (int) pet.HungerPrc);
                packet.WriteByte(pet == null ? 0 : (int) pet.Level);
                packet.WriteByte(pet == null ? 0 : (int) pet.MaxLevel);
                packet.WriteInt16(pet == null ? 0 : (int) pet.Expirience);
                packet.WriteByte(1);
                packet.WriteInt16(pet == null ? (short) 0 : (short) pet.Stat1Type);
                packet.WriteInt32(pet == null ? 0 : pet.Stat1Value);
                packet.WriteInt16(pet == null ? (short) 0 : (short) pet.Stat2Type);
                packet.WriteInt32(pet == null ? 0 : pet.Stat2Value);
                packet.WriteInt16(pet == null ? (short) 0 : (short) pet.Stat3Type);
                packet.WriteInt32(pet == null ? 0 : pet.Stat3Value);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                for (int index = 0; index < 2; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Slot);
                    packet.WriteInt16(asda2Item == null ? -1 : (asda2Item.IsDeleted ? -1 : 0));
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteSkip(Asda2PetHandler.stab25);
                    packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteSkip(Asda2PetHandler.stab28);
                }

                client.Send(packet, false);
            }
        }
    }
}