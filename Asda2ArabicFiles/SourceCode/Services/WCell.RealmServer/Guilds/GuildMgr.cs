/*************************************************************************
 *
 *   file		: GuildMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate$
 *   last author	: $LastChangedBy$
 *   revision		: $Rev$
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NLog;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
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

		public const int MIN_GUILD_RANKS = 5;
		public const int MAX_GUILD_RANKS = 10;
		public static int MaxGuildNameLength = 20;
		public static int MaxGuildRankNameLength = 10;
		public static int MaxGuildMotdLength = 100;
		public static int MaxGuildInfoLength = 500;
		public static int MaxGuildMemberNoteLength = 100;

		/// <summary>
		/// Cost (in copper) of a new Guild Tabard
		/// </summary>
		public static uint GuildTabardCost = 100000;

		/// <summary>
		/// The delay (in hours) before Guild Members' BankMoneyWithdrawlAllowance resets.
		/// </summary>
		public const int BankMoneyAllowanceResetDelay = 24;

		public const uint UNLIMITED_BANK_MONEY_WITHDRAWL = UInt32.MaxValue;
		public const uint UNLIMITED_BANK_SLOT_WITHDRAWL = UInt32.MaxValue;
		public const int MAX_BANK_TABS = 6;
		public const int MAX_BANK_TAB_SLOTS = 98;

		public static uint GuildCharterCost
		{
			get { return guildCharterCost; }
			set
			{
				guildCharterCost = value;
				PetitionerEntry.GuildPetitionEntry.Cost = value;
			}
		}

		private static int requiredCharterSignature = 9;

		public static int RequiredCharterSignature
		{
			get { return requiredCharterSignature; }
			set
			{
				requiredCharterSignature = value;
				PetitionerEntry.GuildPetitionEntry.RequiredSignatures = value;
			}
		}

		/// <summary>
		/// Maps char-id to the corresponding GuildMember object so it can be looked up when char reconnects
		/// </summary>
		public static readonly IDictionary<uint, GuildMember> OfflineMembers;
		public static readonly IDictionary<uint, Guild> GuildsById;
		public static readonly IDictionary<string, Guild> GuildsByName;
		private static readonly ReaderWriterLockWrapper guildsLock = new ReaderWriterLockWrapper();
		private static readonly ReaderWriterLockWrapper membersLock = new ReaderWriterLockWrapper();

		#region Init
		static GuildMgr()
		{
			GuildsById = new SynchronizedDictionary<uint, Guild>();
			GuildsByName = new SynchronizedDictionary<string, Guild>(StringComparer.InvariantCultureIgnoreCase);
			OfflineMembers = new SynchronizedDictionary<uint, GuildMember>();
		}

		private GuildMgr()
		{
		}

		[Initialization(InitializationPass.Fifth, "Initialize Guilds")]
		public static bool Initialize()
		{
            CharacterFormulas.InitGuildSkills();
			return Instance.Start();
		}

		private bool Start()
		{
			Guild[] guilds = null;

#if DEBUG
			try
			{
#endif
				guilds = ActiveRecordBase<Guild>.FindAll();
#if DEBUG
			}
			catch (Exception e)
			{
				RealmDBMgr.OnDBError(e);
				guilds = ActiveRecordBase<Guild>.FindAll();
			}
#endif

			if (guilds != null)
			{
				foreach (var guild in guilds)
				{
					guild.InitAfterLoad();
				}
			}

			return true;
		}
		#endregion

		public static ImmutableList<GuildRank> CreateDefaultRanks(Guild guild)
		{
			var ranks = new ImmutableList<GuildRank>();
			var ranksNum = 0;
			var gmRank = new GuildRank(guild, "Guild Master", GuildPrivileges.All, ranksNum++);
			ranks.Add(gmRank);
			ranks.Add(new GuildRank(guild, "Officer", GuildPrivileges.Applicants, ranksNum++));
			ranks.Add(new GuildRank(guild, "Veteran", GuildPrivileges.Default, ranksNum++));
			ranks.Add(new GuildRank(guild, "Member", GuildPrivileges.Default, ranksNum++));
			ranks.Add(new GuildRank(guild, "Initiate", GuildPrivileges.Default, ranksNum));
			return ranks;

		}

		internal void OnCharacterLogin(Character chr)
		{
			GuildMember member;
			using (membersLock.EnterWriteLock())
			{
				if (OfflineMembers.TryGetValue(chr.EntityId.Low, out member))
				{
					OfflineMembers.Remove(chr.EntityId.Low);
					member.Character = chr;
				}
			}

			if (member != null)
			{
				chr.GuildMember = member;
				if (member.Guild != null)
				{
                    //Asda2GuildHandler.SendGuildNotificationResponse(member.Guild,GuildNotificationType.LoggedIn, chr.GuildMember);
					//GuildHandler.SendEventToGuild(member.Guild, GuildEvents.ONLINE, member);
				}
				else
				{
					// now this is bad
					LogManager.GetCurrentClassLogger().Warn("Found orphaned GuildMember for character \"{0}\" during logon.");
				}
			    foreach (var activeSkill in chr.Guild.ActiveSkills)
			    {
			        activeSkill.ApplyToCharacter(chr);
			    }
			}
		    
		}

		/// <summary>
		/// Cleanup character invitations and group leader, looter change on character logout/disconnect
		/// </summary>
		/// <param name="member">The GuildMember logging out / disconnecting (or null if the corresponding Character is not in a Guild)</param>
		internal void OnCharacterLogout(GuildMember member)
		{
			if (member == null)
			{
				// null check is only required because we stated in the documentation of this method that memeber is allowed to be null
				return;
			}

			var chr = member.Character;
			var listInviters = RelationMgr.Instance.GetPassiveRelations(chr.EntityId.Low, CharacterRelationType.GuildInvite);

			foreach (IBaseRelation inviteRelation in listInviters)
			{
				RelationMgr.Instance.RemoveRelation(inviteRelation);
			}
            foreach (var activeSkill in chr.Guild.ActiveSkills)
            {
                activeSkill.RemoveFromCharacter(chr);
            }
			var guild = member.Guild;

			if (guild == null) // ???
				return;

			member.LastLogin = DateTime.Now;
			var zone = member.Character.Zone;
			member.ZoneId = zone != null ? (int)zone.Id : 0;
			member.Class = member.Character.Class;
			member.Level = member.Character.Level;

			member.Character = null;

			member.UpdateLater();

			using (membersLock.EnterWriteLock())
			{
				OfflineMembers[chr.EntityId.Low] = member;
			}

			GuildHandler.SendEventToGuild(member.Guild, GuildEvents.OFFLINE, member);
		}

		/// <summary>
		/// New or loaded Guild
		/// </summary>
		/// <param name="guild"></param>
		internal void RegisterGuild(Guild guild)
		{
			using (guildsLock.EnterWriteLock())
			{
				GuildsById.Add(guild.Id, guild);
				GuildsByName.Add(guild.Name, guild);
				using (membersLock.EnterWriteLock())
				{
					foreach (var gm in guild.Members.Values)
					{
						if (gm.Character == null && !OfflineMembers.ContainsKey(gm.Id))
						{
							OfflineMembers.Add(gm.Id, gm);
						}
					}
				}
			}
		}

		internal void UnregisterGuild(Guild guild)
		{
			using (guildsLock.EnterWriteLock())
			{
				GuildsById.Remove(guild.Id);
				GuildsByName.Remove(guild.Name);
				// no need to remove offline members, since, at this point
				// all members have already been evicted
			}
		}

		internal void RegisterGuildMember(GuildMember gm)
		{
			if (gm.Character == null)
			{
				using (membersLock.EnterWriteLock())
				{
					OfflineMembers.Add(gm.Id, gm);
				}
			}
		}

		internal void UnregisterGuildMember(GuildMember gm)
		{
			using (membersLock.EnterWriteLock())
			{
				OfflineMembers.Remove(gm.Id);
			}
		}

		public static Guild GetGuild(uint guildId)
		{
			using (guildsLock.EnterReadLock())
			{
				Guild guild;
				GuildsById.TryGetValue(guildId, out guild);
				return guild;
			}
		}

		public static Guild GetGuild(string name)
		{
			using (guildsLock.EnterReadLock())
			{
				Guild guild;
				GuildsByName.TryGetValue(name, out guild);
				return guild;
			}
		}

		#region Checks
		public static bool CanUseName(string name)
		{
			if (IsValidGuildName(name))
			{
				return GetGuild(name) == null;
			}
			return false;
		}

		public static bool DoesGuildExist(string name)
		{
			return GetGuild(name) != null;
		}

		public static bool IsValidGuildName(string name)
		{
			name = name.Trim();
			if (name.Length < 3 && name.Length > MaxGuildNameLength || name.Contains(" "))
			{
				return false;
			}

			return true;
		}

	
		#endregion

	    public static void OnShutdown()
	    {
            using (guildsLock.EnterReadLock())
	        {
                foreach (var g in GuildsById)
                {
                    g.Value.Save();
                }
	        }
	        
	    }
	}
}