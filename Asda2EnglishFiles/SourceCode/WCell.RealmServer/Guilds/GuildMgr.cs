using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Collections;

namespace WCell.RealmServer.Guilds
{
    /// <summary>
    /// </summary>
    public sealed class GuildMgr : Manager<GuildMgr>
    {
        private static uint guildCharterCost = 1000;
        public static int MaxGuildNameLength = 20;
        public static int MaxGuildRankNameLength = 10;
        public static int MaxGuildMotdLength = 100;
        public static int MaxGuildInfoLength = 500;
        public static int MaxGuildMemberNoteLength = 100;

        /// <summary>Cost (in copper) of a new Guild Tabard</summary>
        public static uint GuildTabardCost = 100000;

        private static int requiredCharterSignature = 9;
        private static readonly ReaderWriterLockWrapper guildsLock = new ReaderWriterLockWrapper();
        private static readonly ReaderWriterLockWrapper membersLock = new ReaderWriterLockWrapper();

        public static readonly IDictionary<uint, Guild> GuildsById =
            (IDictionary<uint, Guild>) new SynchronizedDictionary<uint, Guild>();

        public static readonly IDictionary<string, Guild> GuildsByName =
            (IDictionary<string, Guild>) new SynchronizedDictionary<string, Guild>(
                (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Maps char-id to the corresponding GuildMember object so it can be looked up when char reconnects
        /// </summary>
        public static readonly IDictionary<uint, GuildMember> OfflineMembers =
            (IDictionary<uint, GuildMember>) new SynchronizedDictionary<uint, GuildMember>();

        public const int MIN_GUILD_RANKS = 5;
        public const int MAX_GUILD_RANKS = 10;

        /// <summary>
        /// The delay (in hours) before Guild Members' BankMoneyWithdrawlAllowance resets.
        /// </summary>
        public const int BankMoneyAllowanceResetDelay = 24;

        public const uint UNLIMITED_BANK_MONEY_WITHDRAWL = 4294967295;
        public const uint UNLIMITED_BANK_SLOT_WITHDRAWL = 4294967295;
        public const int MAX_BANK_TABS = 6;
        public const int MAX_BANK_TAB_SLOTS = 98;

        public static uint GuildCharterCost
        {
            get { return GuildMgr.guildCharterCost; }
            set
            {
                GuildMgr.guildCharterCost = value;
                PetitionerEntry.GuildPetitionEntry.Cost = value;
            }
        }

        public static int RequiredCharterSignature
        {
            get { return GuildMgr.requiredCharterSignature; }
            set
            {
                GuildMgr.requiredCharterSignature = value;
                PetitionerEntry.GuildPetitionEntry.RequiredSignatures = value;
            }
        }

        private GuildMgr()
        {
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize Guilds")]
        public static bool Initialize()
        {
            CharacterFormulas.InitGuildSkills();
            return Singleton<GuildMgr>.Instance.Start();
        }

        private bool Start()
        {
            Guild[] all = ActiveRecordBase<Guild>.FindAll();
            if (all != null)
            {
                foreach (Guild guild in all)
                    guild.InitAfterLoad();
            }

            return true;
        }

        public static ImmutableList<GuildRank> CreateDefaultRanks(Guild guild)
        {
            ImmutableList<GuildRank> immutableList1 = new ImmutableList<GuildRank>();
            int num1 = 0;
            Guild guild1 = guild;
            string name1 = "Guild Master";
            int maxValue = (int) sbyte.MaxValue;
            int id1 = num1;
            int num2 = id1 + 1;
            GuildRank guildRank1 = new GuildRank(guild1, name1, (GuildPrivileges) maxValue, id1);
            immutableList1.Add(guildRank1);
            ImmutableList<GuildRank> immutableList2 = immutableList1;
            Guild guild2 = guild;
            string name2 = "Officer";
            int num3 = 2;
            int id2 = num2;
            int num4 = id2 + 1;
            GuildRank guildRank2 = new GuildRank(guild2, name2, (GuildPrivileges) num3, id2);
            immutableList2.Add(guildRank2);
            ImmutableList<GuildRank> immutableList3 = immutableList1;
            Guild guild3 = guild;
            string name3 = "Veteran";
            int num5 = 0;
            int id3 = num4;
            int num6 = id3 + 1;
            GuildRank guildRank3 = new GuildRank(guild3, name3, (GuildPrivileges) num5, id3);
            immutableList3.Add(guildRank3);
            ImmutableList<GuildRank> immutableList4 = immutableList1;
            Guild guild4 = guild;
            string name4 = "Member";
            int num7 = 0;
            int id4 = num6;
            int id5 = id4 + 1;
            GuildRank guildRank4 = new GuildRank(guild4, name4, (GuildPrivileges) num7, id4);
            immutableList4.Add(guildRank4);
            immutableList1.Add(new GuildRank(guild, "Initiate", GuildPrivileges.None, id5));
            return immutableList1;
        }

        internal void OnCharacterLogin(Character chr)
        {
            GuildMember guildMember;
            using (GuildMgr.membersLock.EnterWriteLock())
            {
                if (GuildMgr.OfflineMembers.TryGetValue(chr.EntityId.Low, out guildMember))
                {
                    GuildMgr.OfflineMembers.Remove(chr.EntityId.Low);
                    guildMember.Character = chr;
                }
            }

            if (guildMember == null)
                return;
            chr.GuildMember = guildMember;
            if (guildMember.Guild == null)
                LogManager.GetCurrentClassLogger()
                    .Warn("Found orphaned GuildMember for character \"{0}\" during logon.");
            foreach (GuildSkill activeSkill in chr.Guild.ActiveSkills)
                activeSkill.ApplyToCharacter(chr);
        }

        /// <summary>
        /// Cleanup character invitations and group leader, looter change on character logout/disconnect
        /// </summary>
        /// <param name="member">The GuildMember logging out / disconnecting (or null if the corresponding Character is not in a Guild)</param>
        internal void OnCharacterLogout(GuildMember member)
        {
            if (member == null)
                return;
            Character character = member.Character;
            foreach (IBaseRelation passiveRelation in Singleton<RelationMgr>.Instance.GetPassiveRelations(
                character.EntityId.Low, CharacterRelationType.GuildInvite))
                Singleton<RelationMgr>.Instance.RemoveRelation(passiveRelation);
            foreach (GuildSkill activeSkill in character.Guild.ActiveSkills)
                activeSkill.RemoveFromCharacter(character);
            if (member.Guild == null)
                return;
            member.LastLogin = DateTime.Now;
            Zone zone = member.Character.Zone;
            member.ZoneId = zone != null ? (int) zone.Id : 0;
            member.Class = member.Character.Class;
            member.Level = member.Character.Level;
            member.Character = (Character) null;
            member.UpdateLater();
            using (GuildMgr.membersLock.EnterWriteLock())
                GuildMgr.OfflineMembers[character.EntityId.Low] = member;
            GuildHandler.SendEventToGuild(member.Guild, GuildEvents.OFFLINE, member);
        }

        /// <summary>New or loaded Guild</summary>
        /// <param name="guild"></param>
        internal void RegisterGuild(Guild guild)
        {
            using (GuildMgr.guildsLock.EnterWriteLock())
            {
                GuildMgr.GuildsById.Add(guild.Id, guild);
                GuildMgr.GuildsByName.Add(guild.Name, guild);
                using (GuildMgr.membersLock.EnterWriteLock())
                {
                    foreach (GuildMember guildMember in (IEnumerable<GuildMember>) guild.Members.Values)
                    {
                        if (guildMember.Character == null && !GuildMgr.OfflineMembers.ContainsKey(guildMember.Id))
                            GuildMgr.OfflineMembers.Add(guildMember.Id, guildMember);
                    }
                }
            }
        }

        internal void UnregisterGuild(Guild guild)
        {
            using (GuildMgr.guildsLock.EnterWriteLock())
            {
                GuildMgr.GuildsById.Remove(guild.Id);
                GuildMgr.GuildsByName.Remove(guild.Name);
            }
        }

        internal void RegisterGuildMember(GuildMember gm)
        {
            if (gm.Character != null)
                return;
            using (GuildMgr.membersLock.EnterWriteLock())
                GuildMgr.OfflineMembers.Add(gm.Id, gm);
        }

        internal void UnregisterGuildMember(GuildMember gm)
        {
            using (GuildMgr.membersLock.EnterWriteLock())
                GuildMgr.OfflineMembers.Remove(gm.Id);
        }

        public static Guild GetGuild(uint guildId)
        {
            using (GuildMgr.guildsLock.EnterReadLock())
            {
                Guild guild;
                GuildMgr.GuildsById.TryGetValue(guildId, out guild);
                return guild;
            }
        }

        public static Guild GetGuild(string name)
        {
            using (GuildMgr.guildsLock.EnterReadLock())
            {
                Guild guild;
                GuildMgr.GuildsByName.TryGetValue(name, out guild);
                return guild;
            }
        }

        public static bool CanUseName(string name)
        {
            if (GuildMgr.IsValidGuildName(name))
                return GuildMgr.GetGuild(name) == null;
            return false;
        }

        public static bool DoesGuildExist(string name)
        {
            return GuildMgr.GetGuild(name) != null;
        }

        public static bool IsValidGuildName(string name)
        {
            name = name.Trim();
            return (name.Length >= 3 || name.Length <= GuildMgr.MaxGuildNameLength) && !name.Contains(" ");
        }

        public static void OnShutdown()
        {
            using (GuildMgr.guildsLock.EnterReadLock())
            {
                foreach (KeyValuePair<uint, Guild> keyValuePair in (IEnumerable<KeyValuePair<uint, Guild>>) GuildMgr
                    .GuildsById)
                    keyValuePair.Value.Save();
            }
        }
    }
}