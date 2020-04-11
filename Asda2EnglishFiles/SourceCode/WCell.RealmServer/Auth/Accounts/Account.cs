using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
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
    /// <summary>Class for performing account-related tasks.</summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class Account : WCellRecord<Account>, IAccount, IAccountInfo
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Account), nameof(AccountId), 1L);
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        private bool m_IsActive;
        private List<CharacterRecord> _characters;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static int NextId()
        {
            return (int) Account._idGenerator.Next();
        }

        /// <summary>
        /// Queries the DB for the count of all existing Accounts.
        /// </summary>
        /// <returns></returns>
        internal static int GetCount()
        {
            return ActiveRecordBase<Account>.Count();
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
            this.Name = username;
            this.Password = password;
            this.EmailAddress = email;
            this.LastIP = IPAddress.Any.GetAddressBytes();
            this.AccountId = Account.NextId();
            this.State = RecordState.New;
        }

        internal void OnLogin(IRealmClient client)
        {
            IPAddress clientAddress = client.ClientAddress;
            if (clientAddress == null)
                return;
            this.LastIP = clientAddress.GetAddressBytes();
            this.LastLogin = new DateTime?(DateTime.Now);
            this.UpdateAndFlush();
            Action<Account, IRealmClient> loggedIn = Account.LoggedIn;
            if (loggedIn == null)
                return;
            loggedIn(this, client);
        }

        public void Update(IAccount newInfo)
        {
            this.IsActive = newInfo.IsActive;
            this.StatusUntil = newInfo.StatusUntil;
            this.EmailAddress = newInfo.EmailAddress;
            this.RoleGroupName = newInfo.RoleGroupName;
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public int AccountId { get; set; }

        [Property(NotNull = true)] public DateTime Created { get; set; }

        [Property(Length = 16, NotNull = true, Unique = true)]
        public string Name { get; set; }

        [Property(Length = 20, NotNull = true)]
        public string Password { get; set; }

        [Property] public string EmailAddress { get; set; }

        public ClientId ClientId
        {
            get { return ClientId.Original; }
        }

        [Property] public string ClientVersion { get; set; }

        [Property(Length = 16, NotNull = true)]
        public string RoleGroupName { get; set; }

        public RoleGroupInfo Role
        {
            get { return Singleton<PrivilegeMgr>.Instance.GetRoleGroup(this.RoleGroupName); }
        }

        /// <summary>
        /// Whether the Account may currently be used
        /// (inactive Accounts are banned).
        /// </summary>
        [Property(NotNull = true)]
        public bool IsActive
        {
            get { return this.m_IsActive; }
            set
            {
                this.m_IsActive = value;
                this.StatusUntil = new DateTime?();
            }
        }

        /// <summary>
        /// If set: Once this time is reached,
        /// the Active status of this account will be toggled
        /// (from inactive to active or vice versa)
        /// </summary>
        [Property]
        public DateTime? StatusUntil { get; set; }

        /// <summary>
        /// The time of when this Account last changed from outside. Used for Synchronization.
        /// </summary>
        /// <remarks>Only Accounts that changed, will be fetched from DB during resync when caching is enabled.</remarks>
        [Property]
        public DateTime? LastChanged { get; set; }

        [Property] public DateTime? LastLogin { get; set; }

        [Property] public byte[] LastIP { get; set; }

        [Property] public int HighestCharLevel { get; set; }

        [Property] public ClientLocale Locale { get; set; }

        public string LastIPStr
        {
            get { return new IPAddress(this.LastIP).ToString(); }
        }

        public bool CheckActive()
        {
            if (this.StatusUntil.HasValue)
            {
                DateTime? statusUntil = this.StatusUntil;
                DateTime now = DateTime.Now;
                if ((statusUntil.HasValue ? (statusUntil.GetValueOrDefault() > now ? 1 : 0) : 0) != 0)
                {
                    this.m_IsActive = !this.m_IsActive;
                    this.StatusUntil = new DateTime?();
                    this.Save();
                }
            }

            return this.m_IsActive;
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
                return string.Format(
                    "Account: {0} ({1}) is {7} ({9}Role: {2}, Age: {3}, Last IP: {4}, Last Login: {5}, Version: {6}, Locale: {8})",
                    (object) this.Name, (object) this.AccountId, (object) this.RoleGroupName,
                    (object) (DateTime.Now - this.Created).Format(), (object) this.LastIPStr,
                    this.LastLogin.HasValue ? (object) this.LastLogin.ToString() : (object) "<Never>", (object) 0,
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.IsAccountLoggedIn(this.Name)
                        ? (object) "Online"
                        : (object) "Offline", (object) this.Locale,
                    (object) ((this.IsActive ? "" : "INACTIVE") +
                              (this.StatusUntil.HasValue ? " (Until: " + (object) this.StatusUntil : "")));
            }
        }

        public override string ToString()
        {
            return this.Name + " (Id: " + (object) this.AccountId + ")";
        }

        public bool IsLogedOn { get; set; }

        /// <summary>If not initialized load characters from DB.</summary>
        public List<CharacterRecord> Characters
        {
            get
            {
                return this._characters ?? (this._characters =
                           new List<CharacterRecord>(
                               (IEnumerable<CharacterRecord>) CharacterRecord.FindAllOfAccount(this.AccountId)));
            }
        }

        public string Status
        {
            get { return !this.IsActive ? "Заблокирован" : "Доступен"; }
        }
    }
}