using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2GuildHandler
    {
        private static readonly byte[] unk13 = new byte[40]
        {
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
            byte.MaxValue
        };

        private static readonly byte[] stub106 = new byte[293];

        private static readonly byte[] stab38 = new byte[40]
        {
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
            byte.MaxValue
        };

        private static readonly byte[] stab56 = new byte[4];
        private static readonly byte[] stab64 = new byte[4];

        private static readonly byte[] stab44 = new byte[33]
        {
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
            byte.MaxValue
        };

        private static readonly byte[] stab15 = new byte[4]
        {
            (byte) 190,
            (byte) 11,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] Unk13 = new byte[40]
        {
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
            byte.MaxValue
        };

        private static readonly byte[] stab45 = new byte[17]
        {
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab124 = new byte[4];

        [PacketHandler(RealmServerOpCode.ImpeachmentReuest)]
        public static void ImpeachmentReuestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild || client.ActiveCharacter.Asda2GuildRank != (byte) 3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to do ImpeachmentRequest with wrong parametrs.",
                    50);
            }
            else
            {
                int impeachment =
                    (int) client.ActiveCharacter.Guild.CreateImpeachment(client.ActiveCharacter.GuildMember);
            }
        }

        public static void SendImpeachmentStatusResponse(Character chr,
            Asda2GuildHandler.CreateImpeachmentResult status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentStatus))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(chr.GuildId);
                packet.WriteInt32(chr.AccId);
                chr.Send(packet, true);
            }
        }

        public static void SendImpeachmentAnswerResponse(Guild g, string newLeaderName)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentAnswer))
            {
                packet.WriteInt16(g.Id);
                packet.WriteFixedAsciiString(g.Leader.Name, 20, Locale.Start);
                packet.WriteFixedAsciiString(newLeaderName, 20, Locale.Start);
                g.Send(packet, true, Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.ChangeGuildLeaderRequest)]
        public static void ChangeGuildLeaderRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            int targetAccId = packet.ReadInt32();
            byte num = packet.ReadByte();
            if (!client.ActiveCharacter.IsInGuild || !client.ActiveCharacter.GuildMember.IsLeader)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to change leader with wrong parametrs.", 50);
            }
            else
            {
                GuildMember member =
                    client.ActiveCharacter.Guild.Members[
                        Character.CharacterIdFromAccIdAndCharNum(targetAccId, (short) num)];
                if (member == null)
                {
                    client.ActiveCharacter.SendSystemMessage("Can't find target character.");
                }
                else
                {
                    client.ActiveCharacter.GuildMember.Asda2RankId = (byte) 3;
                    Asda2GuildHandler.SendGuildNotificationResponse(client.ActiveCharacter.Guild,
                        GuildNotificationType.RankChanged, client.ActiveCharacter.GuildMember);
                    member.Asda2RankId = (byte) 4;
                    client.ActiveCharacter.Guild.Leader = member;
                    Asda2GuildHandler.SendGuildNotificationResponse(client.ActiveCharacter.Guild,
                        GuildNotificationType.ApointedAsNewGuildLeader, member);
                    client.ActiveCharacter.Guild.AddHistoryMessage(Asda2GuildHistoryType.ApointedAsGuildLeader, 0,
                        member.Name, DateTime.Now.ToLongTimeString());
                    Asda2GuildHandler.SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild, GuildInfoMode.Silent,
                        (Character) null);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.ImpeachmentVote)]
        public static void ImpeachmentVoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("trying to do impeachment vote with wrong parametrs", 50);
            }
            else
            {
                packet.Position += 2;
                if (!packet.ReadBoolean())
                    return;
                client.ActiveCharacter.Guild.AddImpeachmentVote(client.ActiveCharacter.GuildMember);
            }
        }

        public static void SendImpeachmentResultResponse(Guild g, Asda2GuildHandler.ImpeachmentResult result)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ImpeachmentResult))
            {
                packet.WriteByte((byte) result);
                packet.WriteInt16(g.Id);
                packet.WriteFixedAsciiString(g.Leader.Name, 20, Locale.Start);
                g.Send(packet, true, Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.ReqUpdateGuildPoints)]
        public static void ReqUpdateGuildPointsRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2GuildHandler.SendUpdateGuildPointsResponse(client);
        }

        public static void SendUpdateGuildPointsResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildPoints))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(client.ActiveCharacter.GuildPoints);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.CreateGuild)]
        public static void CreateGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            string clanName = packet.ReadAsdaString(17, Locale.Start);
            if (client.ActiveCharacter.Asda2FactionId == (short) -1 ||
                client.ActiveCharacter.Asda2FactionId == (short) 2)
            {
                client.ActiveCharacter.SendInfoMsg("Wrong faction to create guild.");
                Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.YouCantCreateGuildWith,
                    (Guild) null);
            }
            else if (client.ActiveCharacter.IsInGuild)
                Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.YouAreInAnotherGuild,
                    (Guild) null);
            else if (!Asda2CharacterHandler.IsNameValid(clanName))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("guild name");
                Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.YouHaveChoosedTheInvalidGuildName,
                    (Guild) null);
            }
            else if (GuildMgr.GetGuild(clanName) != null)
                Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.GuildNameAlreadyExist,
                    (Guild) null);
            else if (client.ActiveCharacter.Level < 10)
                Asda2GuildHandler.SendGuildCreatedResponse(client,
                    CreateGuildStatus.YouMustBeLevel10And1StJobToCreateClan, (Guild) null);
            else if (!client.ActiveCharacter.SubtractMoney(10000U))
                Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.NotEnoghtMoney, (Guild) null);
            else
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    Asda2GuildHandler.SendGuildCreatedResponse(client, CreateGuildStatus.Ok,
                        new Guild(client.ActiveCharacter.Record, clanName));
                    client.ActiveCharacter.SendMoneyUpdate();
                    client.ActiveCharacter.Map.CallDelayed(750,
                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Leader106)));
                }));
        }

        public static void SendGuildCreatedResponse(IRealmClient client, CreateGuildStatus status, Guild guild)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildCreated))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(guild == null ? 0U : guild.Id);
                packet.WriteInt16(guild == null ? 0U : guild.Id);
                packet.WriteFixedAsciiString(guild == null ? "" : guild.Name, 17, Locale.Start);
                packet.WriteInt16(guild == null ? 0 : (int) guild.Level);
                packet.WriteByte(guild == null ? 0 : (int) guild.MaxMembersCount);
                packet.WriteInt32(guild == null ? 0 : guild.MemberCount);
                packet.WriteInt16(0);
                packet.WriteByte(0);
                packet.WriteSkip(Asda2GuildHandler.unk13);
                packet.WriteInt16(guild == null ? 0 : (int) (short) guild.Ranks[4].Privileges);
                packet.WriteInt16(guild == null ? 0 : (int) (short) guild.Ranks[3].Privileges);
                packet.WriteInt16(guild == null ? 0 : (int) (short) guild.Ranks[2].Privileges);
                packet.WriteInt16(guild == null ? 0 : (int) (short) guild.Ranks[1].Privileges);
                packet.WriteInt32(guild == null ? 0U : guild.Leader.Character.AccId);
                packet.WriteByte(10);
                packet.WriteFixedAsciiString(guild == null ? "" : guild.Leader.Name, 20, Locale.Start);
                packet.WriteSkip(Asda2GuildHandler.stub106);
                client.Send(packet, false);
            }

            if (status != CreateGuildStatus.Ok)
                return;
            Asda2GuildHandler.SendUpdateGuildInfoResponse(guild, GuildInfoMode.Silent, (Character) null);
            Character activeCharacter = client.ActiveCharacter;
            GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(activeCharacter);
            Asda2GuildHandler.SendClanFlagAndClanNameInfoSelfResponse(activeCharacter);
        }

        public static void SendSendInviteGuiledResponseResponse(IRealmClient client, Guild guild)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SendInviteGuiledResponse))
            {
                packet.WriteByte(1);
                packet.WriteInt32(guild.Leader.AccId);
                packet.WriteInt16(7);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt16(guild.Id);
                packet.WriteFixedAsciiString(guild.Name, 17, Locale.Start);
                client.Send(packet, false);
            }
        }

        public static void SendGuildSkillsInfoResponse(Character client)
        {
            using (RealmPacketOut guildSkillInfoPacket =
                Asda2GuildHandler.CreateGuildSkillInfoPacket(client.Guild, client.AccId))
                client.Send(guildSkillInfoPacket, true);
        }

        public static void SendGuildSkillsInfoToGuild(Guild g)
        {
            foreach (Character character in g.GetCharacters())
            {
                using (RealmPacketOut guildSkillInfoPacket =
                    Asda2GuildHandler.CreateGuildSkillInfoPacket(g, character.AccId))
                    character.Send(guildSkillInfoPacket, true);
            }
        }

        public static RealmPacketOut CreateGuildSkillInfoPacket(Guild g, uint accId)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.GuildSkillsInfo);
            realmPacketOut.WriteInt32(accId);
            for (int index = 0; index < 10; ++index)
            {
                GuildSkill skill = g.Skills[index];
                realmPacketOut.WriteInt16(skill == null ? -1 : (int) skill.Id);
                realmPacketOut.WriteByte(skill == null ? 0 : (int) skill.Level);
                realmPacketOut.WriteByte(skill == null ? 0 : (skill.IsActivated ? 1 : 0));
            }

            return realmPacketOut;
        }

        public static void SendGuildInfoOnLoginResponse(Character rcvChar, Guild guild)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildInfoOnLogin))
            {
                packet.WriteInt32(rcvChar.AccId);
                packet.WriteInt16(guild.Id);
                packet.WriteFixedAsciiString(guild.Name, 17, Locale.Start);
                packet.WriteInt16(guild.Level);
                packet.WriteByte(guild.MaxMembersCount);
                packet.WriteByte(guild.MemberCount);
                packet.WriteInt32(guild.Points);
                packet.WriteByte(4);
                packet.WriteByte(guild.ClanCrest[0] != (byte) 0 ? 1 : 0);
                packet.Write(guild.ClanCrest[0] != (byte) 0 ? guild.ClanCrest : Asda2GuildHandler.stab38);
                packet.WriteInt16((short) guild.Ranks[4].Privileges);
                packet.WriteInt16((short) guild.Ranks[3].Privileges);
                packet.WriteInt16((short) guild.Ranks[2].Privileges);
                packet.WriteInt16((short) guild.Ranks[1].Privileges);
                packet.WriteInt32(guild.Leader.AccId);
                packet.WriteByte(guild.Leader.CharNum);
                packet.WriteFixedAsciiString(guild.Leader.Name, 20, Locale.Start);
                packet.WriteFixedAsciiString(guild.MOTD, 256, Locale.Start);
                packet.WriteFixedAsciiString(
                    guild.NoticeDateTime.ToString((IFormatProvider) CultureInfo.InvariantCulture), 17, Locale.Start);
                packet.WriteFixedAsciiString(guild.NoticeWriter, 20, Locale.Start);
                rcvChar.Send(packet, true);
            }
        }

        public static void SendGuildMembersInfoResponse(IRealmClient client, Guild guild)
        {
            List<List<GuildMember>> guildMemberListList = new List<List<GuildMember>>();
            int num = 5;
            List<GuildMember> guildMemberList1 = (List<GuildMember>) null;
            foreach (GuildMember guildMember in guild)
            {
                if (num == 5)
                {
                    guildMemberList1 = new List<GuildMember>();
                    guildMemberListList.Add(guildMemberList1);
                    num = 0;
                }

                guildMemberList1.Add(guildMember);
                ++num;
            }

            foreach (List<GuildMember> guildMemberList2 in guildMemberListList)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildMembersInfo))
                {
                    for (int index = 0; index < guildMemberList2.Count; ++index)
                    {
                        GuildMember guildMember = guildMemberList2[index];
                        Character character = guildMember.Character;
                        packet.WriteInt32(guildMember.AccId);
                        packet.WriteByte(guildMember.CharNum);
                        packet.WriteFixedAsciiString(guildMember.Name, 20, Locale.Start);
                        packet.WriteByte(guildMember.Level);
                        packet.WriteByte(guildMember.ProffessionLevel);
                        packet.WriteByte((byte) guildMember.Class);
                        packet.WriteByte(4 - guildMember.RankId);
                        packet.WriteInt32(0);
                        packet.WriteByte(character == null ? 0 : 1);
                        packet.WriteAsdaString(guildMember.LastLogin.ToShortDateString(), 17, Locale.Start);
                        packet.WriteByte(1);
                        packet.WriteByte(character == null ? (byte) 0 : (byte) character.Map.Id);
                        packet.WriteFixedAsciiString(guildMember.PublicNote, 60, Locale.Start);
                        packet.WriteInt32(0);
                    }

                    client.Send(packet, false);
                }
            }

            Asda2GuildHandler.SendGuildMembersInfoEndedResponse(client);
        }

        public static void SendGuildMembersInfoEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildMembersInfoEnded))
                client.Send(packet, false);
        }

        [PacketHandler(RealmServerOpCode.SendInviteGuild)]
        public static void SendInviteGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            uint accId = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            Character characterByAccId = World.GetCharacterByAccId(accId);
            Guild guild = activeCharacter.Guild;
            if (Guild.CheckInvite(activeCharacter, characterByAccId,
                    characterByAccId == null ? "" : characterByAccId.Name) != GuildResult.SUCCESS)
                return;
            Singleton<RelationMgr>.Instance.AddRelation(RelationMgr.CreateRelation(activeCharacter.EntityId.Low,
                characterByAccId.EntityId.Low, CharacterRelationType.GuildInvite));
            guild.EventLog.AddInviteEvent(activeCharacter.EntityId.Low, characterByAccId.EntityId.Low);
            activeCharacter.SendSystemMessage(string.Format("You have invited {0}.", (object) characterByAccId.Name));
            Asda2GuildHandler.SendJoinMyGuildRequestResponse(characterByAccId, activeCharacter, guild);
        }

        [PacketHandler(RealmServerOpCode.ChangeClanMemberRank)]
        public static void ChangeClanMemberRankRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            byte num = packet.ReadByte();
            ++packet.Position;
            int targetAccId = packet.ReadInt32();
            short targetCharNumOnAcc = packet.ReadInt16();
            if (!client.ActiveCharacter.IsInGuild || num > (byte) 3)
            {
                client.ActiveCharacter.YouAreFuckingCheater("trying to change clan member rank with wrong paramtrs",
                    50);
            }
            else
            {
                GuildMember member =
                    client.ActiveCharacter.Guild.Members[
                        Character.CharacterIdFromAccIdAndCharNum(targetAccId, targetCharNumOnAcc)];
                if (member == null)
                    Asda2GuildHandler.SendChangeClanMemberRankAccessResultResponse(client,
                        ChangeMemberRankAccessResult.Ok);
                else if ((int) num > (int) client.ActiveCharacter.Asda2GuildRank ||
                         (int) member.Asda2RankId > (int) client.ActiveCharacter.Asda2GuildRank)
                    Asda2GuildHandler.SendChangeClanMemberRankAccessResultResponse(client,
                        ChangeMemberRankAccessResult.YouCantMakeChangesToAUserOfHigherRankThanYou);
                else if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag(
                    (Enum) GuildPrivileges.SetMemberPrivilegies))
                {
                    Asda2GuildHandler.SendChangeClanMemberRankAccessResultResponse(client,
                        ChangeMemberRankAccessResult.YouDontHavePermitionToUseThis);
                }
                else
                {
                    member.Asda2RankId = num;
                    Asda2GuildHandler.SendChangeClanMemberRankAccessResultResponse(client,
                        ChangeMemberRankAccessResult.Ok);
                    Asda2GuildHandler.SendGuildNotificationResponse(client.ActiveCharacter.Guild,
                        GuildNotificationType.RankChanged, member);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.LevelUpGuild)]
        public static void LevelUpGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to lvl up guild level", 50);
            else if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.UsePoints))
            {
                client.ActiveCharacter.SendSystemMessage("You have not permitions to use points.");
            }
            else
            {
                bool success = client.ActiveCharacter.Guild.LevelUp();
                if (success)
                    Asda2GuildHandler.SendClanFlagAndClanNameInfoSelfResponse(client.ActiveCharacter);
                Asda2GuildHandler.LevelUpGuildResponse(client, success);
            }
        }

        public static void LevelUpGuildResponse(IRealmClient client, bool success)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildLeveluped))
            {
                packet.Write(success);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.DisolveGuild)]
        public static void DisolveGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild || !client.ActiveCharacter.GuildMember.IsLeader)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to disolve guild with wrong parametrs", 50);
                Asda2GuildHandler.DisolveGuildResponse(client, false);
            }
            else if (client.ActiveCharacter.Guild.MemberCount > 1)
            {
                client.ActiveCharacter.SendSystemMessage("You must kick all members before disolving guild.");
                Asda2GuildHandler.DisolveGuildResponse(client, false);
            }
            else
            {
                client.ActiveCharacter.Guild.Disband();
                Asda2GuildHandler.DisolveGuildResponse(client, true);
            }
        }

        public static void DisolveGuildResponse(IRealmClient client, bool success)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildDisolved))
            {
                packet.Write(success);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.RegisterGuildCrest)]
        public static void RegisterGuildCrestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to register guild crest without guild", 50);
            else if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.EditCrest))
            {
                client.ActiveCharacter.SendSystemMessage("You have not permitions to edit clan crest.");
            }
            else
            {
                packet.Position += 6;
                short num1 = packet.ReadInt16();
                int num2 = (int) packet.ReadByte();
                Asda2Item asda2Item = (Asda2Item) null;
                try
                {
                    asda2Item = client.ActiveCharacter.Asda2Inventory.ShopItems[(int) num1];
                }
                catch (Exception ex)
                {
                }

                if (asda2Item == null || asda2Item.Category != Asda2ItemCategory.GuildCrest)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to register guild crest without wrong item",
                        10);
                }
                else
                {
                    asda2Item.ModAmount(-1);
                    byte[] numArray = packet.ReadBytes(40);
                    client.ActiveCharacter.Guild.ClanCrest = numArray;
                    client.ActiveCharacter.SendSystemMessage("You have success edit crest.");
                }
            }
        }

        [PacketHandler(RealmServerOpCode.KickFromGuild)]
        public static void KickFromGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to kick from guild without guild", 50);
            }
            else
            {
                packet.Position += 2;
                int targetAccId = packet.ReadInt32();
                byte num = packet.ReadByte();
                if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.EditRankSettings)
                )
                {
                    client.ActiveCharacter.SendSystemMessage("You have not permitions to kick him.");
                }
                else
                {
                    GuildMember member =
                        client.ActiveCharacter.Guild.Members[
                            Character.CharacterIdFromAccIdAndCharNum(targetAccId, (short) num)];
                    if (member == null)
                        client.ActiveCharacter.SendSystemMessage("Member not founded.");
                    else if (member.IsLeader)
                    {
                        client.ActiveCharacter.SendSystemMessage("You can't kick leader.");
                    }
                    else
                    {
                        if ((int) member.Asda2RankId > (int) client.ActiveCharacter.Asda2GuildRank)
                            client.ActiveCharacter.SendSystemMessage("You can't kick Higher rank than yours.");
                        if (member.Character != null)
                            Asda2GuildHandler.SendYouHaveBeenKickedFromGuildResponse(member.Character);
                        member.LeaveGuild(true);
                    }
                }
            }
        }

        public static void SendYouHaveBeenKickedFromGuildResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouHaveBeenKickedFromGuild))
            {
                packet.WriteByte(1);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt32(chr.AccId);
                packet.WriteFixedAsciiString(chr.Name, 20, Locale.Start);
                chr.Send(packet, false);
            }
        }

        public static void SendChangeClanMemberRankAccessResultResponse(IRealmClient client,
            ChangeMemberRankAccessResult status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChangeClanMemberRankAccessResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                client.Send(packet, false);
            }
        }

        public static void SendJoinMyGuildRequestResponse(Character rcv, Character sender, Guild guild)
        {
            rcv.GetTitle(Asda2TitleId.Loyal105);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.JoinMyGuildRequest))
            {
                packet.WriteInt32(rcv.AccId);
                packet.WriteInt16(rcv.SessionId);
                packet.WriteInt32(sender.AccId);
                packet.WriteInt16(sender.SessionId);
                packet.WriteInt16(guild.Id);
                packet.WriteFixedAsciiString(guild.Name, 17, Locale.Start);
                rcv.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.JoinGuildResponse)]
        public static void JoinGuildResponseRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            GuildInviteAnswerStatus inviteAnswerStatus = (GuildInviteAnswerStatus) packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GuildInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Character character1 = World.GetCharacter(relation.CharacterId);
            if (character1 == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            if (inviteAnswerStatus == GuildInviteAnswerStatus.Decline)
            {
                character1.SendSystemMessage(
                    string.Format("{0} declined your invation.", (object) activeCharacter.Name));
            }
            else
            {
                GuildMember guildMember = character1.GuildMember;
                if (guildMember == null)
                    return;
                Guild guild = guildMember.Guild;
                guild.AddMember(activeCharacter);
                character1.SendSystemMessage(
                    string.Format("{0} accepted your invation.", (object) activeCharacter.Name));
                Asda2GuildHandler.SendSendInviteGuiledResponseResponse(activeCharacter.Client, guild);
                Asda2GuildHandler.SendClanFlagAndClanNameInfoSelfResponse(activeCharacter);
                Asda2GuildHandler.SendGuildInfoOnLoginResponse(activeCharacter, guild);
                Asda2GuildHandler.SendGuildMembersInfoResponse(activeCharacter.Client, guild);
                Character character2 = activeCharacter;
                GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(character2);
                Asda2GuildHandler.SendGuildSkillsInfoResponse(character2);
            }
        }

        public static void SendYouHaveLeftGuildResponse(Character lefter, bool ok)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouHaveLeftGuild))
            {
                packet.WriteByte(ok ? 1 : 0);
                packet.WriteInt32(lefter.AccId);
                lefter.Send(packet, false);
            }
        }

        public static void SendUpdateGuildInfoResponse(Guild guild, GuildInfoMode mode = GuildInfoMode.Silent,
            Character rcv = null)
        {
            if (guild.Ranks == null)
                return;
            if (rcv != null)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildInfo))
                {
                    packet.WriteByte(2);
                    packet.WriteByte((byte) mode);
                    packet.WriteInt16(guild.Id);
                    packet.WriteFixedAsciiString(guild.Name, 17, Locale.Start);
                    packet.WriteInt16(guild.Level);
                    packet.WriteByte(guild.MaxMembersCount);
                    packet.WriteByte(guild.MemberCount);
                    packet.WriteInt32(guild.Points);
                    packet.WriteByte(4);
                    packet.WriteByte(0);
                    for (int index = 0; index < 10; ++index)
                    {
                        GuildSkill skill = guild.Skills[index];
                        packet.WriteInt16(skill == null ? -1 : (int) skill.Id);
                        packet.WriteByte(skill == null ? 0 : (int) skill.Level);
                        packet.WriteByte(skill == null ? 0 : (skill.IsActivated ? 1 : 0));
                    }

                    packet.WriteInt16((short) guild.Ranks[4].Privileges);
                    packet.WriteInt16((short) guild.Ranks[3].Privileges);
                    packet.WriteInt16((short) guild.Ranks[2].Privileges);
                    packet.WriteInt16((short) guild.Ranks[1].Privileges);
                    packet.WriteInt32(guild.Leader.AccId);
                    packet.WriteByte(guild.Leader.CharNum);
                    packet.WriteFixedAsciiString(guild.Leader.Name, 20, Locale.Start);
                    packet.WriteFixedAsciiString(guild.MOTD, 293, Locale.Start);
                    rcv.Send(packet, true);
                }
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateGuildInfo))
                {
                    packet.WriteByte(2);
                    packet.WriteByte((byte) mode);
                    packet.WriteInt16(guild.Id);
                    packet.WriteFixedAsciiString(guild.Name, 17, Locale.Start);
                    packet.WriteInt16(guild.Level);
                    packet.WriteByte(guild.MaxMembersCount);
                    packet.WriteByte(guild.MemberCount);
                    packet.WriteInt32(guild.Points);
                    packet.WriteByte(4);
                    packet.WriteByte(0);
                    for (int index = 0; index < 10; ++index)
                    {
                        GuildSkill skill = guild.Skills[index];
                        packet.WriteInt16(skill == null ? -1 : (int) skill.Id);
                        packet.WriteByte(skill == null ? 0 : (int) skill.Level);
                        packet.WriteByte(skill == null ? 0 : (skill.IsActivated ? 1 : 0));
                    }

                    packet.WriteInt16((short) guild.Ranks[4].Privileges);
                    packet.WriteInt16((short) guild.Ranks[3].Privileges);
                    packet.WriteInt16((short) guild.Ranks[2].Privileges);
                    packet.WriteInt16((short) guild.Ranks[1].Privileges);
                    packet.WriteInt32(guild.Leader.AccId);
                    packet.WriteByte(guild.Leader.CharNum);
                    packet.WriteFixedAsciiString(guild.Leader.Name, 20, Locale.Start);
                    packet.WriteFixedAsciiString(guild.MOTD, 293, Locale.Start);
                    guild.Send(packet, true, Locale.Any);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.GuildChatReq)]
        public static void GuildChatReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadAsciiString(client.Locale);
            if (!client.ActiveCharacter.IsInGuild || str.Length > 200)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Guild chat request without guild.", 50);
            }
            else
            {
                if (str.Length < 1 || RealmCommandHandler.HandleCommand((IUser) client.ActiveCharacter, str,
                        (IGenericChatTarget) (client.ActiveCharacter.Target as Character)))
                    return;
                Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
                Asda2GuildHandler.SendGuildChatResResponse(str, client.ActiveCharacter.Guild, client.ActiveCharacter,
                    locale);
            }
        }

        public static void SendGuildChatResResponse(string msg, Guild guild, Character sender, Locale locale)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildChatRes))
            {
                packet.WriteInt16(guild.Id);
                packet.WriteInt32(sender.AccId);
                packet.WriteInt16(sender.SessionId);
                packet.WriteFixedAsciiString(sender.Name, 20, Locale.Start);
                packet.WriteAsdaString(msg, msg.Length, Locale.Start);
                guild.Send(packet, false, locale);
            }
        }

        [PacketHandler(RealmServerOpCode.WritePublicGuildMemberNote)]
        public static void WritePublicGuildMemberNoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Trying to write public guild member note request with out guild.", 50);
            }
            else
            {
                packet.Position += 2;
                string s = packet.ReadAsdaString(64, Locale.Start);
                if (!Asda2EncodingHelper.IsPrueEnglish(s))
                {
                    client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("member note");
                    Asda2GuildHandler.SendEditPublicGuildNoteResponse(client, false);
                }
                else
                {
                    Asda2GuildHandler.SendEditPublicGuildNoteResponse(client, true);
                    client.ActiveCharacter.GuildMember.PublicNote = s;
                }
            }
        }

        public static void SendEditPublicGuildNoteResponse(IRealmClient client, bool b)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EditPublicGuildNote))
            {
                packet.WriteByte(b ? 1 : 0);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.EditClanNotice)]
        public static void EditClanNoticeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
                client.ActiveCharacter.YouAreFuckingCheater("Edit clan notice request without guild", 50);
            else if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.EditAnnounce))
            {
                client.ActiveCharacter.SendSystemMessage("You don't have permitins to edit Announce.");
                Asda2GuildHandler.SendAnnouncementEditedResponse(client, false);
            }
            else
            {
                packet.Position += 2;
                string s = packet.ReadAsdaString(260, Locale.Start);
                if (!Asda2EncodingHelper.IsPrueEnglish(s))
                {
                    client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("notice");
                    Asda2GuildHandler.SendEditPublicGuildNoteResponse(client, false);
                }
                else
                {
                    client.ActiveCharacter.Guild.MOTD = s;
                    Asda2GuildHandler.SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild,
                        GuildInfoMode.Announcement, (Character) null);
                    Asda2GuildHandler.SendAnnouncementEditedResponse(client, true);
                }
            }
        }

        public static void SendAnnouncementEditedResponse(IRealmClient client, bool success)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AnnouncementEdited))
            {
                packet.WriteByte(success ? 1 : 0);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.AskForGuildHistory)]
        public static void AskForGuildHistoryRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to act guild history while not in guild.", 50);
            else
                Asda2GuildHandler.SendGuildHistoryResponse(client, client.ActiveCharacter.Guild);
        }

        public static void SendGuildHistoryResponse(IRealmClient client, Guild guild)
        {
            using (RealmPacketOut packet1 = new RealmPacketOut(RealmServerOpCode.GuildHistory))
            {
                for (int index = 0; index < 12 && guild.History.Count > index; ++index)
                {
                    HistoryRecord historyRecord = guild.History[index];
                    packet1.WriteInt16(guild.Id);
                    packet1.WriteByte(historyRecord.Type);
                    packet1.WriteInt32(historyRecord.Value);
                    packet1.WriteFixedAsciiString(historyRecord.TrigerName, 20, Locale.Start);
                    packet1.WriteFixedAsciiString(historyRecord.Time, 17, Locale.Start);
                }

                client.Send(packet1, false);
                using (RealmPacketOut packet2 = new RealmPacketOut(RealmServerOpCode.GuildHistoryEnded))
                    client.Send(packet2, false);
            }
        }

        [PacketHandler(RealmServerOpCode.SetPrivilegies)]
        public static void SetPrivilegiesRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to set privilegies while not in guild.", 50);
            }
            else
            {
                if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.EditRankSettings)
                )
                    Asda2GuildHandler.SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.HaveNotPermitions,
                        (short) client.ActiveCharacter.Guild.Id);
                packet.Position += 2;
                client.ActiveCharacter.Guild.Ranks[4].Privileges = (GuildPrivileges) packet.ReadInt16();
                client.ActiveCharacter.Guild.Ranks[3].Privileges = (GuildPrivileges) packet.ReadInt16();
                client.ActiveCharacter.Guild.Ranks[2].Privileges = (GuildPrivileges) packet.ReadInt16();
                client.ActiveCharacter.Guild.Ranks[1].Privileges = (GuildPrivileges) packet.ReadInt16();
                Asda2GuildHandler.SendUpdateGuildInfoResponse(client.ActiveCharacter.Guild,
                    GuildInfoMode.GuildPrivilegiesChanged, (Character) null);
                Asda2GuildHandler.SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.Ok,
                    (short) client.ActiveCharacter.GuildId);
            }
        }

        public static void SendPrivilagiesChangedResponse(IRealmClient client, ChangePrivilegiesStatus status,
            short clanId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PrivilagiesChanged))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(clanId);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.LeaveGuild)]
        public static void LeaveGuildRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Leave guild while not in guild", 10);
                Asda2GuildHandler.SendYouHaveLeftGuildResponse(client.ActiveCharacter, false);
            }
            else
            {
                Asda2GuildHandler.SendYouHaveLeftGuildResponse(client.ActiveCharacter, true);
                client.ActiveCharacter.GuildMember.LeaveGuild(false);
            }
        }

        public static void SendClanFlagAndClanNameInfoSelfResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ClanFlagAndClanNameInfoSelf))
            {
                packet.WriteInt32(chr.AccId);
                packet.WriteInt32(chr.GuildPoints);
                packet.WriteSkip(Asda2GuildHandler.stab15);
                packet.WriteInt16(chr.GuildId);
                packet.WriteFixedAsciiString(chr.Guild.Name, 17, Locale.Start);
                packet.WriteInt32(chr.Asda2GuildRank);
                packet.WriteByte(3);
                packet.WriteByte(chr.Guild.ClanCrest[0] != (byte) 0 ? 1 : 0);
                packet.WriteSkip(chr.Guild.ClanCrest[0] != (byte) 0 ? chr.Guild.ClanCrest : Asda2GuildHandler.Unk13);
                packet.WriteFixedAsciiString(chr.GuildMember.PublicNote, 60, Locale.Start);
                packet.WriteInt32(0);
                chr.Send(packet, false);
            }
        }

        public static void SendGuildNotificationResponse(Guild guild, GuildNotificationType status,
            GuildMember trigerer)
        {
            if (trigerer == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildNotification))
            {
                packet.WriteByte(2);
                packet.WriteByte((byte) status);
                packet.WriteInt16(guild.Id);
                packet.WriteInt32(trigerer.AccId);
                packet.WriteByte(trigerer.CharNum);
                packet.WriteFixedAsciiString(trigerer.Name, 20, Locale.Start);
                packet.WriteByte(trigerer.Level);
                packet.WriteByte(trigerer.Character == null ? 0 : (int) trigerer.Character.ProfessionLevel);
                packet.WriteByte(trigerer.Character == null ? (byte) 0 : (byte) trigerer.Character.Archetype.ClassId);
                packet.WriteByte(trigerer.Asda2RankId);
                packet.WriteInt32(trigerer.Character == null ? 0 : trigerer.Character.GuildPoints);
                packet.WriteByte(trigerer.Character == null ? 0 : (trigerer.Character.IsLoggingOut ? 0 : 1));
                packet.WriteSkip(Asda2GuildHandler.stab45);
                packet.WriteByte(1);
                packet.WriteByte(trigerer.Character == null ? (byte) 0 : (byte) trigerer.Character.MapId);
                packet.WriteFixedAsciiString(trigerer.PublicNote, 60, Locale.Start);
                packet.WriteSkip(Asda2GuildHandler.stab124);
                guild.Send(packet, true, Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.LearnClanSkill)]
        public static void LearnClanSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            short num = packet.ReadInt16();
            if (!client.ActiveCharacter.IsInGuild)
                client.ActiveCharacter.YouAreFuckingCheater("Learning clan skill while not in guild.", 50);
            else if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.UsePoints))
                Asda2GuildHandler.SendClanSkillLearnedResponse(client.ActiveCharacter, (GuildSkill) null,
                    LearnGuildSkillResult.YouDontHavePermitionToDoThis, 0);
            else if (num < (short) 0 || num > (short) 4)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Learning wrong clan skill Id.", 10);
            }
            else
            {
                GuildSkill skill;
                LearnGuildSkillResult status =
                    client.ActiveCharacter.Guild.TryLearnSkill((GuildSkillId) num, out skill);
                Asda2GuildHandler.SendClanSkillLearnedResponse(client.ActiveCharacter, skill, status, (int) num);
            }
        }

        public static void SendClanSkillLearnedResponse(Character chr, GuildSkill skill, LearnGuildSkillResult status,
            int skillId = 0)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ClanSkillLearned))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(chr.Guild.Id);
                packet.WriteInt16(skill == null ? (short) skillId : (short) skill.Id);
                packet.WriteByte(skill == null ? 1 : (int) skill.Level);
                chr.Send(packet, false);
            }
        }

        public static void SendGuildSkillStatusChangedResponse(GuildSkill skill, ClanSkillStatus status)
        {
            if (skill == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildSkillStatusChanged))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(skill.Guild.Id);
                packet.WriteInt16((short) skill.Id);
                packet.WriteByte(skill.Level);
                packet.WriteByte(skill.IsActivated);
                skill.Guild.Send(packet, true, Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.ActivateGuildSkill)]
        public static void ActivateGuildSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            if (!client.ActiveCharacter.IsInGuild)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Activating guild skill while not in guild", 50);
            }
            else
            {
                if (!client.ActiveCharacter.GuildMember.Rank.Privileges.HasFlag((Enum) GuildPrivileges.UsePoints))
                    Asda2GuildHandler.SendPrivilagiesChangedResponse(client, ChangePrivilegiesStatus.HaveNotPermitions,
                        (short) client.ActiveCharacter.Guild.Id);
                short num = packet.ReadInt16();
                if (num < (short) 0 || num > (short) 4 || client.ActiveCharacter.Guild.Skills[(int) num] == null)
                    client.ActiveCharacter.YouAreFuckingCheater("", 1);
                else
                    client.ActiveCharacter.Guild.Skills[(int) num].ToggleActivate(client.ActiveCharacter);
            }
        }

        public static void SendGuildSkillActivatedResponse(Character activator, GuildSkillActivationStatus status,
            GuildSkill skill)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildSkillActivated))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(activator.AccId);
                packet.WriteInt16(activator.Guild.Id);
                packet.WriteInt16((short) skill.Id);
                packet.WriteByte(skill.Level);
                activator.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.DonateGuildPoints)]
        public static void DonateGuildPointsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            int points = packet.ReadInt32();
            if (!client.ActiveCharacter.IsInGuild || client.ActiveCharacter.GuildPoints < points || points < 0)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Donating guild points while not in guild", 10);
                Asda2GuildHandler.SendGuildPointsDonatedResponse(client, false);
            }
            else
            {
                client.ActiveCharacter.GuildPoints -= points;
                client.ActiveCharacter.Guild.AddGuildPoints(points);
                client.ActiveCharacter.Guild.AddHistoryMessage(Asda2GuildHistoryType.DonatedPoints, points,
                    client.ActiveCharacter.Name, DateTime.Now.ToLongTimeString());
                Asda2GuildHandler.SendGuildPointsDonatedResponse(client, true);
                Asda2GuildHandler.SendGuildNotificationResponse(client.ActiveCharacter.Guild,
                    GuildNotificationType.DonatedPoints, client.ActiveCharacter.GuildMember);
            }
        }

        public static void SendGuildPointsDonatedResponse(IRealmClient client, bool success)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GuildPointsDonated))
            {
                packet.WriteByte(success ? 1 : 0);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(client.ActiveCharacter.GuildId);
                packet.WriteInt32(client.ActiveCharacter.GuildPoints);
                client.Send(packet, false);
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
            Failed,
            Success,
        }
    }
}