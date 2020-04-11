/*************************************************************************
 *
 *   file		: Account.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-12 05:32:34 +0800 (Thu, 12 Mar 2009) $

 *   revision		: $Rev: 794 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Intercommunication;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Auth;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.Res;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer
{
	/// <summary>
	/// Represents the Account that a client used to login with on RealmServer side.
	/// </summary>
	public partial class RealmAccount : IAccount
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		#region Fields
		protected int m_accountId;
		protected string m_email;
		protected int m_HighestCharLevel;
		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="accountName">the name of the account</param>
		public RealmAccount(string accountName, IAccountInfo info)
		{
			Name = accountName;

			Characters = new List<CharacterRecord>();

			m_accountId = info.AccountId;
			ClientId = ClientId.Original;
			IsActive = true;
			Role = PrivilegeMgr.Instance.GetRoleOrDefault(info.RoleGroupName);
			m_email = info.EmailAddress;
			LastIP = info.LastIP;
			LastLogin = info.LastLogin;
			Locale = ClientLocale.Russian;
		}

		#region Properties
		/// <summary>
		/// Still in Auth-Queue and waiting for a free slot
		/// </summary>
		public bool IsEnqueued
		{
			get;
			internal set;
		}

		/// <summary>
		/// The username of this account.
		/// </summary>
		public string Name
		{
			get;
			protected set;
		}

		public bool IsActive
		{
			get;
			protected set;
		}

		public DateTime? StatusUntil
		{
			get;
			protected set;
		}

		/// <summary>
		/// The database row ID for this account.
		/// Don't change it.
		/// </summary>
		public int AccountId
		{
			get
			{
				return m_accountId;
			}
		}

		/// <summary>
		/// The e-mail address of this account.
		/// </summary>
		/// <remarks>Use <c>SetEmail</c> instead to change the EmailAddress.</remarks>
		public string EmailAddress
		{
			get
			{
				return m_email;
			}
		}

		/// <summary>
		/// Setting this would not be saved to DB.
		/// </summary>
		public ClientId ClientId
		{
			get;
			protected set;
		}

		/// <summary>
		/// The last IP-Address that this Account connected with
		/// </summary>
		public byte[] LastIP
		{
			get;
			set;
		}

		/// <summary>
		/// The time of when this Account last logged in.
		/// Might be null.
		/// </summary>
		public DateTime? LastLogin
		{
			get;
			protected set;
		}

		public int HighestCharLevel
		{
			get { return m_HighestCharLevel; }
			set
			{
				m_HighestCharLevel = value;
				/*RealmServer.IOQueue.AddMessage(new Message(() => {
					if (RealmServer.Instance.AuthClient.IsRunning)
					{
						RealmServer.Instance.AuthClient.Channel.SetHighestLevel(AccountId,
																				m_HighestCharLevel);
					}
				}));*/
			}
		}

		public ClientLocale Locale
		{
			get;
			private set;
		}

		/// <summary>
		/// The name of the RoleGroup.
		/// </summary>
		/// <remarks>
		/// Implements <see cref="IAccountInfo.RoleGroupName"/>.
		/// Use <c>SetRole</c> to change the Role.
		/// </remarks>
		public string RoleGroupName
		{
			get
			{
				return Role.Name;
			}
		}

		/// <summary>
		/// The RoleGroup of this Account.
		/// </summary>
		/// <remarks>Use <c>SetRole</c> to change the Role.</remarks>
		public RoleGroup Role
		{
			get;
			protected set;
		}

		/// <summary>
		/// All the character associated with this account.
		/// </summary>
		public List<CharacterRecord> Characters
		{
			get;
			protected set;
		}

		/// <summary>
		/// The Character that is currently being used by this Account (or null)
		/// </summary>
		public Character ActiveCharacter
		{
			get;
			internal set;
		}

		/// <summary>
		/// The client that is connected to this Account.
		/// If connected, the client is either still selecting a Character,
		/// seeing the Login-screen or already ingame (in which case ActiveCharacter is also set).
		/// </summary>
		public IRealmClient Client
		{
			get;
			internal set;
		}

        /// <summary>
        /// The account data cache, related to this account.
        /// </summary>
        public AccountDataRecord AccountData
        {
            get;
            internal set;
        }


	    #endregion

		#region Methods
		public CharacterRecord GetCharacterRecord(uint id)
		{
			foreach (var chr in Characters)
			{
				if (chr.EntityLowId == id)
				{
					return chr;
				}
			}
			return null;
		}

		public void RemoveCharacterRecord(uint id)
		{
			for (var i = 0; i < Characters.Count; i++)
			{
				var chr = Characters[i];
				if (chr.EntityLowId == id)
				{
					Characters.RemoveAt(i);
					break;
				}
			}
		}

		/// <summary>
		/// Tells the AuthServer to change the role for this Account.
		/// </summary>
		/// <param name="role">the new role for this account</param>
		/// <returns>true if the role was set; false otherwise</returns>
		/// <remarks>Requires IO-Context</remarks>
		public bool SetRole(RoleGroup role)
		{
			var wasStaff = Role.IsStaff;
			if (!Role.Equals(role))
			{
				/*if (!RealmServer.Instance.AuthClient.Channel.SetAccountRole(AccountId, role.Name))
				{
					return false;
				}*/

				Role = role;
				if (wasStaff != role.IsStaff)
				{
					var chr = ActiveCharacter;
					if (chr != null)
					{
						var map = chr.Map;
						var context = chr.ContextHandler;
						if (context != null)
						{
							context.AddMessage(() => {
								if (!chr.IsInWorld || chr.Map != context)
								{
									return;
								}

								if (wasStaff)
								{
									// not staff anymore
									World.StaffMemberCount--;
									map.IncreasePlayerCount(chr);
								}
								else
								{
									// new staff
									World.StaffMemberCount++;
									map.DecreasePlayerCount(chr);
								}
							});
						}
					}
				}
			}

			return true;
		}

		public bool SetAccountActive(bool active, DateTime? statusUntil)
		{
		    var acc =AccountMgr.GetAccount(Name);
            if (acc == null)
                return false;
		    acc.StatusUntil = statusUntil;
		    acc.IsActive = active;
		    acc.SaveLater();
			IsActive = active;
			StatusUntil = statusUntil;
            if (ActiveCharacter != null && !active)
                ActiveCharacter.Kick("Banned");
			return true;
		}

		/// <summary>
		/// Sets the e-mail address for this account and persists it to the DB.
		/// Blocking call. Make sure to call this from outside the Map-Thread.
		/// </summary>
		/// <param name="email">the new e-mail address for this account</param>
		/// <returns>true if the e-mail address was set; false otherwise</returns>
		/// <remarks>Requires IO-Context</remarks>
		public bool SetEmail(string email)
		{
			if (EmailAddress != email)
			{
				//if (!RealmServer.Instance.AuthClient.Channel.SetAccountEmail(AccountId, email))
				//{
				//	return false;
				//}

				m_email = email;
			}

			return true;
		}

		/// <summary>
		/// Sets the password for this account and sends it to the Authserver to be saved.
		/// Blocking call. Make sure to call this from outside the Map-Thread.
		/// </summary>
		/// <returns>true if the e-mail address was set; false otherwise</returns>
		public bool SetPass(string oldPassStr, string passStr)
		{
		    return true;
		}

		/// <summary>
		/// Reloads all characters belonging to this account from the database.
		/// Blocking call. Make sure to call this from outside the Map-Thread.
		/// </summary>
		void LoadCharacters()
		{
			var chrs = CharacterRecord.FindAllOfAccount(this);
			for (var i = 0; i < chrs.Length; i++)
			{
				var chr = chrs[i];
				Characters.Add(chr);
			}
		}

		/// <summary>
        /// Loads account based data, creates base data if no data is found.
        /// </summary>
        void LoadAccountData()
        {
            var adr = AccountDataRecord.GetAccountData(AccountId) ?? AccountDataRecord.InitializeNewAccount(AccountId);

            AccountData = adr;
        }
		#endregion

		public override string ToString()
		{
			return Name + " (Id: " + AccountId + ")";
		}

		/// <summary>
		/// Called from within the IO-Context
		/// </summary>
		/// <param name="client"></param>
		/// <param name="accountName"></param>
		internal static void InitializeAccount(IRealmClient client, string accountName)
		{
			if (!client.IsConnected)
			{
				return;
			}

			if (RealmServer.Instance.IsAccountLoggedIn(accountName))
			{
				log.Info("Client ({0}) tried to use online Account: {1}.", client, accountName);
				AuthenticationHandler.OnLoginError(client,AccountStatus.AccountInUse);
                client.Disconnect();
                return;
			}
            var addr = client.ClientAddress;
		    if (addr == null)
		    {
		        return;
		    }
		    var accountInfo = new AccountInfo
		                          {
		                              AccountId = client.AuthAccount.AccountId,
		                              EmailAddress = "",
		                              LastIP = client.ClientAddress.GetAddressBytes(),
		                              LastLogin = DateTime.Now,
                                      RoleGroupName =client.AuthAccount.RoleGroupName
		                          };
				var account = new RealmAccount(accountName, accountInfo);
				RealmServer.Instance.RegisterAccount(account);
				account.LoadCharacters();
			    account.LoadAccountData();

				account.Client = client;
				client.Account = account;

				log.Info("Account \"{0}\" logged in from {1}.", accountName, client.ClientAddress);
		}

		

		internal void OnLogin()
		{
			var evt = LoggedIn;
			if (evt != null)
			{
				evt(this);
			}
		}

		internal void OnLogout()
		{
		    if (AccountData != null) AccountData.Update();

		    var evt = LoggedOut;
			if (evt != null)
			{
				evt(this);
			}
		}
	}

}