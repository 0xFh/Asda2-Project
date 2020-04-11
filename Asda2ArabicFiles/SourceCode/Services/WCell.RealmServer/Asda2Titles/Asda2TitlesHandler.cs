using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2Titles
{
    public static class Asda2TitlesHandler
    {
        public static void SendDiscoveredTitlesResponse(IRealmClient client)
        {
            if(client.ActiveCharacter.DiscoveredTitles==null)return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.DiscoveredTitles))//6086
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{charId}default value : 360729 Len : 4
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 65 Len : 2
                client.ActiveCharacter.DiscoveredTitles.WriteToAsda2Packet(packet);//{data}default value : data Len : 64
                packet.WriteInt16(1);//{id}default value : -1 Len : 2
                packet.WriteInt16(10);//{value}default value : 0 Len : 2
                packet.WriteInt16(2);//{id}default value : -1 Len : 2
                packet.WriteInt16(20);//{value}default value : 0 Len : 2
                packet.WriteInt16(3);//{id}default value : -1 Len : 2
                packet.WriteInt16(40);//{value}default value : 0 Len : 2
                client.Send(packet,addEnd: true);
            }
        }
        
        public static void SendGetedTitlesResponse(IRealmClient client)
        {
            if (client.ActiveCharacter.GetedTitles == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.GetedTitles))//6066
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 65 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{charId}default value : 360729 Len : 4
                packet.WriteInt32(client.ActiveCharacter.Asda2TitlePoints);//{timeToNextChange}default value : 0 Len : 4
                client.ActiveCharacter.GetedTitles.WriteToAsda2Packet(packet);//{data}default value : data Len : 64
                client.Send(packet,addEnd: true);
            }
        }
        
        public static void SendYouGetNewTitleResponse(Character chr, short newTitleId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouGetNewTitle)) //6065
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 3 Len : 2
                packet.WriteInt32(chr.AccId); //{accId}default value : 354889 Len : 4
                packet.WriteInt16(newTitleId); //{titleId}default value : 86 Len : 2
                packet.WriteByte(0); //value name : unk8 default value : 0Len : 1
                packet.WriteInt32(chr.Asda2TitlePoints); //{totalTitlePoints}default value : 10 Len : 4
                chr.SendPacketToArea(packet,true,true);
            }
        }

        public static void SendTitleDiscoveredResponse(IRealmClient client, short titleId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TitleDiscovered)) //6085
            {
                packet.WriteInt32(client.ActiveCharacter.AccId); //{accId}default value : 340701 Len : 4
                packet.WriteInt16(client.ActiveCharacter.SessionId); //{sessId}default value : 33 Len : 2
                packet.WriteInt16(titleId); //{titleId}default value : 219 Len : 2
                client.ActiveCharacter.SendPacketToArea(packet,true,true);
            }
        }
        [PacketHandler(RealmServerOpCode.SetTitle)]//6067
        public static void SetTitleRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var prefixTitleId = packet.ReadInt16();//default : 105Len : 2
            var postFixTitleId = packet.ReadInt16();//default : -1Len : 2
            if (prefixTitleId != -1)
            {
                if (prefixTitleId > (decimal) Asda2TitleId.End || prefixTitleId<-1)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Wrong title id : " + prefixTitleId);
                    return;
                }
                if (!client.ActiveCharacter.GetedTitles.GetBit(prefixTitleId))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tries to set not owned title id : " + prefixTitleId);
                    return;
                }
                client.ActiveCharacter.Record.PreTitleId = prefixTitleId;
            }
            if (postFixTitleId != -1)
            {
                if (postFixTitleId > (decimal)Asda2TitleId.End || postFixTitleId < -1)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Wrong title id : " + prefixTitleId);
                    return;
                }
                if (!client.ActiveCharacter.GetedTitles.GetBit(postFixTitleId))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tries to set not owned title id : " + postFixTitleId);
                    return;
                }
                client.ActiveCharacter.Record.PostTitleId = postFixTitleId;
            }
            GlobalHandler.BroadcastCharacterPlaceInTitleRatingResponse(client.ActiveCharacter);
        }
            

    }
}
