using System.Linq;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Asda2Titles;

namespace WCell.RealmServer.Mounts
{
    internal class Asda2MountHandler
    {
        #region Veiche

        [PacketHandler(RealmServerOpCode.RegisterVeiche)] //6768
        public static void RegisterVeicheRequest(IRealmClient client, RealmPacketIn packet)
        {
            var veicheId = packet.ReadInt32(); //default : 175Len : 4
            var inv = packet.ReadByte(); //default : 1Len : 1
            var slot = packet.ReadInt16(); //default : 12Len : 2
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null)
            {
                SendVeicheRegisteredResponse(client.ActiveCharacter, null, RegisterMountStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Mount item not fount. Restart client please.");
                return;
            }
            MountTemplate templ = null;
            if (Asda2MountMgr.TemplatesByItemIDs.ContainsKey(item.ItemId))
                templ = Asda2MountMgr.TemplatesByItemIDs[item.ItemId];
            if (templ == null)
            {
                SendVeicheRegisteredResponse(client.ActiveCharacter, null, RegisterMountStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Selected item is not mount.");
                return;
            }
            if (client.ActiveCharacter.OwnedMounts.ContainsKey(templ.Id))
            {
                SendVeicheRegisteredResponse(client.ActiveCharacter, null, RegisterMountStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Selected mount already registered.");
                return;
            }
            if (client.ActiveCharacter.MountBoxSize <= client.ActiveCharacter.OwnedMounts.Count)
            {
                SendVeicheRegisteredResponse(client.ActiveCharacter, null, RegisterMountStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Not enoght space in mount inventory.");
                return;
            }
            if (client.ActiveCharacter.OwnedMounts.ContainsKey(templ.Id))
                return;
            var rec = new Asda2MountRecord(templ, client.ActiveCharacter);
            client.ActiveCharacter.OwnedMounts.Add(templ.Id, rec);
            rec.Create();
            item.Amount = 0;
            Asda2TitleChecker.OnNewMount(client.ActiveCharacter, item.ItemId);
            SendVeicheRegisteredResponse(client.ActiveCharacter, item,
                                         RegisterMountStatus.Ok, templ.Id);

        }

        public static void SendVeicheRegisteredResponse(Character chr, Asda2Item veicheItem, RegisterMountStatus status,
                                                        int veicheId = -1)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.VeicheRegistered)) //6769
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 361343 Len : 4
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, veicheItem);
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{invWeight}default value : 11847 Len : 2
                packet.WriteInt32(veicheId); //value name : unk4 default value : 56Len : 4
                chr.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.SummonMount)] //6763
        public static void SummonMountRequest(IRealmClient client, RealmPacketIn packet)
        {
            var summon = packet.ReadBoolean(); //default : 0Len : 1
            if (summon)
            {
                if (client.ActiveCharacter.LastTransportUsedTime.AddSeconds(30) > System.DateTime.Now)
                {
                    client.ActiveCharacter.SendInfoMsg("Mount is on cooldown.");
                    return;
                }
                SendVeicheStatusChangedResponse(client.ActiveCharacter, MountStatusChanged.Summonig);
                SendMountSummoningResponse(client.ActiveCharacter);
            }
            else
            {
                client.ActiveCharacter.MountId = -1;
            }
        }

        public static void SendMountSummoningResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MountSummoning)) //6765
            {
                packet.WriteByte(1); //{status}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 361343 Len : 4
                packet.WriteInt16(3000); //{cooldown3000}default value : 3000 Len : 2
                chr.SendPacketToArea(packet, true, true);
            }
        }

        [PacketHandler(RealmServerOpCode.GetOnMount)] //6766
        public static void GetOnMountRequest(IRealmClient client, RealmPacketIn packet)
        {
            var mountId = packet.ReadInt32(); //default : 56Len : 4
            if (client.ActiveCharacter.OwnedMounts.ContainsKey(mountId))
            {
                client.ActiveCharacter.MountId = mountId;
            }
            else
            {
                SendCharacterOnMountStatusChangedResponse(client.ActiveCharacter, UseMountStatus.Fail);
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use not owned Mount.", 30);
            }
        }

        public static void SendCharacterOnMountStatusChangedResponse(Character chr, UseMountStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CHaracterOnMountStatusChanged)) //6764
            {
                packet.WriteByte((byte)status); //{usingStatus}default value : 1 Len : 1
                packet.WriteByte(chr.IsOnMount); //{onVeicheStatus}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 361343 Len : 4
                packet.WriteInt32(chr.MountId); //{mountId}default value : 56 Len : 4
                chr.SendPacketToArea(packet, true, true);
            }
        }
        public static void SendCharacterOnMountStatusChangedToPneClientResponse(IRealmClient reciver, Character trigger)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CHaracterOnMountStatusChanged)) //6764
            {
                packet.WriteByte((byte)UseMountStatus.Ok); //{usingStatus}default value : 1 Len : 1
                packet.WriteByte(trigger.IsOnMount); //{onVeicheStatus}default value : 1 Len : 1
                packet.WriteInt32(trigger.AccId); //{accId}default value : 361343 Len : 4
                packet.WriteInt32(trigger.MountId); //{mountId}default value : 56 Len : 4
                reciver.Send(packet, addEnd: true);
            }
        }
        public static void SendVeicheStatusChangedResponse(Character chr, MountStatusChanged status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.VeicheStatusChanged)) //6767
            {
                packet.WriteByte((byte)status); //{zeroUnsummonOneGetOnTwoSummoning}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 361343 Len : 4
                packet.WriteInt32(chr.MountId); //{veicheId}default value : 56 Len : 4
                chr.SendPacketToArea(packet, true, true);
            }
        }
        public static void SendMountBoxSizeInitResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MountBoxSizeInit))//6761
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteByte(client.ActiveCharacter.MountBoxSize);//{size}default value : 6 Len : 1
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendOwnedMountsListResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.OwnedMountsList))//6762
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                var mountIds = client.ActiveCharacter.OwnedMounts.Keys.ToArray();
                for (var i = 0; i < 90; i += 1)
                    packet.WriteInt32(mountIds.Length > i ? mountIds[i] : -1);//{mountId}default value : 56 Len : 4
                client.Send(packet, addEnd: true);
            }
        }


        #endregion

        internal enum MountStatusChanged
        {
            Unsumon = 0,
            Summoned = 1,
            Summonig = 2
        }

        internal enum UseMountStatus
        {
            Fail = 0,
            Ok = 1
        }

        internal enum RegisterMountStatus
        {
            Fail = 0,
            Ok = 1
        }

    }
}
