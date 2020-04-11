using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using WCell.AuthServer.Firewall;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Auth.Firewall
{
    /// <summary>TODO: Use some kind of string-tree to improve lookup</summary>
    public static class BanMgr
    {
        public static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public static readonly int[] LocalHostBytes = new int[6]
        {
            (int) sbyte.MaxValue,
            0,
            0,
            1,
            0,
            0
        };

        internal static List<BanEntry> m_bans;

        [WCell.Core.Initialization.Initialization(InitializationPass.Fourth, "Caching Ban list...")]
        public static void InitBanMgr()
        {
            try
            {
                BanMgr.m_bans = ((IEnumerable<BanEntry>) ActiveRecordBase<BanEntry>.FindAll()).ToList<BanEntry>();
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                BanMgr.m_bans = ((IEnumerable<BanEntry>) ActiveRecordBase<BanEntry>.FindAll()).ToList<BanEntry>();
            }
        }

        public static List<BanEntry> AllBans
        {
            get
            {
                BanMgr.Lock.EnterReadLock();
                try
                {
                    List<BanEntry> banEntryList = new List<BanEntry>(BanMgr.m_bans.Count);
                    foreach (BanEntry ban in BanMgr.m_bans)
                    {
                        if (ban.CheckValid())
                            banEntryList.Add(ban);
                    }

                    return banEntryList;
                }
                finally
                {
                    BanMgr.Lock.ExitReadLock();
                }
            }
        }

        public static bool IsBanned(long ip)
        {
            return BanMgr.IsBanned(new IPAddress(ip));
        }

        public static bool IsBanned(IPAddress ip)
        {
            BanMgr.Lock.EnterReadLock();
            try
            {
                byte[] addressBytes = ip.GetAddressBytes();
                for (int index = 0; index < BanMgr.m_bans.Count; ++index)
                {
                    if (BanMgr.m_bans[index].Matches(addressBytes))
                        return true;
                }
            }
            finally
            {
                BanMgr.Lock.ExitReadLock();
            }

            return false;
        }

        /// <summary>
        /// Returns the first Ban that matches the given IP or null if none matches
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static BanEntry GetBan(IPAddress ip)
        {
            byte[] addressBytes = ip.GetAddressBytes();
            BanMgr.Lock.EnterReadLock();
            try
            {
                for (int index = 0; index < BanMgr.m_bans.Count; ++index)
                {
                    BanEntry ban = BanMgr.m_bans[index];
                    if (ban.Matches(addressBytes))
                        return ban;
                }
            }
            finally
            {
                BanMgr.Lock.ExitReadLock();
            }

            return (BanEntry) null;
        }

        public static List<BanEntry> GetBanList(string mask)
        {
            BanMgr.Lock.EnterReadLock();
            try
            {
                List<BanEntry> banEntryList = new List<BanEntry>();
                int[] bytes = BanMgr.GetBytes(mask);
                for (int index = 0; index < BanMgr.m_bans.Count; ++index)
                {
                    BanEntry ban = BanMgr.m_bans[index];
                    if (ban.Matches(bytes))
                        banEntryList.Add(ban);
                }

                return banEntryList;
            }
            finally
            {
                BanMgr.Lock.ExitReadLock();
            }
        }

        public static BanEntry AddBan(DateTime? until, string mask, string reason)
        {
            BanMgr.Lock.EnterWriteLock();
            try
            {
                BanEntry banEntry = new BanEntry(DateTime.Now, until, mask, reason);
                banEntry.Save();
                BanMgr.m_bans.Add(banEntry);
                return banEntry;
            }
            finally
            {
                BanMgr.Lock.ExitWriteLock();
            }
        }

        public static BanEntry AddBan(TimeSpan? lastsFor, string mask, string reason)
        {
            BanMgr.Lock.EnterWriteLock();
            try
            {
                DateTime now1 = DateTime.Now;
                DateTime? expires;
                if (!lastsFor.HasValue)
                {
                    expires = new DateTime?();
                }
                else
                {
                    DateTime now2 = DateTime.Now;
                    TimeSpan? nullable = lastsFor;
                    expires = nullable.HasValue ? new DateTime?(now2 + nullable.GetValueOrDefault()) : new DateTime?();
                }

                string banmask = mask;
                string reason1 = reason;
                BanEntry banEntry = new BanEntry(now1, expires, banmask, reason1);
                banEntry.Save();
                BanMgr.m_bans.Add(banEntry);
                return banEntry;
            }
            finally
            {
                BanMgr.Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returnes whether the given bytes either match the Localhost address
        /// or only consist of wildcards or zeros.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool IsInvalid(int[] bytes)
        {
            if (BanMgr.Match(BanMgr.LocalHostBytes, bytes))
                return true;
            int num1 = -2;
            foreach (int num2 in bytes)
            {
                if (num2 < -1)
                    return true;
                if (num2 != -1 && num2 != 0 || num1 != -2 && num2 != num1)
                    return false;
                num1 = num2;
            }

            return true;
        }

        public static bool Match(int[] bytes, int[] matchBytes)
        {
            for (int index = 0; index < matchBytes.Length; ++index)
            {
                if (!BanMgr.Matches(bytes[index], matchBytes[index]))
                    return false;
            }

            return true;
        }

        public static bool Matches(int bte, int matchByte)
        {
            if (matchByte != -1)
                return bte == matchByte;
            return true;
        }

        public static int[] GetBytes(string mask)
        {
            int[] numArray = new int[6];
            string[] strArray = mask.Trim().Split('.');
            int index;
            for (index = 0; index < strArray.Length; ++index)
            {
                int result;
                numArray[index] = !int.TryParse(strArray[index], out result) ? -1 : result;
            }

            for (; index < numArray.Length; ++index)
                numArray[index] = -1;
            return numArray;
        }
    }
}