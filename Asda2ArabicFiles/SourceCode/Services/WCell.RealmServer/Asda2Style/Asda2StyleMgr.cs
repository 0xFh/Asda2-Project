using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Style
{
    public static class Asda2StyleMgr
    {
        public static Dictionary<short, FaceTableRecord> FaceTemplates = new Dictionary<short, FaceTableRecord>();
        public static Dictionary<short, HairTableRecord> HairTemplates = new Dictionary<short, HairTableRecord>(); 
        
        [Initialization(InitializationPass.Tenth,"Style shop.")]
        public static void Init()
        {
            ContentMgr.Load<HairTableRecord>();
            ContentMgr.Load<FaceTableRecord>();
        }
    }
    public static class Asda2StyleHandler
    {
        [PacketHandler(RealmServerOpCode.ChangeFaceOrHair)]//5470
        public static void ChangeFaceOrHairRequest(IRealmClient client, RealmPacketIn packet)
        {
            var isHair = packet.ReadByte()==1;//default : 1Len : 1
            var id = packet.ReadInt16();//default : 95Len : 2
            var hairId = packet.ReadByte();//default : 1Len : 1
            var hairColor = packet.ReadByte();//default : 23Len : 1
            var faceId = packet.ReadInt32();//default : 0Len : 4
            packet.Position += 2;
            var itemSlot = packet.ReadInt16();//default : 0Len : 4
            

            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(itemSlot);
            
            if (isHair)
            {
                if (!Asda2StyleMgr.HairTemplates.ContainsKey(id))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to change hair to unknown hair.", 50);
                    SendFaceOrHairChangedResponse(client, true);
                    return;
                }
                var template = Asda2StyleMgr.HairTemplates[id];
                if (template.Price > 0)
                {
                    if (!client.ActiveCharacter.SubtractMoney((uint) template.Price))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought gold.");
                        SendFaceOrHairChangedResponse(client,isHair);
                        return;
                    }
                }
                if (template.CuponCount > 0)
                {
                    if (item == null || item.Category!=Constants.Items.Asda2ItemCategory.StyleShopCoupon)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Not enought style coupons.",30);
                        SendFaceOrHairChangedResponse(client, isHair);
                        return;
                    }
                    item.ModAmount(-template.CuponCount);
                }
                client.ActiveCharacter.HairColor = template.HairColor;
                client.ActiveCharacter.HairStyle = template.HairId;
                Asda2TitleChecker.OnHairChange(client.ActiveCharacter);
            }
            else
            {
                if (!Asda2StyleMgr.FaceTemplates.ContainsKey(id))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to change face to unknown face.", 50);
                    SendFaceOrHairChangedResponse(client, isHair);
                    return;
                }
                var template = Asda2StyleMgr.FaceTemplates[id];
                if (template.Price > 0)
                {
                    if (!client.ActiveCharacter.SubtractMoney((uint) template.Price))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought gold.");
                        SendFaceOrHairChangedResponse(client, isHair);
                        return;
                    }
                }
                if (template.CuponCount > 0)
                {
                    if (item == null || item.Category != Constants.Items.Asda2ItemCategory.StyleShopCoupon)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Not enought style coupons.", 30);
                        SendFaceOrHairChangedResponse(client, isHair);
                        return;
                    }
                    item.ModAmount(-template.CuponCount);
                }
                client.ActiveCharacter.Record.Face = (byte) template.FaceId;
            }
            SendFaceOrHairChangedResponse(client, isHair, true, item);
        }
        public static void SendFaceOrHairChangedResponse(IRealmClient client, bool isHair, bool success = false, Asda2Item usedItem = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FaceOrHairChanged))//5471
            {
                packet.WriteByte(success?1:0);//{status}default value : 1 Len : 1
                packet.WriteByte(isHair?1:2);//{hair1face2}default value : 2 Len : 1
                packet.WriteByte(client.ActiveCharacter.HairStyle);//{hairId}default value : 3 Len : 1
                packet.WriteByte(client.ActiveCharacter.HairColor);//{hairColor}default value : 3 Len : 1
                packet.WriteByte(client.ActiveCharacter.Record.Face);//{faceId}default value : 105 Len : 1
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 10202 Len : 4
                packet.WriteInt32(client.ActiveCharacter.Money);//{money}default value : 41512584 Len : 4
                Asda2InventoryHandler.WriteItemInfoToPacket(packet,usedItem, false);
                client.Send(packet,addEnd: true);
            }
        }


    }
    [DataHolder]
    public class FaceTableRecord : IDataHolder
    {
        public short Id { get; set; }
        public byte IsEnabled { get; set; }
        public byte OneOrTwo { get; set; }
        public int FaceId { get; set; }
        public int Price { get; set; }
        public int CuponCount { get; set; }
        public void FinalizeDataHolder()
        {
            if(Asda2StyleMgr.FaceTemplates.ContainsKey(Id))
                return;
            Asda2StyleMgr.FaceTemplates.Add(Id,this);
        }
    }
    [DataHolder]
    public class HairTableRecord : IDataHolder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte IsEnabled { get; set; }
        public byte HairId { get; set; }
        public byte OneOrTwo { get; set; }
        public byte HairColor { get; set; }
        public int Price { get; set; }
        public int CuponCount { get; set; }
        public void FinalizeDataHolder()
        {
            if (Asda2StyleMgr.HairTemplates.ContainsKey((short) Id))
                return;
            Asda2StyleMgr.HairTemplates.Add((short)Id, this);
        }
    }
}
