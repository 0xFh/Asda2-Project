using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2GuildHandler
    {

        #region impeachment
        [PacketHandler(RealmServerOpCode.ImpeachmentReuest)]//4167
        public static void ImpeachmentReuestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild || client.ActiveCharacter.Asda2GuildRank != 3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to do ImpeachmentRequest with wrong parametrs.", 50);
                return;
            }
            var r = client.ActiveCharacter.Guild.CreateImpeachment(client.ActiveCharacter.GuildMember);
        }

        public static void SendImpeachmentStatusResponse(Character chr, CreateImpeachmentResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentStatus))//4168
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(chr.GuildId);//{clanId}default value : 1436 Len : 2
                packet.WriteInt32(chr.AccId);//{accId}default value : 340701 Len : 4
                chr.Send(packet, addEnd: true);
            }
        }
        public static void SendImpeachmentAnswerResponse(Guild g, string newLeaderName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentAnswer)) //4169
            {
                packet.WriteInt16(g.Id); //{guildId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(g.Leader.Name, 20);
                //{currentLeaderName}default value :  Len : 20
                packet.WriteFixedAsciiString(newLeaderName, 20); //{newLeaderName}default value :  Len : 20
                g.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.ChangeGuildLeaderRequest)]//4135
        public static void ChangeGuildLeaderRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var targetAccId = packet.ReadInt32();//default : 355335Len : 4
            var targetCharNum = packet.ReadByte();//default : 11Len : 1
            //var targetName = packet.ReadFixedAsciiString();//default : Len : 20
            if (!client.ActiveCharacter.IsInGuild || !client.ActiveCharacter.GuildMember.IsLeader)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to change leader with wrong parametrs.", 50);
                return;
            }
            var target =
                client.ActiveCharacter.Guild.Members[
                    Character.CharacterIdFromAccIdAndCharNum(targetAccId, targetCharNum)];
            if (target == null)
            {
                client.ActiveCharacter.SendSystemMessage("Can't find target character.");
                return;
            }
            client.ActiveCharacter.GuildMember.Asda2RankId = 3;
            SendGuildNotificationResponse(client.ActiveCharacter.Guild, GuildNotificationType.RankChanged, client.ActiveCharacter.GuildMember);
            target.Asda2RankId = 4;
            client.ActiveCharacter.Guild.Leader = target;
            SendGuildNotificationResponse(client.ActiveCharacter.Guild, GuildNotificationType.ApointedAsNewGuildLeader, target);
            client.ActiveCharacter.Guild.AddHistoryMessage(Asda2GuildHistoryType.ApointedAsGuildLeader, 0, target.Name, DateTime.Now.ToLongTimeString());
            SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild);
        }


        [PacketHandler(RealmServerOpCode.ImpeachmentVote)]//4170
        public static void ImpeachmentVoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("trying to do impeachment vote with wrong parametrs", 50);
                return;
            }
            packet.Position += 2;
            var accept = packet.ReadBoolean();//default : 1Len : 1
            if (accept)
            {
                client.ActiveCharacter.Guild.AddImpeachmentVote(client.ActiveCharacter.GuildMember);
            }
        }
        public static void SendImpeachmentResultResponse(Guild g, ImpeachmentResult result)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentResult))//4171
            {
                packet.WriteByte((byte)result);//{status}default value : 0 Len : 1
                packet.WriteInt16(g.Id);//{guildId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(g.Leader.Name, 20);//{leaderName}default value :  Len : 20
                g.Send(packet, addEnd: true);
            }
        }

        public enum CreateImpeachmentResult
        {
            Failed = 0,
            Success = 1,
            AlreadyInProgress = 6,
        }

        public enum ImpeachmentResult
        {
            Failed = 0,
            Success = 1,
        }
        #endregion
        #region Creation

        #endregion

        [PacketHandler(RealmServerOpCode.ReqUpdateGuildPoints)] //4142
        public static void ReqUpdateGuildPointsRequest(IRealmClient client, RealmPacketIn packet)
        {
            //var accId = packet.ReadInt32();//default : 355335Len : 4
            SendUpdateGuildPointsResponse(client);
        }

        public static void SendUpdateGuildPointsResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildPoints)) //4143
            {
                packet.WriteInt32(client.ActiveCharacter.AccId); //{accId}default value : 333843 Len : 4
                packet.WriteInt32(client.ActiveCharacter.GuildPoints); //{guildPoints}default value : 2624 Len : 4
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.CreateGuild)] //4109
        public static void CreateGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            var clanName = packet.ReadAsdaString(17, Locale.En); //default : Len : 22
            if (client.ActiveCharacter.Asda2FactionId == -1 || client.ActiveCharacter.Asda2FactionId == 2)
            {
                client.ActiveCharacter.SendInfoMsg("Wrong faction to create guild.");
                SendGuildCreatedResponse(client, CreateGuildStatus.YouCantCreateGuildWith, null);
                return;
            }
            if (client.ActiveCharacter.IsInGuild)
            {
                SendGuildCreatedResponse(client, CreateGuildStatus.YouAreInAnotherGuild, null);
                return;
            }
           /* if (!Asda2CharacterHandler.IsNameValid(clanName))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("guild name");
                SendGuildCreatedResponse(client, CreateGuildStatus.YouHaveChoosedTheInvalidGuildName, null);
                return;
            }*/
            if (GuildMgr.GetGuild(clanName) != null)
            {
                SendGuildCreatedResponse(client, CreateGuildStatus.GuildNameAlreadyExist, null);
                return;
            }
            if (client.ActiveCharacter.Level < 10)
            {
                SendGuildCreatedResponse(client, CreateGuildStatus.YouMustBeLevel10And1StJobToCreateClan, null);
                return;
            }
            if (!client.ActiveCharacter.SubtractMoney(10000))
            {
                SendGuildCreatedResponse(client, CreateGuildStatus.NotEnoghtMoney, null);
                return;
            }
            RealmServer.IOQueue.AddMessage(() =>
            {
                var guild = new Guild(client.ActiveCharacter.Record, clanName);
                SendGuildCreatedResponse(client, CreateGuildStatus.Ok, guild);
                client.ActiveCharacter.SendMoneyUpdate();
                Asda2TitleChecker.OnGuildCreated(client.ActiveCharacter);
            });
        }

        public static void SendGuildCreatedResponse(IRealmClient client, CreateGuildStatus status, Guild guild)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildCreated)) //4110
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt32(guild == null ? 0 : guild.Id); //{guildId}default value : 41312 Len : 4
                packet.WriteInt16(guild == null ? 0 : guild.Id); //{oneMoreId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(guild == null ? "" : guild.Name, 17); //{ClanName}default value :  Len : 17
                packet.WriteInt16(guild == null ? 0 : guild.Level); //value name : unk9 default value : 1Len : 2
                packet.WriteByte(guild == null ? 0 : guild.MaxMembersCount); //value name : unk10 default value : 15Len : 1
                packet.WriteInt32(guild == null ? 0 : guild.MemberCount); //value name : unk11 default value : 1Len : 4
                packet.WriteInt16(0); //value name : unk12 default value : 0Len : 2
                packet.WriteByte(0);//TODO New Eng Version!!!
                packet.WriteSkip(unk13); //value name : unk13 default value : unk13Len : 40
                packet.WriteInt16(guild == null ? 0 : (short)guild.Ranks[4].Privileges); //value name : unk14 default value : 0Len : 2
                packet.WriteInt16(guild == null ? 0 : (short)guild.Ranks[3].Privileges); //value name : unk15 default value : 96Len : 2
                packet.WriteInt16(guild == null ? 0 : (short)guild.Ranks[2].Privileges); //value name : unk16 default value : 120Len : 2
                packet.WriteInt16(guild == null ? 0 : (short)guild.Ranks[1].Privileges); //value name : unk17 default value : 127Len : 2
                packet.WriteInt32(guild == null ? 0 : guild.Leader.Character.AccId);
                //{leaderAccId}default value : 355335 Len : 4
                packet.WriteByte(10); //{charNum}default value : 11 Len : 1
                packet.WriteFixedAsciiString(guild == null ? "" : guild.Leader.Name, 20);
                //{leaderName}default value :  Len : 20
                packet.WriteSkip(stub106); //{stub106}default value : stub106 Len : 293
                client.Send(packet);
            }
            if (status != CreateGuildStatus.Ok)
                return;
            SendUpdateGuildInfoResponse(guild);
            var chr = client.ActiveCharacter;
            GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(chr);
            SendClanFlagAndClanNameInfoSelfResponse(chr);
        }

        private static readonly byte[] unk13 = new byte[]
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };

        private static readonly byte[] stub106 = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00
            };
        public static void SendSendInviteGuiledResponseResponse(IRealmClient client, Guild guild)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SendInviteGuiledResponse))//4114
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt32(guild.Leader.AccId);//{leaderAccId}default value : 355335 Len : 4
                packet.WriteInt16(7);//{leaderSessId}default value : 7 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{trigAccId}default value : 340701 Len : 4
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{trigSessId}default value : 0 Len : 2
                packet.WriteInt16(guild.Id);//{clanId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(guild.Name, 17);//{clanName}default value :  Len : 17
                client.Send(packet);
            }
        }
        public static void SendGuildSkillsInfoResponse(Character client)
        {
            using (var p = CreateGuildSkillInfoPacket(client.Guild, client.AccId))
            {
                client.Send(p, addEnd: true);
            }
        }

        public static void SendGuildSkillsInfoToGuild(Guild g)
        {
            foreach (var chr in g.GetCharacters())
            {
                using (var p = CreateGuildSkillInfoPacket(g, chr.AccId))
                {
                    chr.Send(p, addEnd: true);
                }
            }

        }
        public static RealmPacketOut CreateGuildSkillInfoPacket(Guild g, uint accId)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.GuildSkillsInfo);//4148

            packet.WriteInt32(accId);//{leaderAccId}default value : 355335 Len : 4
            //packet.WriteByte(0);//TODO New Eng Version!!!
            for (int i = 0; i < 10; i += 1)
            {
                var skill = g.Skills[i];
                packet.WriteInt16(skill == null ? -1 : (short)skill.Id);//{skillId}default value : -1 Len : 2
                packet.WriteByte(skill == null ? 0 : skill.Level);//{level}default value : 0 Len : 1
                packet.WriteByte(skill == null ? 0 : skill.IsActivated ? 1 : 0);//{activated}default value : 1 Len : 1

            }
            return packet;

        }
        public static void SendGuildInfoOnLoginResponse(Character rcvChar, Guild guild)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildInfoOnLogin)) //4104
            {
                packet.WriteInt32(rcvChar.AccId); //{accId}default value : 355335 Len : 4
                packet.WriteInt16(guild.Id); //{guildId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(guild.Name, 17); //{guildName}default value :  Len : 17
                packet.WriteInt16(guild.Level); //{guildLevel}default value : 1 Len : 2
                packet.WriteByte(guild.MaxMembersCount); //{maxBuildMembers}default value : 0 Len : 1
                packet.WriteByte(guild.MemberCount); //{curGuildMembers}default value : 0 Len : 1
                packet.WriteInt32(guild.Points); //{guildPoints}default value : 0 Len : 4
                packet.WriteByte(guild.WaveLimit); // Wave limit
                packet.WriteSkip(stab38); //value name : stab38 default value : stab38Len : 41
                packet.WriteInt16((short)guild.Ranks[4].Privileges); //value name : unk2 default value : 0Len : 2
                packet.WriteInt16((short)guild.Ranks[3].Privileges); //value name : unk12 default value : 96Len : 2
                packet.WriteInt16((short)guild.Ranks[2].Privileges); //value name : unk13 default value : 120Len : 2
                packet.WriteInt16((short)guild.Ranks[1].Privileges); //value name : unk14 default value : 127Len : 2
                packet.WriteInt32(guild.Leader.AccId); //{leaderAccId}default value : 355335 Len : 4
                packet.WriteByte(guild.MemberCount); //{membersCount}default value : 11 Len : 1
                packet.WriteFixedAsciiString(guild.Leader.Name, 20); //{leaderName}default value :  Len : 20
                packet.WriteFixedAsciiString(guild.MOTD, 256); //{guildNotice}default value :  Len : 256
                packet.WriteFixedAsciiString(guild.NoticeDateTime.ToString(CultureInfo.InvariantCulture), 17);//{guildNoticeDateTime}default value :  Len : 17
                packet.WriteFixedAsciiString(guild.NoticeWriter, 20); //{noticeWriter}default value :  Len : 20
                rcvChar.Send(packet, addEnd: true);
            }
        }

        private static readonly byte[] stab38 = new byte[]
            {
                0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };

        public static void SendGuildMembersInfoResponse(IRealmClient client, Guild guild)
        {
            var members = new List<List<GuildMember>>();
            int cnt = 5;
            List<GuildMember> curList = null;
            foreach (var member in guild)
            {
                if (cnt == 5)
                {
                    curList = new List<GuildMember>();
                    members.Add(curList);
                    cnt = 0;
                }
                curList.Add(member);
                cnt++;
            }
            foreach (var member in members)
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.GuildMembersInfo)) //4107
                {
                    for (int i = 0; i < member.Count; i += 1)
                    {
                        var m = member[i];
                        var chr = m.Character;
                        packet.WriteInt32(m.AccId); //{AccId}default value : 355335 Len : 4
                        packet.WriteByte(m.CharNum); //{CharNum}default value : 11 Len : 1
                        packet.WriteFixedAsciiString(m.Name, 20); //{memberName}default value :  Len : 20
                        packet.WriteByte(m.Level); //{level}default value : 20 Len : 1
                        packet.WriteByte(m.ProffessionLevel); //{proff}default value : 23 Len : 1
                        packet.WriteByte((byte)m.Class); //{class}default value : 7 Len : 1
                        packet.WriteByte(4 - m.RankId); //{rank}default value : 4 Len : 1
                        packet.WriteInt32(0); //value name : unk9 default value : 0Len : 4
                        packet.WriteByte(chr == null ? 0 : 1); //{loggedIn}default value : 1 Len : 1
                        packet.WriteAsdaString(m.LastLogin.ToShortDateString(), 17);
                        //{joinTimeString}default value : joinTimeString Len : 17
                        packet.WriteByte(1); //{chanel}default value : 2 Len : 1
                        packet.WriteByte((byte)(chr == null ? 0 : chr.Map.Id)); //{location}default value : 7 Len : 1
                        packet.WriteFixedAsciiString(m.PublicNote, 60); //{publicMessage}default value :  Len : 60
                        packet.WriteInt32(0); //value name : stab120 default value : stab120Len : 4
                    }
                    client.Send(packet);
                }
            }
            SendGuildMembersInfoEndedResponse(client);
        }

        public static void SendGuildMembersInfoEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildMembersInfoEnded)) //4108
            {
                client.Send(packet);
            }
        }


        [PacketHandler(RealmServerOpCode.SendInviteGuild)] //4113
        public static void SendInviteGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetAccId = packet.ReadUInt32(); //default : 355772Len : 4
            Character inviter = client.ActiveCharacter;
            Character invitee = World.GetCharacterByAccId(targetAccId);
            Guild guild = inviter.Guild;

            if (Guild.CheckInvite(inviter, invitee, invitee == null ? "" : invitee.Name) == GuildResult.SUCCESS)
            {
                var inviteRelation = RelationMgr.CreateRelation(inviter.EntityId.Low,
                                                                invitee.EntityId.Low, CharacterRelationType.GuildInvite);

                RelationMgr.Instance.AddRelation(inviteRelation);

                //SendResult(inviter.Client, GuildCommandId.INVITE, invitee.Name, GuildResult.SUCCESS);

                guild.EventLog.AddInviteEvent(inviter.EntityId.Low, invitee.EntityId.Low);
                inviter.SendSystemMessage(string.Format("You have invited {0}.", invitee.Name));
                // Target has been invited
                //SendGuildInvite(invitee.Client, inviter);

                SendJoinMyGuildRequestResponse(invitee, inviter, guild);
            }
        }
        [PacketHandler(RealmServerOpCode.ChangeClanMemberRank)]//4127
        public static void ChangeClanMemberRankRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var rank = packet.ReadByte();//default : 1Len : 2
            packet.Position += 1;
            var targetAccId = packet.ReadInt32();//default : 338847Len : 4
            var targetCharNumOnAcc = packet.ReadInt16();//default : 12Len : 2
            if (!client.ActiveCharacter.IsInGuild || rank > 3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("trying to change clan member rank with wrong paramtrs", 50);
                return;
            }
            var member =
                client.ActiveCharacter.Guild.Members[
                    Character.CharacterIdFromAccIdAndCharNum(targetAccId, targetCharNumOnAcc)];
            if (member == null)
            {
                SendChangeClanMemberRankAccessResultResponse(client, ChangeMemberRankAccessResult.ThereIsAnErrorInUserProfile);
                return;
            }
            if (rank > client.ActiveCharacter.Asda2GuildRank || member.Asda2RankId > client.ActiveCharacter.Asda2GuildRank)
            {
                SendChangeClanMemberRankAccessResultResponse(client, ChangeMemberRankAccessResult.YouCantMakeChangesToAUserOfHigherRankThanYou);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.SetMemberPrivilegies))
            {
                SendChangeClanMemberRankAccessResultResponse(client, ChangeMemberRankAccessResult.YouDontHavePermitionToUseThis);
                return;
            }
            member.Asda2RankId = rank;
            SendChangeClanMemberRankAccessResultResponse(client, ChangeMemberRankAccessResult.Ok);
            SendGuildNotificationResponse(client.ActiveCharacter.Guild, GuildNotificationType.RankChanged, member);
        }
        [PacketHandler(RealmServerOpCode.LevelUpGuild)]//4146
        public static void LevelUpGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to lvl up guild level", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.UsePoints))
            {
                client.ActiveCharacter.SendSystemMessage("You have not permitions to use points.");
                return;
            }
            var success = client.ActiveCharacter.Guild.LevelUp();
            if (success)
                SendClanFlagAndClanNameInfoSelfResponse(client.ActiveCharacter);
            LevelUpGuildResponse(client, success);
        }
        public static void LevelUpGuildResponse(IRealmClient client, bool success)
        {
            using (var p = new RealmPacketOut(RealmServerOpCode.GuildLeveluped))
            {
                p.Write(success);
                client.Send(p);
            }
        }
        [PacketHandler(RealmServerOpCode.DisolveGuild)]//4111
        public static void DisolveGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild || !client.ActiveCharacter.GuildMember.IsLeader)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to disolve guild with wrong parametrs", 50);
                DisolveGuildResponse(client, false);
                return;
            }
            if (client.ActiveCharacter.Guild.MemberCount > 1)
            {
                client.ActiveCharacter.SendSystemMessage("You must kick all members before disolving guild.");
                DisolveGuildResponse(client, false);
                return;
            }
            client.ActiveCharacter.Guild.Disband();
            DisolveGuildResponse(client, true);
            //SendYouHaveLeftGuildResponse(client.ActiveCharacter,true);
            //todo disolve guild
        }
        public static void DisolveGuildResponse(IRealmClient client, bool success)
        {
            using (var p = new RealmPacketOut(RealmServerOpCode.GuildDisolved))
            {
                p.Write(success);
                client.Send(p);
            }
        }
        [PacketHandler(RealmServerOpCode.RegisterGuildCrest)]//4129
        public static void RegisterGuildCrestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to register guild crest without guild", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.EditCrest))
            {
                client.ActiveCharacter.SendSystemMessage("You have not permitions to edit clan crest.");
                return;
            }
            packet.Position += 6;
            var slot = packet.ReadInt16();//default : 2Len : 2
            var inv = packet.ReadByte();//default : 1Len : 1
            Asda2Item item = null;
            try
            {
                item = client.ActiveCharacter.Asda2Inventory.ShopItems[slot];
            }
            catch (Exception)
            {
            }
            if (item == null || item.Category != Asda2ItemCategory.GuildCrest)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to register guild crest without wrong item", 10);
                return;
            }

            item.ModAmount(-1);
            var crestInfo = packet.ReadBytes(40);
            client.ActiveCharacter.Guild.ClanCrest = crestInfo;
            client.ActiveCharacter.SendSystemMessage("You have success edit crest.");

            /*var borderId = packet.ReadInt64();//default : 23Len : 8
            var borderArgb = packet.ReadInt32();//default : -1Len : 4
            packet.Position += 4;//tab56 default : stab56Len : 4
            var argbBackground = packet.ReadInt32();//default : -15648414Len : 4
            packet.Position += 4;//tab64 default : stab64Len : 4
            var centralSymbolId = packet.ReadInt64();//default : 3Len : 8
            var argbCrestColor = packet.ReadInt32();//default : -1Len : 4
            packet.Position += 4;//nk default : 0Len : 4*/
        }

        static readonly byte[] stab56 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] stab64 = new byte[] { 0x00, 0x00, 0x00, 0x00 };


        [PacketHandler(RealmServerOpCode.KickFromGuild)]//4119
        public static void KickFromGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to kick from guild without guild", 50);
                return;
            }
            packet.Position += 2;
            var targetAccId = packet.ReadInt32();//default : 340701Len : 4
            var targetCharNum = packet.ReadByte();//default : 11Len : 1
            //var targetName = packet.ReadAsdaString(20);//default : Len : 21
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.EditRankSettings))
            {
                client.ActiveCharacter.SendSystemMessage("You have not permitions to kick him.");
                return;
            }
            var target =
                client.ActiveCharacter.Guild.Members[
                    Character.CharacterIdFromAccIdAndCharNum(targetAccId, targetCharNum)];
            if (target == null)
            {
                client.ActiveCharacter.SendSystemMessage("Member not founded.");
                return;
            }
            if (target.IsLeader)
            {
                client.ActiveCharacter.SendSystemMessage("You can't kick leader.");
                return;
            }
            if (target.Asda2RankId > client.ActiveCharacter.Asda2GuildRank)
            {
                client.ActiveCharacter.SendSystemMessage("You can't kick Higher rank than yours.");
            }
            if (target.Character != null)
                SendYouHaveBeenKickedFromGuildResponse(target.Character);
            target.LeaveGuild(true);
        }
        public static void SendYouHaveBeenKickedFromGuildResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouHaveBeenKickedFromGuild))//4120
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteInt32(chr.AccId);//{accId0}default value : 340701 Len : 4
                packet.WriteFixedAsciiString(chr.Name, 20);//{name}default value :  Len : 20
                chr.Send(packet, addEnd: false);
            }
        }



        public static void SendChangeClanMemberRankAccessResultResponse(IRealmClient client, ChangeMemberRankAccessResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ChangeClanMemberRankAccessResult))//4128
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accIdRcv}default value : 340701 Len : 4
                client.Send(packet);
            }
        }

        public static void SendJoinMyGuildRequestResponse(Character rcv, Character sender, Guild guild)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.JoinMyGuildRequest))//4115
            {
                packet.WriteInt32(rcv.AccId);//{rcvAccId}default value : 340701 Len : 4
                packet.WriteInt16(rcv.SessionId);//{rcvSessId}default value : 0 Len : 2
                packet.WriteInt32(sender.AccId);//{senderAccId}default value : 355335 Len : 4
                packet.WriteInt16(sender.SessionId);//{senderSessId}default value : 7 Len : 2
                packet.WriteInt16(guild.Id);//{clanId}default value : 1436 Len : 2
                packet.WriteFixedAsciiString(guild.Name, 17);//{clanName}default value :  Len : 17
                rcv.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.JoinGuildResponse)]//4116
        public static void JoinGuildResponseRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            var status = (GuildInviteAnswerStatus)packet.ReadByte();//default : 1Len : 1

            var invitee = client.ActiveCharacter;

            var listInviters = RelationMgr.Instance.GetPassiveRelations(invitee.EntityId.Low,
                                                                        CharacterRelationType.GuildInvite);

            var invite = listInviters.FirstOrDefault();

            if (invite == null)
                return;

            Character inviter = World.GetCharacter(invite.CharacterId);
            if (inviter == null)
                return;

            //Removes the guild invite relation between the inviter and invitee
            RelationMgr.Instance.RemoveRelation(invite);
            if (status == GuildInviteAnswerStatus.Decline)
            {
                inviter.SendSystemMessage(string.Format("{0} declined your invation.", invitee.Name));
                return;
            }
            var guildMember = inviter.GuildMember;

            if (guildMember == null)
                return;

            var guild = guildMember.Guild;

            //Add the invitee to the guild
            guild.AddMember(invitee);
            inviter.SendSystemMessage(string.Format("{0} accepted your invation.", invitee.Name));
            SendSendInviteGuiledResponseResponse(invitee.Client, guild);
            SendClanFlagAndClanNameInfoSelfResponse(invitee);
            SendGuildInfoOnLoginResponse(invitee, guild);
            SendGuildMembersInfoResponse(invitee.Client, guild);
            var chr = invitee;
            GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(chr);
            SendGuildSkillsInfoResponse(chr);
            Asda2TitleChecker.OnJoinGuild(chr);
        }


        public static void SendYouHaveLeftGuildResponse(Character lefter, bool ok)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouHaveLeftGuild))//4118
            {
                packet.WriteByte(ok ? 1 : 0);//{status}default value : 1 Len : 1
                packet.WriteInt32(lefter.AccId);//{accId}default value : 340701 Len : 4
                lefter.Send(packet, addEnd: false);
            }
        }

        public static void SendUpdateGuildInfoResponse(Guild guild, GuildInfoMode mode = GuildInfoMode.Silent, Character rcv = null)
        {
            if (guild.Ranks == null)
                return;
            if (rcv != null)
                using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildInfo)) //4121
                {

                    packet.WriteByte(2); //value name : unk default value : 2Len : 2
                    packet.WriteByte((byte)mode); //value name : unk default value : 2Len : 2
                    packet.WriteInt16(guild.Id); //{guildId}default value : 1436 Len : 2
                    packet.WriteFixedAsciiString(guild.Name, 17); //{guildName}default value :  Len : 17
                    packet.WriteInt16(guild.Level); //{guildLevel}default value : 1 Len : 2
                    packet.WriteByte(guild.MaxMembersCount); //{maxMembers}default value : 15 Len : 1
                    packet.WriteByte(guild.MemberCount); //{membersCount}default value : 2 Len : 1
                    packet.WriteInt32(guild.Points); //{guildPoints}default value : 141 Len : 4
                    packet.WriteByte(guild.WaveLimit); //value name : unk12 default value : 0Len : 1
                    packet.WriteByte(0); //TODO New Eng Version!!!
                    for (int i = 0; i < 10; i += 1)
                    {
                        var skill = guild.Skills[i];
                        packet.WriteInt16(skill == null ? -1 : (short)skill.Id); //{skillId}default value : -1 Len : 2
                        packet.WriteByte(skill == null ? 0 : skill.Level); //{level}default value : 0 Len : 1
                        packet.WriteByte(skill == null ? 0 : skill.IsActivated ? 1 : 0);
                        //{activated}default value : 1 Len : 1
                    }
                    packet.WriteInt16((short)guild.Ranks[4].Privileges); //value name : unk2 default value : 0Len : 2
                    packet.WriteInt16((short)guild.Ranks[3].Privileges); //value name : unk12 default value : 96Len : 2
                    packet.WriteInt16((short)guild.Ranks[2].Privileges);
                    //value name : unk13 default value : 120Len : 2
                    packet.WriteInt16((short)guild.Ranks[1].Privileges);
                    //value name : unk14 default value : 127Len : 2
                    packet.WriteInt32(guild.Leader.AccId); //{leaderAccId}default value : 355335 Len : 4
                    packet.WriteByte(guild.Leader.CharNum); //{leaderCharNum}default value : 11 Len : 1
                    packet.WriteFixedAsciiString(guild.Leader.Name, 20); //{leaderName}default value :  Len : 20
                    packet.WriteFixedAsciiString(guild.MOTD, 293); //{introMsg}default value :  Len : 293

                    rcv.Send(packet, addEnd: true);
                }
            else
            {

                using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildInfo)) //4121
                {
                    packet.WriteByte(2); //value name : unk default value : 2Len : 2
                    packet.WriteByte((byte)mode); //value name : unk default value : 2Len : 2
                    packet.WriteInt16(guild.Id); //{guildId}default value : 1436 Len : 2
                    packet.WriteFixedAsciiString(guild.Name, 17);
                    //{guildName}default value :  Len : 17
                    packet.WriteInt16(guild.Level); //{guildLevel}default value : 1 Len : 2
                    packet.WriteByte(guild.MaxMembersCount); //{maxMembers}default value : 15 Len : 1
                    packet.WriteByte(guild.MemberCount); //{membersCount}default value : 2 Len : 1
                    packet.WriteInt32(guild.Points); //{guildPoints}default value : 141 Len : 4
                    packet.WriteByte(guild.WaveLimit); //value name : unk12 default value : 0Len : 1
                    packet.WriteByte(0);//TODO New Eng Version!!!
                    for (int i = 0; i < 10; i += 1)
                    {
                        var skill = guild.Skills[i];
                        packet.WriteInt16(skill == null ? -1 : (short)skill.Id); //{skillId}default value : -1 Len : 2
                        packet.WriteByte(skill == null ? 0 : skill.Level); //{level}default value : 0 Len : 1
                        packet.WriteByte(skill == null ? 0 : skill.IsActivated ? 1 : 0);
                        //{activated}default value : 1 Len : 1

                    }
                    packet.WriteInt16((short)guild.Ranks[4].Privileges); //value name : unk2 default value : 0Len : 2
                    packet.WriteInt16((short)guild.Ranks[3].Privileges); //value name : unk12 default value : 96Len : 2
                    packet.WriteInt16((short)guild.Ranks[2].Privileges);
                    //value name : unk13 default value : 120Len : 2
                    packet.WriteInt16((short)guild.Ranks[1].Privileges);
                    //value name : unk14 default value : 127Len : 2
                    packet.WriteInt32(guild.Leader.AccId); //{leaderAccId}default value : 355335 Len : 4
                    packet.WriteByte(guild.Leader.CharNum); //{leaderCharNum}default value : 11 Len : 1
                    packet.WriteFixedAsciiString(guild.Leader.Name, 20);
                    //{leaderName}default value :  Len : 20
                    packet.WriteFixedAsciiString(guild.MOTD, 293);
                    //{introMsg}default value :  Len : 293

                    guild.Send(packet, addEnd: true);
                }
            }

        }
        static readonly byte[] stab44 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        [PacketHandler(RealmServerOpCode.GuildChatReq)]//4123
        public static void GuildChatReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            var msg = packet.ReadAsciiString(client.Locale);//default : Len : 0
            if (!client.ActiveCharacter.IsInGuild || msg.Length > 200)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Guild chat request without guild.", 50);
                return;
            }
            if (client.ActiveCharacter.ChatBanned)
            {
                client.ActiveCharacter.SendInfoMsg(" Your chat Is Banned");
                return;
            }
            if (msg.Length < 1 || Commands.RealmCommandHandler.HandleCommand(client.ActiveCharacter, msg, client.ActiveCharacter.Target as Character))
                return;
            var locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, msg);
            SendGuildChatResResponse(msg, client.ActiveCharacter.Guild, client.ActiveCharacter, locale);
        }
        public static void SendGuildChatResResponse( string msg, Guild guild, Character sender, Locale locale)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildChatRes)) //4124
            {
                packet.WriteInt16(guild.Id); //{clanId}default value : 1436 Len : 2
                packet.WriteInt32(sender.AccId); //{accId}default value : 340701 Len : 4
                packet.WriteInt16(sender.SessionId); //{sessId}default value : 0 Len : 2
                packet.WriteFixedAsciiString(sender.Name, 20); //{name}default value :  Len : 20
                packet.WriteAsdaString(msg, msg.Length); //{msg}default value :  Len : 0
                
              
                guild.Send(packet, false, locale);

            }
        }

        [PacketHandler(RealmServerOpCode.WritePublicGuildMemberNote)]//4133
        public static void WritePublicGuildMemberNoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to write public guild member note request with out guild.", 50);
                return;
            }
            if (client.ActiveCharacter.ChatBanned)
            {
                client.ActiveCharacter.SendInfoMsg("you are banned you can't do this +)");
                return;
            }
            packet.Position += 2;
            var message = packet.ReadAsdaString(64, Locale.En);//default : Len : 64
            /*var isPruEng = Asda2EncodingHelper.IsPrueEnglish(message);
            if (!isPruEng)
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("member note");
                SendEditPublicGuildNoteResponse(client, false);
                return;
            }*/
            SendEditPublicGuildNoteResponse(client, true);
            client.ActiveCharacter.GuildMember.PublicNote = message;

        }
        public static void SendEditPublicGuildNoteResponse(IRealmClient client, bool b)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.EditPublicGuildNote))//4134
            {
                packet.WriteByte(b ? 1 : 0);//{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{trigerAccId}default value : 355335 Len : 4
                client.Send(packet);
            }
        }
        [PacketHandler(RealmServerOpCode.EditClanNotice)]//4131
        public static void EditClanNoticeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.ChatBanned)
            {
                client.ActiveCharacter.SendInfoMsg(" you are banned you can't! +)");
                return;
            }
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Edit clan notice request without guild", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.EditAnnounce))
            {
                client.ActiveCharacter.SendSystemMessage("You don't have permitins to edit Announce.");
                SendAnnouncementEditedResponse(client, false);
                return;
            }
            packet.Position += 2;
            var message = packet.ReadAsdaString(260, Locale.En);//default : Len : 262
            var isPruEng = Asda2EncodingHelper.IsPrueEnglish(message);
           /* if (!isPruEng)
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("notice");
                SendEditPublicGuildNoteResponse(client, false);
                return;
            }*/
            client.ActiveCharacter.Guild.MOTD = message;
            SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild, GuildInfoMode.Announcement);
            SendAnnouncementEditedResponse(client, true);
        }
        public static void SendAnnouncementEditedResponse(IRealmClient client, bool success)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AnnouncementEdited))//4132
            {
                packet.WriteByte(success ? 1 : 0);//{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 355335 Len : 4
                client.Send(packet);
            }
        }



        [PacketHandler(RealmServerOpCode.AskForGuildHistory)]//4139
        public static void AskForGuildHistoryRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to act guild history while not in guild.", 50);
                return;
            }
            SendGuildHistoryResponse(client, client.ActiveCharacter.Guild);
        }
        public static void SendGuildHistoryResponse(IRealmClient client, Guild guild)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildHistory))//4140
            {
                for (int i = 0; i < 12; i += 1)
                {
                    if (guild.History.Count <= i)
                        break;
                    var rec = guild.History[i];

                    packet.WriteInt16(guild.Id);//{uildId}default value : 471 Len : 2
                    packet.WriteByte(rec.Type);//{messageType}default value : 6 Len : 1
                    packet.WriteInt32(rec.Value);//{messageArg}default value : 82 Len : 4
                    packet.WriteFixedAsciiString(rec.TrigerName, 20);//{messageChar}default value :  Len : 20
                    packet.WriteFixedAsciiString(rec.Time, 17);//{messageDateTime}default value :  Len : 17
                }
                client.Send(packet);
                using (var p = new RealmPacketOut(RealmServerOpCode.GuildHistoryEnded))//4141
                {
                    client.Send(p);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.SetPrivilegies)]//4125
        public static void SetPrivilegiesRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to set privilegies while not in guild.", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.EditRankSettings))
            {
                SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.HaveNotPermitions,
                                               (short)client.ActiveCharacter.Guild.Id);
            }
            packet.Position += 2;
            client.ActiveCharacter.Guild.Ranks[4].Privileges = (GuildPrivileges)packet.ReadInt16();//default : 0Len : 2
            client.ActiveCharacter.Guild.Ranks[3].Privileges = (GuildPrivileges)packet.ReadInt16();//default : 0Len : 2
            client.ActiveCharacter.Guild.Ranks[2].Privileges = (GuildPrivileges)packet.ReadInt16();//default : 0Len : 2
            client.ActiveCharacter.Guild.Ranks[1].Privileges = (GuildPrivileges)packet.ReadInt16();//default : 0Len : 2
            SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild, GuildInfoMode.GuildPrivilegiesChanged);
            SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.Ok, (short)client.ActiveCharacter.GuildId);
        }

        public static void SendPrivilagiesChangedResponse(IRealmClient client, ChangePrivilegiesStatus status, short clanId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PrivilagiesChanged))//4126
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(clanId);//{clanId}default value : 1436 Len : 2
                client.Send(packet);
            }
        }
        [PacketHandler(RealmServerOpCode.LeaveGuild)]//4117
        public static void LeaveGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Leave guild while not in guild", 10);
                SendYouHaveLeftGuildResponse(client.ActiveCharacter, false);
                return;
            }
            SendYouHaveLeftGuildResponse(client.ActiveCharacter, true);
            client.ActiveCharacter.GuildMember.LeaveGuild();
        }


        public static void SendClanFlagAndClanNameInfoSelfResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ClanFlagAndClanNameInfoSelf))//4105
            {
                packet.WriteInt32(chr.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteInt32(chr.GuildPoints);//{guildPoints}default value : 491 Len : 4
                packet.WriteSkip(stab15);//value name : stab15 default value : stab15Len : 4
                packet.WriteInt16(chr.GuildId);//{guildId}default value : 471 Len : 2
                packet.WriteFixedAsciiString(chr.Guild.Name, 17);//{guildName}default value :  Len : 17
                packet.WriteInt32(chr.Asda2GuildRank);//{rank}default value : 2 Len : 4
                packet.WriteByte(3);
                packet.WriteByte(1);
                packet.WriteSkip(chr.Guild.ClanCrest);//value name : unk13 default value : unk13Len : 40
                packet.WriteFixedAsciiString(chr.GuildMember.PublicNote, 60);//{publicNote}default value :  Len : 60
                packet.WriteInt32(0);//value name : unk4 default value : 0Len : 4
                chr.Send(packet, addEnd: false);
            }
        }
        static readonly byte[] stab15 = new byte[] { 0xBE, 0x0B, 0x00, 0x00 };
        static readonly byte[] Unk13 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public static void SendGuildNotificationResponse(Guild guild, GuildNotificationType status, GuildMember trigerer)
        {
            if (trigerer == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildNotification))//4122
            {
                packet.WriteByte(2);//{status}default value : 2 Len : 1
                packet.WriteByte((byte)status);//{status1}default value : 8 Len : 1
                packet.WriteInt16(guild.Id);//{guildId}default value : 1436 Len : 2
                packet.WriteInt32(trigerer.AccId);//{accId}default value : 355335 Len : 4
                packet.WriteByte(trigerer.CharNum);//{rank}default value : 11 Len : 1
                packet.WriteFixedAsciiString(trigerer.Name, 20);//{trigererName}default value :  Len : 20
                packet.WriteByte(trigerer.Level);//{trigererLevel}default value : 20 Len : 1
                packet.WriteByte(trigerer.Character == null ? 0 : trigerer.Character.ProfessionLevel);//{trigererProff}default value : 23 Len : 1
                packet.WriteByte((byte)(trigerer.Character == null ? 0 : trigerer.Character.Archetype.ClassId));//{trigererClass}default value : 7 Len : 1
                packet.WriteByte(trigerer.Asda2RankId);//{rank0}default value : 4 Len : 1
                packet.WriteInt32(trigerer.Character == null ? 0 : trigerer.Character.GuildPoints);//{guildPoints}default value : 0 Len : 4
                packet.WriteByte(trigerer.Character == null ? 0 : trigerer.Character.IsLoggingOut ? 0 : 1);//{isOnline}default value : 1 Len : 1
                packet.WriteSkip(stab45);//value name : stab45 default value : stab45Len : 17
                packet.WriteByte(1);//{chanel}default value : 3 Len : 1
                packet.WriteByte((byte)(trigerer.Character == null ? 0 : trigerer.Character.MapId));//{locNum}default value : 7 Len : 1
                packet.WriteFixedAsciiString(trigerer.PublicNote, 60);//{publicNote}default value :  Len : 60
                packet.WriteSkip(stab124);//value name : stab124 default value : stab124Len : 4
                guild.Send(packet, addEnd: true);
            }

        }
        static readonly byte[] stab45 = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] stab124 = new byte[] { 0x00, 0x00, 0x00, 0x00 };

        #region skills
        [PacketHandler(RealmServerOpCode.LearnClanSkill)]//4161
        public static void LearnClanSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var skillId = packet.ReadInt16();//default : 1Len : 2
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Lerning clan skill while not in guild.", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.UsePoints))
            {
                SendClanSkillLearnedResponse(client.ActiveCharacter, null, LearnGuildSkillResult.YouDontHavePermitionToDoThis);
                return;
            }
            if (skillId < 0 || skillId > 4)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Lerning wrong clan skill Id.", 10); return;
            }
            GuildSkill skill;
            var result = client.ActiveCharacter.Guild.TryLearnSkill((GuildSkillId)skillId, out skill);
            SendClanSkillLearnedResponse(client.ActiveCharacter, skill, result);
        }
        public static void SendClanSkillLearnedResponse(Character chr, GuildSkill skill, LearnGuildSkillResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ClanSkillLearned))//4162
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(chr.AccId);//{accId}default value : 0 Len : 4
                packet.WriteInt16(chr.Guild.Id);//{guildId}default value : 0 Len : 2
                packet.WriteInt16((short)(skill == null ? 0 : skill.Id));//{skillId}default value : 0 Len : 2
                packet.WriteByte(skill == null ? 0 : skill.Level);//{skillLevel}default value : 0 Len : 1
                chr.Send(packet, addEnd: false);
            }
        }
        public static void SendGuildSkillStatusChangedResponse(GuildSkill skill, ClanSkillStatus status)
        {
            if (skill == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildSkillStatusChanged))//4149
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(skill.Guild.Id);//{clanId}default value : 1436 Len : 2
                packet.WriteInt16((short)skill.Id);//{skillId}default value : 1 Len : 2
                packet.WriteByte(skill.Level);//{skilllevel}default value : 1 Len : 1
                packet.WriteByte(skill.IsActivated);//{activated}default value : 0 Len : 1
                skill.Guild.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.ActivateGuildSkill)]//4163
        public static void ActivateGuildSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Activating guild skill while not in guild", 50);
                return;
            }
            if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(GuildPrivileges.UsePoints))
            {
                SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.HaveNotPermitions,
                                               (short)client.ActiveCharacter.Guild.Id);
            }
            var skillId = packet.ReadInt16();//default : 1Len : 2
            if (skillId < 0 || skillId > 4 || client.ActiveCharacter.Guild.Skills[skillId] == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater(); return;
            }
            client.ActiveCharacter.Guild.Skills[skillId].ToggleActivate(client.ActiveCharacter);

        }
        public static void SendGuildSkillActivatedResponse(Character activator, GuildSkillActivationStatus status, GuildSkill skill)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildSkillActivated))//4164
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(activator.AccId);//{activatorAccId}default value : 355335 Len : 4
                packet.WriteInt16(activator.Guild.Id);//{guildId}default value : 1436 Len : 2
                packet.WriteInt16((short)skill.Id);//{skillId}default value : 1 Len : 2
                packet.WriteByte(skill.Level);//{activated}default value : 0 Len : 1
                activator.Send(packet, addEnd: false);
            }
        }



        #endregion
        [PacketHandler(RealmServerOpCode.DonateGuildPoints)]//4144
        public static void DonateGuildPointsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var guildPointsToDonate = packet.ReadInt32();//default : 25Len : 4
            if (!client.ActiveCharacter.IsInGuild || client.ActiveCharacter.GuildPoints < guildPointsToDonate || guildPointsToDonate < 0)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Donating guild points while not in guild", 10);
                SendGuildPointsDonatedResponse(client, false);
                return;
            }
            client.ActiveCharacter.GuildPoints -= guildPointsToDonate;
            client.ActiveCharacter.Guild.AddGuildPoints(guildPointsToDonate);
            client.ActiveCharacter.Guild.AddHistoryMessage(Asda2GuildHistoryType.DonatedPoints, guildPointsToDonate, client.ActiveCharacter.Name, DateTime.Now.ToLongTimeString());
            SendGuildPointsDonatedResponse(client, true);
            SendGuildNotificationResponse(client.ActiveCharacter.Guild, GuildNotificationType.DonatedPoints, client.ActiveCharacter.GuildMember);

        }

        public static void SendGuildPointsDonatedResponse(IRealmClient client, bool success)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.GuildPointsDonated))//4145
            {
                packet.WriteByte(success ? 1 : 0);//{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{donatorAccId}default value : 355335 Len : 4
                packet.WriteInt16(client.ActiveCharacter.GuildId);
                packet.WriteInt32(client.ActiveCharacter.GuildPoints);
                client.Send(packet, addEnd: false);
            }
        }

    }
    public enum GuildSkillActivationStatus
    {
        Fail = 0,
        Ok = 1,
        NoPermition = 5,
        IncefitientPoints = 7,

    }
    public enum GuildInviteAnswerStatus
    {
        Decline = 0,
        Accept = 1,
    }
    public enum ClanSkillStatus
    {
        Learned = 1,
        Activation = 2,
    }
    public enum GuildInfoMode
    {
        Silent = 0,
        Announcement = 1,
        GuildCrestChanged = 2,
        GuildPrivilegiesChanged = 3,
        GuildLevelChanged = 5,
        GuildNameChanged = 6
    }

    public enum ChangePrivilegiesStatus
    {
        Fail = 0,
        Ok = 1,
        HaveNotPermitions = 4
    }
    public enum CreateGuildStatus
    {
        YouMustBeLevel10And1StJobToCreateClan = 0,
        Ok = 1,
        YouMustCompleteGuildRightsQuestToCreateGuild = 2,
        NotEnoghtMoney = 4,
        YouAreInAnotherGuild = 5,
        YouCantCreateGuildWith = 7,
        YouCantCreateAnotherGuild = 8,
        GuildNameAlreadyExist = 9,
        YouHaveChoosedTheInvalidGuildName = 10,

    }
    public enum ChangeMemberRankAccessResult
    {
        FailedToChangeGuildRank = 0,
        Ok = 1,
        ThereIsAnErrorInUserProfile = 1,
        YouAreNotInGuild = 3,
        ThereIsAProblemWithGuildInformation = 4,
        CantChangeTheGuildLeaderPrivilegies = 5,
        YouDontHavePermitionToUseThis = 6,
        YouCantChangeThisRank = 7,
        YouCantAddMoreViceGuildLeaders = 8,
        YouCantMakeChangesToAUserOfHigherRankThanYou = 9,

    }
    public enum LeaveGuildStatus
    {
        Fail = 0,
        Ok = 1,
        ErrorInProfileInfo = 2,
        YouAreNotInGuild = 3,
        ErrorGuildInfo = 4,
    }
    public enum GuildRanks
    {
        Initiate = 0,
        Member = 1,
        Veteran = 2,
        Officer = 3,
        ClanLeader = 4,
        GuildLeaderCantLeftGuild = 5,

    }
    public enum GuildNotificationType
    {
        DonatedPoints = 0,
        Joined = 1,//1 1
        Left = 2,//2 8 update guild member info
        Kicked = 3,
        LoggedIn = 4,
        LoggedOut = 5,
        Silence = 6,
        RankChanged = 7,
        EditPublicMessage = 8,
        ApointedAsNewGuildLeader = 9,
    }
    public enum SendGuildInviteStatus
    {
        CannotJoinTheGuild = 0,
        Ok = 1,
        AProblemWasFoundedInUserProfile = 2,
        AProblemWithGuildInformation = 4,
        YouCantInviteMembersOfAnotherGuild = 6,
        GuildInvitionHasBeenRefused = 7,
        YouMustInviteCharacterFromSameFactionAsYours = 8,
        YourGuildRosterIsFullYouCannotAddMoreMembers = 10,
        YouDontHavePermitionsToUseThis = 11,
    }
    public enum LearnGuildSkillResult
    {
        Failed = 0,
        Ok = 1,
        ProblemInUserProfile = 2,
        YouAreNotInAGuild = 3,
        ProblemWithGuildInfo = 4,
        YouDontHavePermitionToDoThis = 5,
        ProlemWithSkillInfo = 6,
        ThisIsTheMaxLevelOfSkill = 7,
        GuildLevelIsNotEnoght = 8,
        IncifitientPoints = 9,
        CantLevelupCurrentActivatedSkills = 10,

    }
}
