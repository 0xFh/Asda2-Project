using System;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    class Asda2PvpHandler
    {
        [PacketHandler(RealmServerOpCode.PvpRquest)]//4302
        public static void PvpRquestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 22;
            var targetSessId = packet.ReadUInt16();//default : 79Len : 2
            //var targetAccId = packet.ReadInt32();//default : 340701Len : 4
            var chr = World.GetCharacterBySessionId(targetSessId);
            if(chr == null)
            {
                client.ActiveCharacter.SendSystemMessage("The character you asking for duel is not found.");
                return;
            }
            /*if(chr.Asda2FactionId!=-1 && chr.Asda2FactionId!=client.ActiveCharacter.Asda2FactionId)
            {
                client.ActiveCharacter.SendSystemMessage("You can't pvp with character from other faction.");
                return;
            }*/
            if (client.ActiveCharacter.IsAsda2Dueling || chr.IsAsda2Dueling)
            {
                client.ActiveCharacter.SendInfoMsg("Already dueling.");
                return;
            }
            SendPvpRequestToCharFromSrvResponse(client.ActiveCharacter, chr);
            chr.Asda2DuelingOponent = client.ActiveCharacter;
        }
        public static void SendPvpRequestToCharFromSrvResponse(Character sender,Character rcv)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PvpRquest))//4302
            {
                packet.WriteInt16(sender.SessionId);//{senderSessId}default value : 39 Len : 2
                packet.WriteInt32(sender.AccId);//{senderAccId}default value : 355335 Len : 4
                packet.WriteFixedAsciiString(sender.Name,20);//{senderName}default value :  Len : 20
                packet.WriteInt16(rcv.SessionId);//{rcvSessId}default value : 45 Len : 2
                packet.WriteInt32(rcv.AccId);//{rcvAccId}default value : 340701 Len : 4
                packet.WriteInt16(0);//value name : unk10 default value : 0Len : 2
                rcv.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.AnswerPvpRequestOrStartPvp)]//4303
        public static void AnswerPvpRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.IsAsda2Dueling)
            {
                client.ActiveCharacter.SendInfoMsg("You already dueling.");
                return;
            }
      if (client.ActiveCharacter.Asda2DuelingOponent == null)
      {
        client.ActiveCharacter.YouAreFuckingCheater("Trying to answer pvp without oponent", 20);
        return;
      }
      if (client.ActiveCharacter.MapId == MapId.BatleField)
      {
        client.ActiveCharacter.SendInfoMsg("Duel not allowed on war.");
        return;
      }
      packet.Position -= 4;
            var accepted = packet.ReadByte() ==1;//default : 1Len : 1
            if(accepted)
            {
                //start pvp
                new Asda2Pvp(client.ActiveCharacter.Asda2DuelingOponent, client.ActiveCharacter);
            }
            else
            {
                SendPvpStartedResponse(Asda2PvpResponseStatus.Rejected, client.ActiveCharacter.Asda2DuelingOponent,
                                       client.ActiveCharacter);
                client.ActiveCharacter.Asda2DuelingOponent = null;
            }
        }
        public static void SendPvpStartedResponse(Asda2PvpResponseStatus status,Character rcv,Character answerer)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AnswerPvpRequestOrStartPvp))//4303
            {
                packet.WriteByte((byte) status);//{status}default value : 1 Len : 1
                packet.WriteInt16(rcv.SessionId);//{rcvSessId}default value : 96 Len : 2
                packet.WriteInt32(rcv.AccId);//{rcvAccId}default value : 354889 Len : 4
                packet.WriteFixedAsciiString(answerer.Name,20);//{AnswererName}default value :  Len : 20
                packet.WriteInt16(0);//value name : unk8 default value : 0Len : 2
                packet.WriteInt16((short) answerer.Asda2X);//{x}default value : 156 Len : 2
                packet.WriteInt16((short)answerer.Asda2Y);//{y}default value : 365 Len : 2
                rcv.Send(packet, addEnd: false);
            }
        }
        public static void SendPvpRoundEffectResponse(Character firstDueler,Character secondDueler)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PvpRoundEffect))//4305
            {
                packet.WriteSkip(stab6);//value name : stab6 default value : stab6Len : 2
                packet.WriteInt32(firstDueler.AccId);//{fisrtDuelerAccId}default value : 354889 Len : 4
                packet.WriteInt32(secondDueler.AccId);//{secondDuelerAccId}default value : 340701 Len : 4
                packet.WriteInt16((short) ((firstDueler.Asda2X + secondDueler.Asda2X) / 2));//{x}default value : 156 Len : 2
                packet.WriteInt16((short)((firstDueler.Asda2Y + secondDueler.Asda2Y) / 2));//{y}default value : 365 Len : 2
                firstDueler.SendPacketToArea(packet,true, false);
            }
        }
        static readonly byte[] stab6 = new byte[] { 0x00, 0x00 };

        public static void SendDuelEndedResponse(Character winer,Character looser)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.DuelEnded))//4304
            {
                packet.WriteByte(0);//value name : unk1 default value : 0Len : 1
                packet.WriteByte(0);//value name : unk5 default value : 0Len : 1
                packet.WriteInt16(winer.SessionId);//{winerSessId}default value : 96 Len : 2
                packet.WriteInt32(winer.AccId);//{winerAccNum}default value : 340701 Len : 4
                packet.WriteInt16(looser.SessionId);//{loosetSessId}default value : 79 Len : 2
                packet.WriteInt32(looser.AccId);//{looserAccNum}default value : 354889 Len : 4
                packet.WriteByte(2);//value name : unk1 default value : 2Len : 1
                packet.WriteFixedAsciiString(winer.Name,20);//{winerName}default value :  Len : 20
                packet.WriteFixedAsciiString(looser.Name,20);//{LooserName}default value :  Len : 20
                winer.SendPacketToArea(packet,true, true);
                looser.Send(packet,addEnd: true);
            }
        }


    }
    public enum Asda2PvpResponseStatus
    {
        Rejected =0,
        Ok =1,
    }
    public class Asda2Pvp
    {
        public bool IsActive { get; set; }
        public static int PvpTimeSecs = 5*60;
        private Character _losser;
        public Character FirstCharacter { get; set; }
        public Character SecondCharacter { get; set; }
        public Character Losser
        {
            get { return _losser; }
            set { _losser = value; StopPvp();}
        }

        public Character Winner { get { return FirstCharacter == Losser ? SecondCharacter : FirstCharacter; } }
        public int PvpTimeOuted { get; set; }
        public Asda2Pvp(Character firstCharacter,Character secondCharacter)
        {
            firstCharacter.Asda2Duel = this;
            secondCharacter.Asda2Duel = this;
            firstCharacter.Asda2DuelingOponent = secondCharacter;
            secondCharacter.Asda2DuelingOponent = firstCharacter;
            Asda2PvpHandler.SendPvpStartedResponse(Asda2PvpResponseStatus.Ok,firstCharacter,secondCharacter);
            Asda2PvpHandler.SendPvpStartedResponse(Asda2PvpResponseStatus.Ok, secondCharacter, firstCharacter);
            Asda2PvpHandler.SendPvpRoundEffectResponse(firstCharacter,secondCharacter);
            firstCharacter.Map.CallDelayed(10000, StartPvp);
            PvpTimeOuted = Environment.TickCount + PvpTimeSecs*1000;
            IsActive = true;
            FirstCharacter = firstCharacter;
            SecondCharacter = secondCharacter;
            FirstCharacter.EnemyCharacters.Add(secondCharacter);
            SecondCharacter.EnemyCharacters.Add(firstCharacter);
        }

        public void StartPvp()
        {
            if(IsActive==false)
                return;
            GlobalHandler.SendFightingModeChangedResponse(FirstCharacter.Client,FirstCharacter.SessionId,(int) FirstCharacter.AccId,SecondCharacter.SessionId);
            GlobalHandler.SendFightingModeChangedResponse(SecondCharacter.Client, SecondCharacter.SessionId, (int)SecondCharacter.AccId, FirstCharacter.SessionId);
            UpdatePvp();
        }
        public void StopPvp()
        {
            if(!IsActive)
                return;
            IsActive = false;
            if (Losser == null)
                Losser = FirstCharacter;
            FirstCharacter.EnemyCharacters.Remove(SecondCharacter);
            SecondCharacter.EnemyCharacters.Remove(FirstCharacter);
            FirstCharacter.CheckEnemysCount();
            SecondCharacter.CheckEnemysCount();
            Asda2PvpHandler.SendDuelEndedResponse(Winner,Losser);

            Asda2TitleChecker.OnWinDuel(Winner);

            FirstCharacter.Asda2Duel = null;
            SecondCharacter.Asda2Duel = null;
            FirstCharacter.Asda2DuelingOponent = null;
            SecondCharacter.Asda2DuelingOponent = null;
            FirstCharacter = null;
            SecondCharacter = null;
        }
        public void UpdatePvp()
        {
            if(PvpTimeOuted<Environment.TickCount||FirstCharacter == null || SecondCharacter == null||FirstCharacter.Map == null || SecondCharacter.Map == null)
            {
                StopPvp();
                return;
            }
            FirstCharacter.Map.CallDelayed(3000, UpdatePvp);
        }
    }
}