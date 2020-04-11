using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    internal class Asda2GroupHandler
    {
        private static readonly byte[] md5 = new byte[16]
        {
            (byte) 84,
            (byte) 178,
            (byte) 102,
            (byte) 175,
            (byte) 222,
            (byte) 29,
            (byte) 90,
            (byte) 44,
            (byte) 212,
            (byte) 98,
            (byte) 136,
            (byte) 69,
            (byte) 6,
            (byte) 190,
            (byte) 0,
            (byte) 115
        };

        [PacketHandler(RealmServerOpCode.SendPartyInvite)]
        public static void SendPartyInviteRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
                return;
            Asda2GroupHandler.OnPartyInviteRequest(client, characterBySessionId);
        }

        private static void OnPartyInviteRequest(IRealmClient client, Character destChr)
        {
            if (!destChr.EnablePartyRequest)
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry ,but {0} rejects all party requests.",
                    (object) destChr.Name));
            else if (client.ActiveCharacter.Asda2FactionId != (short) -1 && destChr.Asda2FactionId != (short) -1 &&
                     (int) destChr.Asda2FactionId != (int) client.ActiveCharacter.Asda2FactionId)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry ,but {0} is in other faction.",
                    (object) destChr.Name));
            }
            else
            {
                string name = destChr.Name;
                Character activeCharacter = client.ActiveCharacter;
                Group group = activeCharacter.Group;
                IBaseRelation relation1 = Singleton<RelationMgr>.Instance
                    .GetPassiveRelations(destChr.EntityId.Low, CharacterRelationType.GroupInvite)
                    .FirstOrDefault<IBaseRelation>();
                if (relation1 != null && Environment.TickCount - int.Parse(relation1.Note) > 30000)
                    Singleton<RelationMgr>.Instance.RemoveRelation(relation1);
                Character target;
                if (Group.CheckInvite(activeCharacter, out target, name) == GroupResult.NoError)
                {
                    HashSet<IBaseRelation> relations =
                        Singleton<RelationMgr>.Instance.GetRelations(activeCharacter.EntityId.Low,
                            CharacterRelationType.GroupInvite);
                    if (group != null && relations.Count >= (int) group.InvitesLeft)
                        return;
                    BaseRelation relation2 = RelationMgr.CreateRelation(activeCharacter.EntityId.Low,
                        target.EntityId.Low, CharacterRelationType.GroupInvite);
                    relation2.Note = Environment.TickCount.ToString((IFormatProvider) CultureInfo.InvariantCulture);
                    Singleton<RelationMgr>.Instance.AddRelation(relation2);
                    Asda2GroupHandler.SendInviteToPartyResponseOrRequestToAnotherPlayerResponse(target.Client,
                        PartyInviteStatusRequest.Invite, client.ActiveCharacter.Name);
                    Asda2GroupHandler.SendPartyIniteResponseResponse(client, activeCharacter, target,
                        PartyInviteStatusResponse.SomeOneRevicingYourInvation);
                }
                else
                    Asda2GroupHandler.SendPartyIniteResponseResponse(client, activeCharacter, target,
                        PartyInviteStatusResponse.ThereIsNoOneToInvite);
            }
        }

        [PacketHandler(RealmServerOpCode.PartyInvireAnswer)]
        public static void PartyInvireAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 12;
            byte num = packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GroupInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            Character character = World.GetCharacter(relation.CharacterId);
            if (character == null)
                return;
            if (num == (byte) 1)
            {
                (character.Group ?? (Group) new PartyGroup(character)).AddMember(activeCharacter, true);
                Asda2GroupHandler.SendPartyIniteResponseResponse(character.Client, character, activeCharacter,
                    PartyInviteStatusResponse.TheInvitionRequestHasBeenAccepted);
                Asda2GroupHandler.SendPartyIniteResponseResponse(activeCharacter.Client, character, activeCharacter,
                    PartyInviteStatusResponse.TheInvitionRequestHasBeenAccepted);
            }
            else
                Asda2GroupHandler.SendPartyIniteResponseResponse(character.Client, character, activeCharacter,
                    PartyInviteStatusResponse.TheInvitionRequestHasBeenDeclined);
        }

        public static void SendPartyIniteResponseResponse(IRealmClient client, Character inviter, Character invitment,
            PartyInviteStatusResponse status)
        {
            if (client == null || inviter == null || invitment == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyIniteResponse))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(inviter.SessionId);
                packet.WriteInt32(inviter.AccId);
                packet.WriteInt16(invitment.SessionId);
                packet.WriteInt32(invitment.AccId);
                packet.WriteInt16(1);
                client.Send(packet, false);
            }
        }

        public static void SendInviteToPartyResponseOrRequestToAnotherPlayerResponse(IRealmClient client,
            PartyInviteStatusRequest status, string senderName)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut(RealmServerOpCode.InviteToPartyResponseOrRequestToAnotherPlayer))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteFixedAsciiString(senderName, 20, Locale.Start);
                packet.WriteByte(1);
                packet.WriteInt16(1);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.InviteSecodCharToParty)]
        public static void InviteSecodCharToPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
                return;
            Asda2GroupHandler.OnPartyInviteRequest(client, characterBySessionId);
        }

        public static void SendPartyInfoResponse(Group group)
        {
            if (group == null)
                return;
            int[] numArray = new int[6];
            string[] strArray = new string[6];
            int index1 = 0;
            for (int index2 = 0; index2 < 6; ++index2)
                numArray[index2] = -1;
            List<Character> characterList = new List<Character>();
            if (group.Leader.Character == null)
                return;
            characterList.Add(group.Leader.Character);
            characterList.AddRange(group
                .Where<GroupMember>((Func<GroupMember, bool>) (member => member.Character != group.Leader.Character))
                .Select<GroupMember, Character>((Func<GroupMember, Character>) (member => member.Character)));
            foreach (Character character in characterList)
            {
                numArray[index1] = (int) character.AccId;
                strArray[index1] = character.Name;
                ++index1;
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyInfo))
            {
                for (int index2 = 0; index2 < 6; ++index2)
                    packet.WriteInt32(numArray[index2]);
                for (int index2 = 0; index2 < 6; ++index2)
                    packet.WriteFixedAsciiString(strArray[index2] ?? "", 20, Locale.Start);
                group.Send(packet, true);
            }
        }

        public static void SendPartyHasBrokenResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyHasBroken))
            {
                packet.WriteInt16(1);
                client.Send(packet, true);
            }
        }

        public static void SendPartyMemberInitialInfoResponse(Character member)
        {
            if (member == null || member.Group == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyMemberInitialInfo))
            {
                packet.WriteInt32(member.AccId);
                packet.WriteByte(member.Level);
                packet.WriteByte(member.ProfessionLevel);
                packet.WriteByte((byte) member.Class);
                packet.WriteInt32(member.MaxHealth);
                packet.WriteInt32(member.Health);
                packet.WriteInt16(member.MaxPower);
                packet.WriteInt16(member.Power);
                packet.WriteInt16((short) member.Asda2X);
                packet.WriteInt16((short) member.Asda2Y);
                packet.WriteByte((byte) member.Map.MapId);
                member.Group.Send(packet, false);
            }
        }

        public static void SendPartyMemberBuffInfoResponse(Character member)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyMemberBuffInfo))
            {
                packet.WriteInt32(member.AccId);
                Aura[] auraArray = new Aura[28];
                int num = 0;
                foreach (Aura activeAura in member.Auras.ActiveAuras)
                {
                    if (activeAura.TicksLeft > 0)
                    {
                        auraArray[num++] = activeAura;
                        if (auraArray.Length <= num)
                            break;
                    }
                }

                for (int index = 0; index < 28; ++index)
                {
                    Aura aura = auraArray[index];
                    packet.WriteByte(aura == null ? 0 : 1);
                    packet.WriteByte(0);
                    packet.WriteInt32(aura == null ? -1 : (int) aura.Spell.RealId);
                }

                member.Group.Send(packet, false);
            }
        }

        public static void SendPartyMemberPositionInfoResponse(Character member)
        {
            if (member.Group == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyMemberPositionInfo))
            {
                packet.WriteInt16(member.SessionId);
                packet.WriteInt32(member.AccId);
                packet.WriteInt16((byte) member.Map.MapId);
                packet.WriteInt16((short) member.Asda2X);
                packet.WriteInt16((short) member.Asda2Y);
                member.Group.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ExileFromParty)]
        public static void ExileFromPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 8;
            Character characterByAccId = World.GetCharacterByAccId(packet.ReadUInt32());
            if (characterByAccId == null || !characterByAccId.IsInGroup)
                return;
            using (RealmPacketOut packet1 = new RealmPacketOut(RealmServerOpCode.PartyMemberKicked))
            {
                packet1.WriteInt32(characterByAccId.AccId);
                packet1.WriteFixedAsciiString(characterByAccId.Name, 20, Locale.Start);
                characterByAccId.Group.Send(packet1, true);
            }

            characterByAccId.GroupMember.LeaveGroup();
        }

        [PacketHandler(RealmServerOpCode.LeaveFromParty)]
        public static void LeaveFromPartyRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.IsInGroup)
                client.ActiveCharacter.GroupMember.LeaveGroup();
            Asda2GroupHandler.SendPartyHasBrokenResponse(client);
        }
    }
}