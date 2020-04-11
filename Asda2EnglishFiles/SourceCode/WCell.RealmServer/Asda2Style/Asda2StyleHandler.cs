using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2Style
{
    public static class Asda2StyleHandler
    {
        [PacketHandler(RealmServerOpCode.ChangeFaceOrHair)]
        public static void ChangeFaceOrHairRequest(IRealmClient client, RealmPacketIn packet)
        {
            bool isHair = packet.ReadByte() == (byte) 1;
            short key = packet.ReadInt16();
            int num1 = (int) packet.ReadByte();
            int num2 = (int) packet.ReadByte();
            packet.ReadInt32();
            packet.Position += 2;
            short slotInq = packet.ReadInt16();
            Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq);
            if (isHair)
            {
                if (!Asda2StyleMgr.HairTemplates.ContainsKey(key))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to change hair to unknown hair.", 50);
                    Asda2StyleHandler.SendFaceOrHairChangedResponse(client, true, false, (Asda2Item) null);
                    return;
                }

                HairTableRecord hairTemplate = Asda2StyleMgr.HairTemplates[key];
                if (hairTemplate.Price > 0 && !client.ActiveCharacter.SubtractMoney((uint) hairTemplate.Price))
                {
                    client.ActiveCharacter.SendInfoMsg("Not enought gold.");
                    Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, false, (Asda2Item) null);
                    return;
                }

                if (hairTemplate.CuponCount > 0)
                {
                    if (shopShopItem == null || shopShopItem.Category != Asda2ItemCategory.StyleShopCoupon)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Not enought style coupons.", 30);
                        Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, false, (Asda2Item) null);
                        return;
                    }

                    shopShopItem.ModAmount(-hairTemplate.CuponCount);
                }

                client.ActiveCharacter.HairColor = hairTemplate.HairColor;
                client.ActiveCharacter.HairStyle = hairTemplate.HairId;
            }
            else
            {
                if (!Asda2StyleMgr.FaceTemplates.ContainsKey(key))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to change face to unknown face.", 50);
                    Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, false, (Asda2Item) null);
                    return;
                }

                FaceTableRecord faceTemplate = Asda2StyleMgr.FaceTemplates[key];
                if (faceTemplate.Price > 0 && !client.ActiveCharacter.SubtractMoney((uint) faceTemplate.Price))
                {
                    client.ActiveCharacter.SendInfoMsg("Not enought gold.");
                    Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, false, (Asda2Item) null);
                    return;
                }

                if (faceTemplate.CuponCount > 0)
                {
                    if (shopShopItem == null || shopShopItem.Category != Asda2ItemCategory.StyleShopCoupon)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Not enought style coupons.", 30);
                        Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, false, (Asda2Item) null);
                        return;
                    }

                    shopShopItem.ModAmount(-faceTemplate.CuponCount);
                }

                client.ActiveCharacter.Record.Face = (byte) faceTemplate.FaceId;
            }

            Asda2StyleHandler.SendFaceOrHairChangedResponse(client, isHair, true, shopShopItem);
        }

        public static void SendFaceOrHairChangedResponse(IRealmClient client, bool isHair, bool success = false,
            Asda2Item usedItem = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FaceOrHairChanged))
            {
                packet.WriteByte(success ? 1 : 0);
                packet.WriteByte(isHair ? 1 : 2);
                packet.WriteByte(client.ActiveCharacter.HairStyle);
                packet.WriteByte(client.ActiveCharacter.HairColor);
                packet.WriteByte(client.ActiveCharacter.Record.Face);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(client.ActiveCharacter.Money);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, usedItem, false);
                client.Send(packet, true);
                if (!success)
                    return;
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(9U);
                switch (++progressRecord.Counter)
                {
                    case 50:
                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Beautiful47);
                        break;
                    case 100:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Beautiful47);
                        break;
                }

                progressRecord.SaveAndFlush();
            }
        }
    }
}