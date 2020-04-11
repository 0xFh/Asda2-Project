using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Handlers
{
    class Asda2GroupHandler
    {
        [PacketHandler(RealmServerOpCode.SendPartyInvite)]//5090
        public static void SendPartyInviteRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            var destSessId = packet.ReadUInt16();//default : 14Len : 2

            var destChr = World.GetCharacterBySessionId(destSessId);
            if (destChr == null)
                return;
            OnPartyInviteRequest(client, destChr);
        }

        private static void OnPartyInviteRequest(IRealmClient client, Character destChr)
        {
            if (!destChr.EnablePartyRequest)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry ,but {0} rejects all party requests.", destChr.Name));
                return;
            }
            
            var inviteeName = destChr.Name;

            var inviter = client.ActiveCharacter;
            var group = inviter.Group;

            Character invitee;

            var listInviters = Singleton<RelationMgr>.Instance.GetPassiveRelations(destChr.EntityId.Low,
                                                                                   CharacterRelationType.GroupInvite);

            var invite = listInviters.FirstOrDefault();
            if (invite != null)
            {
                var ticks = int.Parse(invite.Note);
                var timeFromLastInvite = Environment.TickCount - ticks;
                if (timeFromLastInvite > 30000)
                    RelationMgr.Instance.RemoveRelation(invite);
            }
            var res = Group.CheckInvite(inviter, out invitee, inviteeName);

            if (res == GroupResult.NoError)
            {
                var listInvitees = Singleton<RelationMgr>.Instance.GetRelations(inviter.EntityId.Low,
                                                                                CharacterRelationType.GroupInvite);

                if (group == null || listInvitees.Count < group.InvitesLeft)
                {
                    BaseRelation inviteRelation = RelationMgr.CreateRelation(inviter.EntityId.Low,
                                                                             invitee.EntityId.Low, CharacterRelationType.GroupInvite);
                    inviteRelation.Note = Environment.TickCount.ToString(CultureInfo.InvariantCulture);
                    Singleton<RelationMgr>.Instance.AddRelation(inviteRelation);

                    // Target has been invited
                    //Group.SendResult(inviter.Client, GroupResult.NoError, inviteeName);
                    SendInviteToPartyResponseOrRequestToAnotherPlayerResponse(invitee.Client, PartyInviteStatusRequest.Invite, client.ActiveCharacter.Name);
                    SendPartyIniteResponseResponse(client, inviter, invitee, PartyInviteStatusResponse.SomeOneRevicingYourInvation);
                }
            }
            else
            {
                SendPartyIniteResponseResponse(client, inviter, invitee, PartyInviteStatusResponse.ThereIsNoOneToInvite);
            }
        }
        [PacketHandler(RealmServerOpCode.PartyInvireAnswer)]//5092
        public static void PartyInvireAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 12;
            var status = packet.ReadByte();//default : 0Len : 1
            //var destSessId0 = packet.ReadInt16();//default : -1Len : 2
            //  var srcAccId = packet.ReadInt32();//default : 340701Len : 4
            var invitee = client.ActiveCharacter;

            var listInviters = Singleton<RelationMgr>.Instance.GetPassiveRelations(invitee.EntityId.Low,
                                                                                   CharacterRelationType.GroupInvite);

            var invite = listInviters.FirstOrDefault();

            //Check if we got invited
            if (invite == null)
                return;
            //Removes the group invite relation between the inviter and invitee
            RelationMgr.Instance.RemoveRelation(invite);

            var inviter = World.GetCharacter(invite.CharacterId);
            if (inviter == null)
                return;
            if (status == 1)//accept
            {
                //If the inviter isnt in a group already we create a new one
                var inviterGroup = inviter.Group ?? new PartyGroup(inviter);
                //Add the invitee to the group
                inviterGroup.AddMember(invitee, true);
                SendPartyIniteResponseResponse(inviter.Client, inviter, invitee, PartyInviteStatusResponse.TheInvitionRequestHasBeenAccepted);
                SendPartyIniteResponseResponse(invitee.Client, inviter, invitee, PartyInviteStatusResponse.TheInvitionRequestHasBeenAccepted);
            }
            else
            {
                SendPartyIniteResponseResponse(inviter.Client, inviter, invitee, PartyInviteStatusResponse.TheInvitionRequestHasBeenDeclined);
            }
        }

        public static void SendPartyIniteResponseResponse(IRealmClient client, Character inviter, Character invitment, PartyInviteStatusResponse status)
        {
            if (client == null || inviter == null || invitment == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyIniteResponse))//5093
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(inviter.SessionId);//{senderSessId}default value : 20 Len : 2
                packet.WriteInt32(inviter.AccId);//{senderAccId}default value : 239913 Len : 4
                packet.WriteInt16(invitment.SessionId);//{recieverSessId}default value : 60 Len : 2
                packet.WriteInt32(invitment.AccId);//{recieverAccId}default value : 340701 Len : 4
                packet.WriteInt16(1);//value name : PartyId default value : 43Len : 2
                client.Send(packet, addEnd: false);
            }
        }




        public static void SendInviteToPartyResponseOrRequestToAnotherPlayerResponse(IRealmClient client, PartyInviteStatusRequest status, string senderName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.InviteToPartyResponseOrRequestToAnotherPlayer))//5091
            {
                packet.WriteByte((byte)status);//{status}default value : 0 Len : 1
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{resieverSessId}default value : 60 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteFixedAsciiString(senderName, 20);//{name}default value :  Len : 20
                packet.WriteByte(1);//value name : unk8 default value : 1Len : 1
                packet.WriteInt16(1);//value name : partyId default value : 43Len : 2
                client.Send(packet, addEnd: false);
            }
        }
        [PacketHandler(RealmServerOpCode.InviteSecodCharToParty)]//5094
        public static void InviteSecodCharToPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            var targetSessId = packet.ReadUInt16();//default : 18Len : 2
            var destChr = World.GetCharacterBySessionId(targetSessId);
            if (destChr == null)
                return;
            OnPartyInviteRequest(client, destChr);
        }

        static readonly byte[] md5 = new byte[] { 0x54, 0xB2, 0x66, 0xAF, 0xDE, 0x1D, 0x5A, 0x2C, 0xD4, 0x62, 0x88, 0x45, 0x06, 0xBE, 0x00, 0x73 };

        public static void SendPartyInfoResponse(Group @group)
        {
            if (group == null)
                return;
            var groupAccIdIds = new int[6];
            var groupNames = new string[6];
            var it = 0;
            for (int i = 0; i < 6; i++)
            {
                groupAccIdIds[i] = -1;
            }
            var sqChrs = new List<Character>();
            if (@group.Leader.Character == null)
                return;
            sqChrs.Add(@group.Leader.Character);
            sqChrs.AddRange(from member in @group where member.Character != @group.Leader.Character select member.Character);
            foreach (var member in sqChrs)
            {
                groupAccIdIds[it] = (int)member.AccId;
                groupNames[it] = member.Name;
                it++;
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyInfo))//5107
            {
                for (int i = 0; i < 6; i += 1)
                {
                    packet.WriteInt32(groupAccIdIds[i]);//{memberAccId}default value : 340701 Len : 4

                } for (int i = 0; i < 6; i += 1)
                {
                    packet.WriteFixedAsciiString(groupNames[i] ?? "", 20);//{Name}default value :  Len : 20
                }
                @group.Send(packet, addEnd: true);
            }
        }
        public static void SendPartyHasBrokenResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyHasBroken))//5103
            {
                packet.WriteInt16(1);//{status}default value : 0 Len : 1
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendPartyMemberInitialInfoResponse(Character member)
        {
            if (member == null || member.Group == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyMemberInitialInfo))//5109
            {
                packet.WriteInt32(member.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteByte(member.Level);//{level}default value : 33 Len : 1
                packet.WriteByte(member.ProfessionLevel);//{profLevel}default value : 2 Len : 1
                packet.WriteByte((byte)member.Class);//{profType}default value : 3 Len : 1
                packet.WriteInt32(member.MaxHealth);//{maxHealth}default value : 1615 Len : 4
                packet.WriteInt32(member.Health);//{curHealth}default value : 1615 Len : 4
                packet.WriteInt16(member.MaxPower);//{maxMana}default value : 239 Len : 2
                packet.WriteInt16(member.Power);//{curMana}default value : 239 Len : 2
                packet.WriteInt16((short)member.Asda2X);//{x}default value : 75 Len : 2
                packet.WriteInt16((short)member.Asda2Y);//{y}default value : 358 Len : 2
                packet.WriteByte((byte)member.Map.MapId);//{map}default value : 3 Len : 1
                member.Group.Send(packet, addEnd: false);
            }
        }
        public static void SendPartyMemberBuffInfoResponse(Character member)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyMemberBuffInfo))//5108
            {
                packet.WriteInt32(member.AccId);//{accId}default value : 340701 Len : 4
                var auras = new Aura[28];
                var it = 0;
                foreach (var visibleAura in member.Auras.ActiveAuras)
                {
                    if (visibleAura.TicksLeft <= 0) continue;
                    auras[it++] = visibleAura;
                    if (auras.Length <= it)
                        break;
                }
                for (int i = 0; i < 28; i += 1)
                {
                    var aura = auras[i];
                    packet.WriteByte(aura == null ? 0 : 1);//exist?
                    packet.WriteByte(0);//spadaet?
                    packet.WriteInt32(aura == null ? -1 : aura.Spell.RealId);//{duration}default value : -1 Len : 4
                }
                member.Group.Send(packet, addEnd: false);
            }
        }
        public static void SendPartyMemberPositionInfoResponse(Character member)
        {
            if (member.Group == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.PartyMemberPositionInfo))//5104
            {
                packet.WriteInt16(member.SessionId);//{sessId}default value : 31 Len : 2
                packet.WriteInt32(member.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteInt16((byte)member.Map.MapId);//{mapId}default value : 0 Len : 2
                packet.WriteInt16((short)member.Asda2Position.X);//{x}default value : 79 Len : 2
                packet.WriteInt16((short)member.Asda2Position.Y);//{y}default value : 187 Len : 2
                member.Group.Send(packet, addEnd: false);
            }
        }
        [PacketHandler(RealmServerOpCode.ExileFromParty)]//5100
        public static void ExileFromPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            var targetAccId = packet.ReadUInt32();
            //var kickedSessId = packet.ReadUInt16();//default : 47Len : 2
            //var name = packet.ReadAsdaString(20);//default : Len : 20
            var chr = World.GetCharacterByAccId(targetAccId);
            if (chr == null || !chr.IsInGroup)
                return;
            using (var p = new RealmPacketOut(RealmServerOpCode.PartyMemberKicked))//5101
            {
                p.WriteInt32(chr.AccId);//{accId}default value : 355010 Len : 4
                p.WriteFixedAsciiString(chr.Name, 20);//{name}default value :  Len : 20
                chr.Group.Send(p, addEnd: true);
            }
            chr.GroupMember.LeaveGroup();
        }

        [PacketHandler(RealmServerOpCode.LeaveFromParty)]//5098
        public static void LeaveFromPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.IsInGroup)
                client.ActiveCharacter.GroupMember.LeaveGroup();
            SendPartyHasBrokenResponse(client);
        }


    }
    public enum PartyInviteStatusRequest
    {
        Invite = 0,
        Invited = 1,
        AlreadyBelongToAParty = 2,
        AlreadyLogout = 3,
        YouCantInvite2OrMorePeopleAtOneTime = 4,
        ADifferentPersonInvitedAtThisTime = 5,
        YouAlreadyInGroup = 8,
        Dicline = 9,
        TargetAlreadyInGroup = 10,
        YouCantInviteOtherFactionToAGroup = 11
    }
    public enum PartyInviteStatusResponse
    {
        TheInvitionRequestHasBeenDeclined = 0,
        TheInvitionRequestHasBeenAccepted = 1,
        ThereIsNoOneToInvite = 2,
        TheInvitionTimeHasPassed = 3,
        YouAreAlreadyInParty = 4,
        SomeOneRevicingYourInvation = 5,
        SomeoneIsInvitingYou = 6
    }
}
