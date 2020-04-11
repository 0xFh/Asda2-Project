/*************************************************************************
 *
 *   file		: Account.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-11 01:51:02 +0800 (Sun, 11 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 333 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using Castle.ActiveRecord;
using NLog;
using WCell.AuthServer.Privileges;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Database;
using WCell.Intercommunication;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Database;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Auth.Accounts
{
	/// <summary>
	/// Class for performing account-related tasks.
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class Account : WCellRecord<Account>, IAccount
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(Account), "AccountId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static int NextId()
		{
			return (int) _idGenerator.Next();
		}

		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		private bool m_IsActive;

		/// <summary>
		/// Queries the DB for the count of all existing Accounts.
		/// </summary>
		/// <returns></returns>
		internal static int GetCount()
		{
			return Count();
		}

		/// <summary>
		/// Event is raised when the given Account logs in successfully with the given client.
		/// </summary>
		public static event Action<Account, IRealmClient> LoggedIn;

		public Account()
		{
		}

		public Account(string username, string password, string email)
		{
			Name = username;
            Password = password;
			EmailAddress = email;
		    LastIP = IPAddress.Any.GetAddressBytes();
			AccountId = NextId();
			State = RecordState.New;
		}

		internal void OnLogin(IRealmClient client)
		{
			var addr = client.ClientAddress;
			if (addr == null)
			{
				// client disconnected
				return;
            }

            LastIP = addr.GetAddressBytes();
            LastLogin = DateTime.Now;
			UpdateAndFlush();

			//!!!AuthCommandHandler.AutoExecute(this);

			var evt = LoggedIn;
			if (evt != null)
			{
				evt(this, client);
			}

		}

		public void Update(IAccount newInfo)
		{
			// TODO: Status changed - kick Account if banned?

			IsActive = newInfo.IsActive;
			StatusUntil = newInfo.StatusUntil;
			EmailAddress = newInfo.EmailAddress;
			RoleGroupName = newInfo.RoleGroupName;
		}

		#region Props
		[PrimaryKey(PrimaryKeyType.Assigned)]
		public int AccountId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public DateTime Created
		{
			get;
			set;
		}

		[Property(Length = 16, NotNull = true, Unique = true)]
		public string Name
		{
			get;
			set;
		}

        [Property(Length = 20, NotNull = true)]
		public string Password
		{
			get;
			set;
		}


	    [Property]
		public string EmailAddress
		{
			get;
			set;
		}

	    public ClientId ClientId
	    {
            get { return ClientId.Original; }
	    }

	    [Property]
		public string ClientVersion
		{
			get;
			set;
		}

		[Property(Length = 16, NotNull = true)]
		public string RoleGroupName
		{
			get;
			set;
		}

		public RoleGroupInfo Role
		{
			get { return Singleton<PrivilegeMgr>.Instance.GetRoleGroup(RoleGroupName); }
		}

		/// <summary>
		/// Whether the Account may currently be used 
		/// (inactive Accounts are banned).
		/// </summary>
		[Property(NotNull = true)]
		public bool IsActive
		{
			get { return m_IsActive; }
			set
			{
				m_IsActive = value;
				StatusUntil = null;
			}
		}

		/// <summary>
		/// If set: Once this time is reached,
		/// the Active status of this account will be toggled
		/// (from inactive to active or vice versa)
		/// </summary>
		[Property]
		public DateTime? StatusUntil
		{
			get;
			set;
		}

		/// <summary>
		/// The time of when this Account last changed from outside. Used for Synchronization.
		/// </summary>
		/// <remarks>Only Accounts that changed, will be fetched from DB during resync when caching is enabled.</remarks>
		[Property]
		public DateTime? LastChanged
		{
			get;
			set;
		}

		[Property]
		public DateTime? LastLogin
		{
			get;
			set;
		}

		[Property]
		public byte[] LastIP
		{
			get;
			set;
		}

		[Property]
		public int HighestCharLevel
		{
			get;
			set;
		}

		[Property]
		public ClientLocale Locale
		{
			get;
			set;
		}

		#endregion

		public string LastIPStr
		{
			get
			{
				return new IPAddress(LastIP).ToString();
			}
		}

		public bool CheckActive()
		{
			if (StatusUntil != null && StatusUntil > DateTime.Now)
			{
				m_IsActive = !m_IsActive;
				StatusUntil = null;
				Save();
			}
			return m_IsActive;
		}

		public override void Delete()
		{
			AccountMgr.Instance.Remove(this);
			base.Delete();
		}

		public override void DeleteAndFlush()
		{
			AccountMgr.Instance.Remove(this);
			base.DeleteAndFlush();
		}

		public string Details
		{
			get
			{
				return string.Format("Account: {0} ({1}) is {7} " +
									 "({9}Role: {2}, Age: {3}, Last IP: {4}, Last Login: {5}, Version: {6}, Locale: {8})",
									 Name, AccountId, RoleGroupName,
									 (DateTime.Now - Created).Format(),
									 LastIPStr,
									 LastLogin != null ? LastLogin.ToString() : "<Never>",
									 0,
									 ServerApp<RealmServer>.Instance.IsAccountLoggedIn(Name) ? "Online" : "Offline",
									 Locale,
									 (IsActive ? "" : "INACTIVE") + (StatusUntil != null ? " (Until: " + StatusUntil : ""));
			}
		}



	    public override string ToString()
		{
			return Name + " (Id: " + AccountId + ")";
		}

	    public bool IsLogedOn { get; set; }

	    private List<CharacterRecord> _characters;
        /// <summary>
        /// If not initialized load characters from DB.
        /// </summary>
        public List<CharacterRecord> Characters
	    {
	       get{return _characters??(_characters = new List<CharacterRecord>(CharacterRecord.FindAllOfAccount( AccountId)));}
	    }

	    public string Status
	    {
            get { return IsActive ? "Доступен" : "Заблокирован"; }
	    }
	}
}