using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class GuildHandler
    {
        /// <summary>Handles an incoming guild query</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Query(IRealmClient client, RealmPacketIn packet)
        {
            uint guildId = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            Guild guild =
                activeCharacter == null || activeCharacter.Guild == null ||
                (int) activeCharacter.Guild.Id != (int) guildId
                    ? GuildMgr.GetGuild(guildId)
                    : activeCharacter.Guild;
            if (guild == null)
                return;
            GuildHandler.SendGuildQueryResponse((IPacketReceiver) client, guild);
        }

        /// <summary>Handles an incoming guild roster query</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Roster(IRealmClient client, RealmPacketIn packet)
        {
            GuildMember guildMember = client.ActiveCharacter.GuildMember;
            if (guildMember == null)
                return;
            Guild guild = guildMember.Guild;
        }

        /// <summary>Handles an incoming guild roster query</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void EventLog(IRealmClient client, RealmPacketIn packet)
        {
            GuildMember guildMember = client.ActiveCharacter.GuildMember;
            if (guildMember == null)
                return;
            Guild guild = guildMember.Guild;
            if (guild == null)
                return;
            GuildHandler.SendGuildEventLog((IPacketReceiver) client, guild);
        }

        /// <summary>
        /// Handles an incoming guild invite request (/ginvite Player)
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void InviteRequest(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            Character character = World.GetCharacter(str, false);
            Guild guild = activeCharacter.Guild;
            if (Guild.CheckInvite(activeCharacter, character, str) != GuildResult.SUCCESS)
                return;
            Singleton<RelationMgr>.Instance.AddRelation(RelationMgr.CreateRelation(activeCharacter.EntityId.Low,
                character.EntityId.Low, CharacterRelationType.GuildInvite));
            GuildHandler.SendResult((IPacketReceiver) activeCharacter.Client, GuildCommandId.INVITE, character.Name,
                GuildResult.SUCCESS);
            guild.EventLog.AddInviteEvent(activeCharacter.EntityId.Low, character.EntityId.Low);
            GuildHandler.SendGuildInvite((IPacketReceiver) character.Client, activeCharacter);
        }

        /// <summary>Handles an incoming accept on guild invite request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Accept(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GuildInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Character character = World.GetCharacter(relation.CharacterId);
            if (character == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            GuildMember guildMember = character.GuildMember;
            if (guildMember == null)
                return;
            guildMember.Guild.AddMember(activeCharacter);
        }

        /// <summary>Handles an incoming decline on guild invite request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Decline(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GuildInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Character character = World.GetCharacter(relation.CharacterId);
            if (character == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            GuildHandler.SendGuildDecline((IPacketReceiver) character.Client, activeCharacter.Name);
        }

        /// <summary>Handles an incoming guild leave request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Leave(IRealmClient client, RealmPacketIn packet)
        {
            GuildMember guildMember = client.ActiveCharacter.GuildMember;
            if (guildMember == null)
                GuildHandler.SendResult((IPacketReceiver) client, GuildCommandId.QUIT, GuildResult.PLAYER_NOT_IN_GUILD);
            else if (guildMember.IsLeader && guildMember.Guild.MemberCount > 1)
            {
                GuildHandler.SendResult((IPacketReceiver) client, GuildCommandId.QUIT, GuildResult.PERMISSIONS);
            }
            else
            {
                guildMember.LeaveGuild(false);
                GuildHandler.SendResult((IPacketReceiver) client, GuildCommandId.QUIT, GuildResult.SUCCESS);
            }
        }

        /// <summary>Handles an incoming guild member remove request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Remove(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>
        /// Handles an incoming guild information (one in guild roster) change request
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeInfo(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>
        /// Handles an incoming guild information (/ginfo) request
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void InfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            GuildMember guildMember = client.ActiveCharacter.GuildMember;
            if (guildMember == null)
                GuildHandler.SendResult((IPacketReceiver) client, GuildCommandId.CREATE,
                    GuildResult.PLAYER_NOT_IN_GUILD);
            else
                GuildHandler.SendGuildInformation((IPacketReceiver) client, guildMember.Guild);
        }

        /// <summary>Handles an incoming guild MOTD change request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeMOTD(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming public guild note change request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangePublicNote(IRealmClient client, RealmPacketIn packet)
        {
            string name = packet.ReadCString();
            packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            World.GetCharacter(name, false);
        }

        /// <summary>Handles an incoming officer guild note change request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeOfficerNote(IRealmClient client, RealmPacketIn packet)
        {
            string name = packet.ReadCString();
            packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            World.GetCharacter(name, false);
        }

        public static void HandleGuildBankMoneyWithdrawnRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Guild == null || activeCharacter.GuildMember == null)
                return;
            uint withdrawlAllowance = activeCharacter.GuildMember.BankMoneyWithdrawlAllowance;
            GuildHandler.SendMemberRemainingDailyWithdrawlAllowance((IPacketReceiver) activeCharacter,
                withdrawlAllowance);
        }

        public static void HandleSetGuildTabard(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint num1 = packet.ReadUInt32();
            uint num2 = packet.ReadUInt32();
            uint num3 = packet.ReadUInt32();
            uint num4 = packet.ReadUInt32();
            uint num5 = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            NPC vendor = activeCharacter.Map.GetObject(id) as NPC;
            GuildTabard tabard = new GuildTabard()
            {
                BackgroundColor = (int) num5,
                BorderColor = (int) num4,
                BorderStyle = (int) num3,
                EmblemColor = (int) num2,
                EmblemStyle = (int) num1
            };
            if (activeCharacter.Guild != null && activeCharacter.GuildMember != null)
                activeCharacter.Guild.TrySetTabard(activeCharacter.GuildMember, vendor, tabard);
            else
                GuildHandler.SendTabardResult((IPacketReceiver) activeCharacter, GuildTabardResult.NoGuild);
        }

        public static void HandleGuildPermissionsQuery(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Guild == null || activeCharacter.GuildMember == null)
                return;
            GuildHandler.SendGuildBankPermissions(activeCharacter);
        }

        /// <summary>Called when the GuildBank vault GameObject is clicked</summary>
        public static void HandleGuildBankActivate(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            int num = (int) packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            GameObject bank = activeCharacter.Map.GetObject(id) as GameObject;
            if (activeCharacter.Guild != null && activeCharacter.GuildMember != null)
                GuildHandler.SendGuildBankTabNames(activeCharacter, bank);
            else
                GuildHandler.SendResult((IPacketReceiver) activeCharacter, GuildCommandId.BANK,
                    GuildResult.PLAYER_NOT_IN_GUILD);
        }

        public static void HandleGuildBankTabQuery(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            byte tabId = packet.ReadByte();
            int num = (int) packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            GameObject bank = activeCharacter.Map.GetObject(id) as GameObject;
            if (activeCharacter.Guild == null || activeCharacter.GuildMember == null)
                return;
            GuildHandler.GetBankTabContent(activeCharacter, bank, tabId);
        }

        public static void HandleGuildBankDepositMoney(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint deposit = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            GameObject bankObj = activeCharacter.Map.GetObject(id) as GameObject;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.DepositMoney(activeCharacter, bankObj, deposit);
        }

        public static void HandleGuildBankWithdrawMoney(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            uint withdrawl = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            GameObject bankObj = activeCharacter.Map.GetObject(id) as GameObject;
            if (activeCharacter.Guild == null || activeCharacter.GuildMember == null)
                return;
            activeCharacter.Guild.Bank.WithdrawMoney(activeCharacter, bankObj, withdrawl);
        }

        public static void HandleGuildBankSwapItems(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            bool flag1 = packet.ReadBoolean();
            byte toBankTabId = 1;
            byte toTabSlot = 1;
            bool flag2 = false;
            byte autoStoreCount = 0;
            byte bagSlot = 0;
            byte slot = 0;
            bool flag3 = true;
            byte num1;
            byte num2;
            uint itemEntryId;
            byte amount;
            if (flag1)
            {
                toBankTabId = packet.ReadByte();
                toTabSlot = packet.ReadByte();
                int num3 = (int) packet.ReadUInt32();
                num1 = packet.ReadByte();
                num2 = packet.ReadByte();
                itemEntryId = packet.ReadUInt32();
                int num4 = (int) packet.ReadByte();
                amount = packet.ReadByte();
                if (toTabSlot >= (byte) 98 || (int) toBankTabId == (int) num1 && (int) toTabSlot == (int) num2)
                    return;
            }
            else
            {
                num1 = packet.ReadByte();
                num2 = packet.ReadByte();
                itemEntryId = packet.ReadUInt32();
                flag2 = packet.ReadBoolean();
                autoStoreCount = (byte) 0;
                if (flag2)
                {
                    autoStoreCount = packet.ReadByte();
                    packet.SkipBytes(5);
                }
                else
                {
                    bagSlot = packet.ReadByte();
                    slot = packet.ReadByte();
                }

                flag3 = packet.ReadBoolean();
                amount = packet.ReadByte();
                if (num2 >= (byte) 98 && num2 != byte.MaxValue)
                    return;
            }

            Character activeCharacter = client.ActiveCharacter;
            GameObject gameObject = activeCharacter.Map.GetObject(id) as GameObject;
            Guild guild = activeCharacter.Guild;
            if (flag1)
                guild.Bank.SwapItemsManualBankToBank(activeCharacter, gameObject, num1, num2, toBankTabId, toTabSlot,
                    itemEntryId, amount);
            else if (flag3)
            {
                if (flag2)
                    guild.Bank.SwapItemsAutoStoreBankToChar(activeCharacter, gameObject, num1, num2, itemEntryId,
                        autoStoreCount);
                else
                    guild.Bank.SwapItemsManualBankToChar(activeCharacter, gameObject, num1, num2, bagSlot, slot,
                        itemEntryId, amount);
            }
            else if (flag2)
                guild.Bank.SwapItemsAutoStoreCharToBank(activeCharacter, gameObject, num1, bagSlot, slot, itemEntryId,
                    autoStoreCount);
            else
                guild.Bank.SwapItemsManualCharToBank(activeCharacter, gameObject, bagSlot, slot, itemEntryId, num1,
                    num2, amount);
        }

        public static void HandleGuildBankBuyTab(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            byte tabId = packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            GameObject bank = activeCharacter.Map.GetObject(id) as GameObject;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.BuyTab(activeCharacter, bank, tabId);
        }

        public static void HandleGuildBankModifyTabInfo(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            byte tabId = packet.ReadByte();
            string newName = packet.ReadCString();
            string newIcon = packet.ReadCString();
            if (newName.Length == 0 || newIcon.Length == 0)
                return;
            Character activeCharacter = client.ActiveCharacter;
            GameObject bank = activeCharacter.Map.GetObject(id) as GameObject;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.ModifyTabInfo(activeCharacter, bank, tabId, newName, newIcon);
        }

        public static void HandleQueryGuildBankTabText(IRealmClient client, RealmPacketIn packet)
        {
            byte tabId = packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.GetBankTabText(activeCharacter, tabId);
        }

        public static void HandleSetGuildBankTabText(IRealmClient client, RealmPacketIn packet)
        {
            byte tabId = packet.ReadByte();
            string newText = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.SetBankTabText(activeCharacter, tabId, newText);
        }

        public static void HandleQueryGuildBankLog(IRealmClient client, RealmPacketIn packet)
        {
            byte tabId = packet.ReadByte();
            Character activeCharacter = client.ActiveCharacter;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.Bank.QueryBankLog(activeCharacter, tabId);
        }

        /// <summary>Handles an incoming add rank request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void AddRank(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadCString().Trim();
            if (str.Length < 2)
                return;
            int length = str.Length;
            int guildRankNameLength = GuildMgr.MaxGuildRankNameLength;
        }

        /// <summary>Handles an incoming rank remove request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void DeleteRank(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming rank change request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeRank(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming guild member promote request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandlePromote(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming guild member demote request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Demote(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming guild disband request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void Disband(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming guild leader change request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeLeader(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            Character character = World.GetCharacter(str, false);
            if (Guild.CheckIsLeader(activeCharacter, character, GuildCommandId.CREATE, str) != GuildResult.SUCCESS)
                return;
            Guild guild = activeCharacter.Guild;
            if (guild == null)
                return;
            guild.ChangeLeader(character.GuildMember);
        }

        /// <summary>Sends a guild query response to the client.</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="guild">guild to be sent</param>
        public static void SendGuildQueryResponse(IPacketReceiver client, Guild guild)
        {
            using (RealmPacketOut queryResponsePacket = GuildHandler.CreateGuildQueryResponsePacket(guild))
                client.Send(queryResponsePacket, false);
        }

        /// <summary>
        /// Sends a guild query response to all member of this guild
        /// </summary>
        /// <param name="guild">guild to be sent</param>
        public static void SendGuildQueryToGuildMembers(Guild guild)
        {
            using (RealmPacketOut queryResponsePacket = GuildHandler.CreateGuildQueryResponsePacket(guild))
                guild.Broadcast(queryResponsePacket);
        }

        /// <summary>Sends a guild invite packet to a client</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="inviter">inviter</param>
        public static void SendGuildInvite(IPacketReceiver client, Character inviter)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_INVITE))
            {
                packet.WriteCString(inviter.Name);
                packet.WriteCString(inviter.GuildMember.Guild.Name);
                client.Send(packet, false);
            }
        }

        private static RealmPacketOut CreateGuildQueryResponsePacket(Guild guild)
        {
            GuildRank[] ranks = guild.Ranks;
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_QUERY_RESPONSE);
            realmPacketOut.WriteUInt((byte) guild.Id);
            realmPacketOut.WriteCString(guild.Name);
            for (int index = 0; index < 10; ++index)
                realmPacketOut.WriteCString(ranks[index].Name);
            if (ranks.Length < 10)
                realmPacketOut.Fill((byte) 0, 10 - ranks.Length);
            realmPacketOut.Write(guild.Tabard.EmblemStyle);
            realmPacketOut.Write(guild.Tabard.EmblemColor);
            realmPacketOut.Write(guild.Tabard.BorderStyle);
            realmPacketOut.Write(guild.Tabard.BorderColor);
            realmPacketOut.Write(guild.Tabard.BackgroundColor);
            realmPacketOut.Write(0);
            return realmPacketOut;
        }

        /// <summary>Sends guild invitation decline</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="decliner">player who has declined your request</param>
        public static void SendGuildDecline(IPacketReceiver client, string decliner)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GUILD_DECLINE, decliner.Length + 1))
            {
                packet.WriteCString(decliner);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends guild roster</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="guild">guild</param>
        public static void SendGuildRoster(IPacketReceiver client, Guild guild, bool showOfficerNotes)
        {
            using (RealmPacketOut guildRosterPacket = GuildHandler.CreateGuildRosterPacket(guild, showOfficerNotes))
                client.Send(guildRosterPacket, false);
        }

        /// <summary>Sends guild roster to guild members</summary>
        /// <param name="guild">guild</param>
        public static void SendGuildRosterToGuildMembers(Guild guild)
        {
        }

        private static RealmPacketOut CreateGuildRosterPacket(Guild guild, bool showOfficerNotes)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_ROSTER);
            realmPacketOut.Write(guild.MemberCount);
            if (guild.MOTD != null)
                realmPacketOut.WriteCString(guild.MOTD);
            else
                realmPacketOut.WriteByte(0);
            if (guild.Info != null)
                realmPacketOut.WriteCString(guild.Info);
            else
                realmPacketOut.WriteByte(0);
            GuildRank[] ranks = guild.Ranks;
            realmPacketOut.Write(ranks.Length);
            for (int index1 = 0; index1 < ranks.Length; ++index1)
            {
                GuildRank guildRank = ranks[index1];
                realmPacketOut.Write((uint) guildRank.Privileges);
                realmPacketOut.Write(guildRank.DailyBankMoneyAllowance);
                for (int index2 = 0; index2 < 6; ++index2)
                {
                    GuildBankTabRights bankTabRight = guildRank.BankTabRights[index2];
                    realmPacketOut.WriteInt((uint) bankTabRight.Privileges);
                    realmPacketOut.WriteInt(bankTabRight.WithdrawlAllowance);
                }
            }

            foreach (GuildMember guildMember in (IEnumerable<GuildMember>) guild.Members.Values)
            {
                Character character = guildMember.Character;
                realmPacketOut.Write((ulong) EntityId.GetPlayerId(guildMember.Id));
                if (character != null)
                    realmPacketOut.WriteByte((byte) character.Status);
                else
                    realmPacketOut.WriteByte((byte) 0);
                realmPacketOut.WriteCString(guildMember.Name);
                realmPacketOut.Write(guildMember.Rank.RankIndex);
                realmPacketOut.Write((byte) guildMember.Level);
                realmPacketOut.Write((byte) guildMember.Class);
                realmPacketOut.Write((byte) 0);
                realmPacketOut.Write(guildMember.ZoneId);
                if (character == null)
                    realmPacketOut.Write((float) (DateTime.Now - guildMember.LastLogin).TotalDays);
                if (guildMember.PublicNote != null)
                    realmPacketOut.WriteCString(guildMember.PublicNote);
                else
                    realmPacketOut.Write((byte) 0);
                if (showOfficerNotes && guildMember.OfficerNote != null)
                    realmPacketOut.WriteCString(guildMember.OfficerNote);
                else
                    realmPacketOut.Write((byte) 0);
            }

            return realmPacketOut;
        }

        /// <summary>Sends a guild information to a client</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="guild">guild</param>
        public static void SendGuildInformation(IPacketReceiver client, Guild guild)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_INFO))
            {
                packet.WriteCString(guild.Name);
                packet.WriteInt((byte) guild.Created.Year);
                packet.WriteInt((byte) guild.Created.Month);
                packet.WriteInt((byte) guild.Created.Day);
                packet.WriteInt((byte) guild.MemberCount);
                packet.WriteInt((byte) guild.MemberCount);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends result of actions connected with guilds</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="name">name of player event has happened to</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.Guilds.GuildResult" /> result code</param>
        public static void SendResult(IPacketReceiver client, GuildCommandId commandId, string name,
            GuildResult resultCode)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_COMMAND_RESULT))
            {
                packet.WriteUInt((uint) commandId);
                packet.WriteCString(name);
                packet.WriteUInt((uint) resultCode);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends result of actions connected with guilds</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="commandId">command executed</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.Guilds.GuildResult" /> result code</param>
        public static void SendResult(IPacketReceiver client, GuildCommandId commandId, GuildResult resultCode)
        {
            GuildHandler.SendResult(client, commandId, string.Empty, resultCode);
        }

        /// <summary>Sends events connected with guilds</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        /// <param name="affectedMember">The <see cref="T:WCell.RealmServer.Guilds.GuildMember" /> which was affected</param>
        public static void SendEvent(IPacketReceiver client, Guild guild, GuildEvents guildEvent,
            GuildMember affectedMember)
        {
            using (RealmPacketOut eventPacket = GuildHandler.CreateEventPacket(guild, guildEvent, affectedMember))
                client.Send(eventPacket, false);
        }

        /// <summary>
        /// Sends event connected with guilds to all guild members
        /// </summary>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        public static void SendEventToGuild(Guild guild, GuildEvents guildEvent)
        {
            using (RealmPacketOut eventPacket = GuildHandler.CreateEventPacket(guild, guildEvent, (GuildMember) null))
                guild.Broadcast(eventPacket);
        }

        /// <summary>
        /// Sends event connected with guilds to all guild members. Use this one for promotion/demotion events.
        /// </summary>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        /// <param name="affectedMember">The <see cref="T:WCell.RealmServer.Guilds.GuildMember" /> which was affected</param>
        /// <param name="influencer">one who caused this event</param>
        public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember,
            GuildMember influencer)
        {
            using (RealmPacketOut eventPacket =
                GuildHandler.CreateEventPacket(guild, guildEvent, affectedMember, influencer))
                guild.Broadcast(eventPacket);
        }

        /// <summary>
        /// Sends event connected with guilds to all guild members
        /// </summary>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        /// <param name="affectedMember">The <see cref="T:WCell.RealmServer.Guilds.GuildMember" /> which was affected</param>
        public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember)
        {
            using (RealmPacketOut eventPacket = GuildHandler.CreateEventPacket(guild, guildEvent, affectedMember))
                guild.Broadcast(eventPacket);
        }

        /// <summary>
        /// Sends event connected with guilds to all guild members, except one
        /// </summary>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        /// <param name="ignoredCharacter">character to be ignored</param>
        public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, Character ignoredCharacter)
        {
            using (RealmPacketOut eventPacket = GuildHandler.CreateEventPacket(guild, guildEvent, (GuildMember) null))
                guild.Broadcast(eventPacket, ignoredCharacter);
        }

        /// <summary>
        /// Sends event connected with guilds to all guild members, except one
        /// </summary>
        /// <param name="guild">guild</param>
        /// <param name="guildEvent">event that happened</param>
        /// <param name="affectedMember">The <see cref="T:WCell.RealmServer.Guilds.GuildMember" /> which was affected</param>
        /// <param name="ignoredCharacter">character to be ignored</param>
        public static void SendEventToGuild(Guild guild, GuildEvents guildEvent, GuildMember affectedMember,
            Character ignoredCharacter)
        {
            using (RealmPacketOut eventPacket = GuildHandler.CreateEventPacket(guild, guildEvent, affectedMember))
                guild.Broadcast(eventPacket, ignoredCharacter);
        }

        private static RealmPacketOut CreateEventPacket(Guild guild, GuildEvents guildEvent, GuildMember affectedMember)
        {
            return GuildHandler.CreateEventPacket(guild, guildEvent, affectedMember, (GuildMember) null);
        }

        /// <summary>TODO: Fix for 3.3</summary>
        /// <param name="guild"></param>
        /// <param name="guildEvent"></param>
        /// <param name="affectedMember"></param>
        /// <param name="influencer"></param>
        /// <returns></returns>
        private static RealmPacketOut CreateEventPacket(Guild guild, GuildEvents guildEvent, GuildMember affectedMember,
            GuildMember influencer)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_GUILD_EVENT);
            realmPacketOut.WriteByte((byte) guildEvent);
            switch (guildEvent)
            {
                case GuildEvents.PROMOTION:
                case GuildEvents.DEMOTION:
                    realmPacketOut.Write((byte) 3);
                    realmPacketOut.WriteCString(influencer.Name);
                    realmPacketOut.WriteCString(affectedMember.Name);
                    realmPacketOut.WriteCString(affectedMember.Rank.Name);
                    break;
                case GuildEvents.MOTD:
                    if (guild.MOTD != null)
                    {
                        realmPacketOut.Write((byte) 1);
                        realmPacketOut.WriteCString(guild.MOTD);
                        break;
                    }

                    realmPacketOut.Write((byte) 0);
                    break;
                case GuildEvents.JOINED:
                case GuildEvents.LEFT:
                case GuildEvents.ONLINE:
                case GuildEvents.OFFLINE:
                    realmPacketOut.Write((byte) 1);
                    realmPacketOut.WriteCString(affectedMember.Name);
                    break;
                case GuildEvents.REMOVED:
                    realmPacketOut.Write((byte) 2);
                    realmPacketOut.WriteCString(influencer.Name);
                    realmPacketOut.WriteCString(affectedMember.Name);
                    break;
                case GuildEvents.LEADER_CHANGED:
                    realmPacketOut.Write((byte) 2);
                    realmPacketOut.WriteCString(affectedMember.Name);
                    realmPacketOut.WriteCString(influencer.Name);
                    break;
                default:
                    realmPacketOut.Write((byte) 0);
                    break;
            }

            return realmPacketOut;
        }

        /// <summary>Sends a guild log to a client</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="guild">guild</param>
        public static void SendGuildEventLog(IPacketReceiver client, Guild guild)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_EVENT_LOG_QUERY))
            {
                GuildEventLog eventLog = guild.EventLog;
                long position = packet.Position;
                packet.WriteByte(0);
                int num = 0;
                foreach (GuildEventLogEntry guildEventLogEntry in eventLog)
                {
                    ++num;
                    packet.WriteByte((byte) guildEventLogEntry.Type);
                    packet.Write((ulong) EntityId.GetPlayerId((uint) guildEventLogEntry.Character1LowId));
                    if (guildEventLogEntry.Type != GuildEventLogEntryType.JOIN_GUILD &&
                        guildEventLogEntry.Type != GuildEventLogEntryType.LEAVE_GUILD)
                        packet.Write((ulong) EntityId.GetPlayerId((uint) guildEventLogEntry.Character2LowId));
                    if (guildEventLogEntry.Type == GuildEventLogEntryType.PROMOTE_PLAYER ||
                        guildEventLogEntry.Type == GuildEventLogEntryType.DEMOTE_PLAYER)
                        packet.WriteByte(guildEventLogEntry.NewRankId);
                    packet.Write((int) (DateTime.Now - guildEventLogEntry.TimeStamp).TotalSeconds);
                }

                packet.Position = position;
                packet.Write(num);
                client.Send(packet, false);
            }
        }

        public static void SendGuildBankPermissions(Character chr)
        {
            GuildMember guildMember = chr.GuildMember;
            if (guildMember == null)
                return;
            Guild guild = guildMember.Guild;
            if (guild == null)
                return;
            GuildRank rank = guildMember.Rank;
            if (rank == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_PERMISSIONS))
            {
                packet.Write(rank.RankIndex);
                packet.Write((uint) rank.Privileges);
                packet.Write(guildMember.BankMoneyWithdrawlAllowance);
                packet.Write(guild.PurchasedBankTabCount);
                for (int index = 0; index < 6; ++index)
                {
                    packet.Write((uint) rank.BankTabRights[index].Privileges);
                    packet.Write(rank.BankTabRights[index].WithdrawlAllowance);
                }

                chr.Client.Send(packet, false);
            }
        }

        public static void SendGuildBankTabNames(Character chr, GameObject bank)
        {
            GuildHandler.SendGuildBankList(chr, bank, (byte) 0, true, false);
        }

        public static void SendGuildBankTabContents(Character chr, GameObject bank, byte tabId)
        {
            GuildHandler.SendGuildBankList(chr, bank, tabId, false, true);
        }

        public static void SendGuildBankTabContentUpdateToAll(this Guild guild, byte tabId, int tabSlot)
        {
            guild.SendGuildBankTabContentUpdateToAll(tabId, tabSlot, -1);
        }

        public static void SendGuildBankTabContentUpdateToAll(this Guild guild, byte tabId, int slot1, int slot2)
        {
            lock (guild)
            {
                foreach (GuildMember member in (IEnumerable<GuildMember>) guild.Members.Values)
                {
                    if (!member.Rank.BankTabRights[(int) tabId].Privileges
                        .HasFlag((Enum) GuildBankTabPrivileges.ViewTab))
                        break;
                    GuildHandler.SendGuildBankTabContentUpdate(member, tabId, slot1, slot2);
                }
            }
        }

        private static void SendGuildBankTabContentUpdate(GuildMember member, byte tabId, int slot1, int slot2)
        {
        }

        public static void SendGuildBankMoneyUpdate(Character chr, GameObject bank)
        {
            GuildHandler.SendGuildBankList(chr, bank, (byte) 0, false, false);
        }

        private static void SendGuildBankList(Character chr, GameObject bank, byte tabId, bool hasTabNames,
            bool hasItemInfo)
        {
            if (bank == null || chr.Guild == null || (chr.GuildMember == null || !bank.CanBeUsedBy(chr)))
                return;
            Guild guild = chr.Guild;
            GuildBank bank1 = guild.Bank;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CMSG_GUILD_BANK_QUERY_TAB))
            {
                packet.Write(guild.Money);
                packet.Write(tabId);
                packet.Write(chr.GuildMember.Rank.BankTabRights[(int) tabId].WithdrawlAllowance);
                bool flag = tabId == (byte) 0 && hasTabNames;
                packet.Write(flag);
                if (flag)
                {
                    packet.Write((byte) guild.PurchasedBankTabCount);
                    for (int index = 0; index < guild.PurchasedBankTabCount; ++index)
                    {
                        packet.Write(bank1[index].Name);
                        packet.Write(bank1[index].Icon);
                    }
                }

                if (!hasItemInfo)
                {
                    chr.Client.Send(packet, false);
                }
                else
                {
                    GuildBankTab guildBankTab = bank1[(int) tabId];
                    int num1 = guildBankTab.ItemRecords
                        .Where<ItemRecord>((Func<ItemRecord, bool>) (record => record != null)).Count<ItemRecord>();
                    packet.Write((byte) num1);
                    foreach (ItemRecord itemRecord in guildBankTab.ItemRecords.Where<ItemRecord>(
                        (Func<ItemRecord, bool>) (record => record != null)))
                    {
                        packet.Write((byte) itemRecord.Slot);
                        packet.Write(itemRecord.EntryId);
                        packet.Write(0U);
                        int randomProperty = itemRecord.RandomProperty;
                        packet.Write(randomProperty);
                        if (randomProperty > 0)
                            packet.Write(itemRecord.RandomSuffix);
                        packet.Write(itemRecord.Amount);
                        packet.Write(0U);
                        packet.Write((byte) 0);
                        if (itemRecord.EnchantIds == null)
                        {
                            packet.Write((byte) 0);
                        }
                        else
                        {
                            long position = packet.Position;
                            int num2 = 0;
                            for (int index = 0; index < 3; ++index)
                            {
                                if (itemRecord.EnchantIds[index] != 0)
                                {
                                    packet.Write((byte) index);
                                    packet.Write(itemRecord.EnchantIds[index]);
                                    ++num2;
                                }
                            }

                            packet.InsertByteAt((byte) num2, position, true);
                        }
                    }

                    chr.Client.Send(packet, false);
                }
            }
        }

        public static void GetBankTabContent(Character chr, GameObject bank, byte tabId)
        {
            if (bank == null || chr.Guild == null || (chr.GuildMember == null || !bank.CanBeUsedBy(chr)) ||
                (chr.Guild.Bank[(int) tabId] == null ||
                 !chr.GuildMember.HasBankTabRight((int) tabId, GuildBankTabPrivileges.ViewTab)))
                return;
            GuildHandler.SendMemberRemainingDailyWithdrawlAllowance((IPacketReceiver) chr,
                chr.GuildMember.BankMoneyWithdrawlAllowance);
            GuildHandler.SendGuildBankTabContents(chr, bank, tabId);
        }

        public static void SendGuildBankTabText(Character chr, byte tabId, string text)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_QUERY_GUILD_BANK_TEXT))
            {
                packet.Write(tabId);
                packet.Write(text);
                chr.Client.Send(packet, false);
            }
        }

        public static RealmPacketOut CreateBankTabTextPacket(byte tabId, string text)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.MSG_QUERY_GUILD_BANK_TEXT);
            realmPacketOut.Write(tabId);
            realmPacketOut.Write(text);
            return realmPacketOut;
        }

        public static void SendGuildBankLog(Character chr, GuildBankLog log, byte tabId)
        {
            if (log == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_BANK_LOG_QUERY))
            {
                IEnumerable<GuildBankLogEntry> bankLogEntries = log.GetBankLogEntries(tabId);
                packet.Write(tabId);
                long position = packet.Position;
                packet.Write((byte) 0);
                int num1 = 0;
                foreach (GuildBankLogEntry guildBankLogEntry in bankLogEntries)
                {
                    ++num1;
                    packet.Write((byte) guildBankLogEntry.Type);
                    packet.Write((ulong) guildBankLogEntry.Actor.EntityId);
                    if (guildBankLogEntry.Type == GuildBankLogEntryType.DepositMoney ||
                        guildBankLogEntry.Type == GuildBankLogEntryType.WithdrawMoney ||
                        (guildBankLogEntry.Type == GuildBankLogEntryType.MoneyUsedForRepairs ||
                         guildBankLogEntry.Type == GuildBankLogEntryType.Unknown1) ||
                        guildBankLogEntry.Type == GuildBankLogEntryType.Unknown2)
                    {
                        packet.Write(guildBankLogEntry.Money);
                    }
                    else
                    {
                        packet.Write(guildBankLogEntry.ItemEntryId);
                        packet.Write(guildBankLogEntry.ItemStackCount);
                        if (guildBankLogEntry.Type == GuildBankLogEntryType.MoveItem ||
                            guildBankLogEntry.Type == GuildBankLogEntryType.MoveItem_2)
                            packet.Write((byte) guildBankLogEntry.DestinationTabId);
                    }

                    uint num2 = Utility.GetDateTimeToGameTime(DateTime.Now) -
                                Utility.GetDateTimeToGameTime(guildBankLogEntry.Created);
                    packet.Write(num2);
                }

                packet.Position = position;
                packet.Write(num1);
                chr.Send(packet, false);
            }
        }

        public static void SendMemberRemainingDailyWithdrawlAllowance(IPacketReceiver client, uint remainingAllowance)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_GUILD_BANK_MONEY_WITHDRAWN))
            {
                packet.Write(remainingAllowance);
                client.Send(packet, false);
            }
        }

        public static void SendTabardResult(IPacketReceiver client, GuildTabardResult result)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_SAVE_GUILD_EMBLEM))
            {
                packet.Write((uint) result);
                client.Send(packet, false);
            }
        }
    }
}