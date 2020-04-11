using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.Intercommunication;
using WCell.RealmServer.Database;
using WCell.RealmServer.Logs;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Auth.Accounts
{
    /// <summary>
    /// Use this class to retrieve Accounts.
    /// Caching can be specified in the Config.
    /// If activated, Accounts will be cached and retrieved instantly
    /// instead of querying them from the server.
    /// Whenever accessing any of the 2 Account collections,
    /// make sure to also synchronize against the <c>Lock</c>.
    /// </summary>
    public class AccountMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static AccountMgr Instance = new AccountMgr();
        public static int MinAccountNameLen = 3;
        public static int MaxAccountNameLen = 20;
        public static readonly Account[] EmptyAccounts = new Account[0];
        private static int _accountReloadIntervalMs = 180000;
        public static readonly Regex DefaultNameValidationRegex = new Regex("^[A-Za-z0-9]+$");

        public static AccountMgr.NameValidationHandler NameValidator =
            new AccountMgr.NameValidationHandler(AccountMgr.ValidateNameDefault);

        private readonly TimerEntry _accountsReloadTimer = new TimerEntry();
        private ReaderWriterLockWrapper m_lock;
        private readonly Dictionary<long, Account> m_cachedAccsById;
        private readonly Dictionary<string, Account> m_cachedAccsByName;
        private bool m_IsCached;
        private DateTime m_lastResyncTime;

        /// <summary>
        /// Is called everytime, Accounts are (re-)fetched from DB (if caching is used)
        /// </summary>
        public event Action AccountsResync;

        /// <summary>
        /// Interval in milliseconds between reloading the account cache from the database
        /// if caching is enabled. Default is 180000ms == 3 minutes.
        /// </summary>
        public static int AccountReloadIntervalMs
        {
            set
            {
                AccountMgr._accountReloadIntervalMs = value;
                if (AccountMgr.Instance._accountsReloadTimer == null)
                    return;
                AccountMgr.Instance._accountsReloadTimer.IntervalMillis = value;
                AccountMgr.Instance._accountsReloadTimer.Start();
            }
            get { return AccountMgr._accountReloadIntervalMs; }
        }

        protected AccountMgr()
        {
            this.m_cachedAccsById = new Dictionary<long, Account>();
            this.m_cachedAccsByName =
                new Dictionary<string, Account>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>All cached Accounts by Id or null if not cached.</summary>
        public Dictionary<long, Account> AccountsById
        {
            get { return this.m_cachedAccsById; }
        }

        /// <summary>All cached Accounts by Name or null if not cached.</summary>
        public Dictionary<string, Account> AccountsByName
        {
            get { return this.m_cachedAccsByName; }
        }

        /// <summary>The count of all Accounts</summary>
        public int Count
        {
            get
            {
                if (!this.m_IsCached)
                    return Account.GetCount();
                return this.m_cachedAccsById.Count;
            }
        }

        /// <summary>
        /// Whether all Accounts are cached.
        /// Setting this value will correspondingly
        /// activate or deactivate caching.
        /// </summary>
        public bool IsCached
        {
            get { return this.m_IsCached; }
            set
            {
                if (this.m_IsCached == value)
                    return;
                if (value)
                    this.Cache();
                else
                    this.Purge();
                this.m_IsCached = value;
            }
        }

        public void ForeachAccount(Action<Account> action)
        {
            using (this.m_lock.EnterReadLock())
            {
                foreach (Account account in this.AccountsById.Values)
                    action(account);
            }
        }

        public IEnumerable<Account> GetAccounts(Predicate<Account> predicate)
        {
            using (this.m_lock.EnterReadLock())
            {
                foreach (Account account in this.AccountsById.Values)
                {
                    if (predicate(account))
                        yield return account;
                }
            }
        }

        private void Cache()
        {
            AccountMgr.log.Info("Chaching accaunts.");
            this.m_lock = new ReaderWriterLockWrapper();
            this.m_lastResyncTime = new DateTime();
            this.Resync();
        }

        private void Purge()
        {
            using (this.m_lock.EnterWriteLock())
            {
                this.m_cachedAccsById.Clear();
                this.m_cachedAccsByName.Clear();
            }
        }

        /// <summary>Purge and re-cache everything again</summary>
        public void ResetCache()
        {
            this.Purge();
            this.Cache();
        }

        internal void Remove(Account acc)
        {
            using (this.m_lock.EnterWriteLock())
                this.RemoveUnlocked(acc);
        }

        private void RemoveUnlocked(Account acc)
        {
            this.m_cachedAccsById.Remove((long) acc.AccountId);
            this.m_cachedAccsByName.Remove(acc.Name.ToString());
        }

        /// <summary>Reloads all account-data</summary>
        public void Resync()
        {
            this.m_lastResyncTime = DateTime.Now;
            Account[] accountArray = (Account[]) null;
            try
            {
                using (this.m_lock.EnterWriteLock())
                {
                    this.m_cachedAccsById.Clear();
                    this.m_cachedAccsByName.Clear();
                    accountArray = ActiveRecordBase<Account>.FindAll();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (accountArray != null)
                {
                    List<Account> accountList = new List<Account>(5);
                    foreach (Account account in this.m_cachedAccsById.Values)
                    {
                        if (!((IEnumerable<Account>) accountArray).Contains<Account>(account))
                            accountList.Add(account);
                    }

                    foreach (Account acc in accountList)
                        this.RemoveUnlocked(acc);
                    foreach (Account acc in accountArray)
                        this.Update(acc);
                }
            }

            AccountMgr.log.Info(string.Format("{0} accaunts chached."),
                accountArray != null ? ((IEnumerable<Account>) accountArray).Count<Account>() : 0);
            Action accountsResync = this.AccountsResync;
            if (accountsResync == null)
                return;
            accountsResync();
        }

        private void Update(Account acc)
        {
            Account account;
            if (!this.m_cachedAccsById.TryGetValue((long) acc.AccountId, out account))
            {
                this.m_cachedAccsById[(long) acc.AccountId] = acc;
                this.m_cachedAccsByName[acc.Name] = acc;
            }
            else
                account.Update((IAccount) acc);
        }

        /// <summary>
        /// Creates a game account.
        /// Make sure that the Account-name does not exist before calling this method.
        /// </summary>
        /// <param name="username">the username of the account</param>
        /// <param name="passHash">the hashed password of the account</param>
        public Account CreateAccount(string username, string password, string email, string privLevel)
        {
            try
            {
                Account acc = new Account(username, password, email)
                {
                    RoleGroupName = privLevel,
                    Created = DateTime.Now,
                    IsActive = true
                };
                try
                {
                    acc.CreateAndFlush();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (this.IsCached)
                {
                    using (this.m_lock.EnterWriteLock())
                        this.Update(acc);
                }

                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acc.AccountId)
                    .AddAttribute("operation", 1.0, "registering").AddAttribute("name", 0.0, username)
                    .AddAttribute("role", 0.0, privLevel).Write();
                AccountMgr.log.Info(string.Format("Autocreating acc {0} - {1}", (object) username,
                    (object) acc.RoleGroupName));
                return acc;
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, string.Format("Failed autocreate acc {0}.", (object) username),
                    new object[0]);
            }

            return (Account) null;
        }

        /// <summary>Checks to see if an account already exists.</summary>
        /// <returns>true if the account exists; false otherwise</returns>
        public static bool DoesAccountExist(string accName)
        {
            AccountMgr instance = AccountMgr.Instance;
            if (instance.IsCached)
            {
                using (instance.m_lock.EnterReadLock())
                    return instance.m_cachedAccsByName.ContainsKey(accName);
            }
            else
                return ActiveRecordBase<Account>.Exists(new ICriterion[1]
                {
                    (ICriterion) Restrictions.InsensitiveLike("Name", accName, MatchMode.Exact)
                });
        }

        public static Account GetAccount(string accountName)
        {
            return AccountMgr.Instance[accountName];
        }

        public static Account GetAccount(long uid)
        {
            return AccountMgr.Instance[uid];
        }

        public Account this[string accountName]
        {
            get
            {
                if (this.IsCached)
                {
                    using (this.m_lock.EnterReadLock())
                    {
                        Account account;
                        this.m_cachedAccsByName.TryGetValue(accountName, out account);
                        return account;
                    }
                }
                else
                    return ActiveRecordBase<Account>.FindOne(new ICriterion[1]
                    {
                        (ICriterion) Restrictions.Eq("Name", (object) accountName)
                    });
            }
        }

        public Account this[long id]
        {
            get
            {
                if (this.IsCached)
                {
                    using (this.m_lock.EnterReadLock())
                    {
                        Account account;
                        this.m_cachedAccsById.TryGetValue(id, out account);
                        return account;
                    }
                }
                else
                {
                    try
                    {
                        return ActiveRecordBase<Account>.FindOne(new ICriterion[1]
                        {
                            (ICriterion) Restrictions.Eq("AccountId", (object) (int) id)
                        });
                    }
                    catch (Exception ex)
                    {
                        RealmDBMgr.OnDBError(ex);
                        return ActiveRecordBase<Account>.FindOne(new ICriterion[1]
                        {
                            (ICriterion) Restrictions.Eq("AccountId", (object) (int) id)
                        });
                    }
                }
            }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize Accounts")]
        public static bool Initialize()
        {
            return AccountMgr.Instance.Start();
        }

        protected bool Start()
        {
            try
            {
                this.IsCached = RealmServerConfiguration.CacheAccounts;
                if (this.Count == 0)
                    AccountMgr.log.Info("Detected empty Account-database.");
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
            }

            return true;
        }

        protected bool Stop()
        {
            this._accountsReloadTimer.Stop();
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.UnregisterUpdatable(
                (IUpdatable) this._accountsReloadTimer);
            return true;
        }

        /// <summary>
        /// Validates the name against the stored Regex <see cref="F:WCell.RealmServer.Auth.Accounts.AccountMgr.DefaultNameValidationRegex" />
        /// </summary>
        /// <param name="name">The name to be validated</param>
        /// <returns>A Boolean value true is the name is valid; otherwise false</returns>
        public static bool ValidateNameDefault(ref string name)
        {
            name = name.Trim().ToUpper();
            if (name.Length >= AccountMgr.MinAccountNameLen && name.Length <= AccountMgr.MaxAccountNameLen)
                return AccountMgr.DefaultNameValidationRegex.IsMatch(name);
            return false;
        }

        public delegate bool NameValidationHandler(ref string name);
    }
}