using Castle.ActiveRecord;
using System;
using System.Net;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Auth.Firewall;
using WCell.RealmServer.Database;
using WCell.Util.Threading;

namespace WCell.AuthServer.Firewall
{
    /// <summary>Represents a Ban entry</summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class BanEntry : WCellRecord<BanEntry>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BanEntry), nameof(BanId), 1L);
        private string m_mask;
        private int[] m_MaskBytes;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static long NextId()
        {
            return BanEntry._idGenerator.Next();
        }

        public BanEntry(DateTime created, DateTime? expires, string banmask, string reason)
        {
            this.Created = created;
            this.Expires = expires;
            this.BanMask = banmask;
            this.Reason = reason;
            this.State = RecordState.New;
        }

        public BanEntry()
        {
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public long BanId { get; set; }

        [Property(NotNull = true)] public DateTime Created { get; set; }

        [Property] public DateTime? Expires { get; set; }

        [Property] public string Reason { get; set; }

        /// <summary>
        /// A mask matching IP-Addresses in the format:
        /// 123.45.*.1 (also matches 123.45.*.1.*.*)
        /// 41.3.*.23.*.243
        /// </summary>
        [Property(NotNull = true)]
        public string BanMask
        {
            get { return this.m_mask; }
            set
            {
                this.m_mask = value.Trim();
                this.m_MaskBytes = BanMgr.GetBytes(this.m_mask);
            }
        }

        public bool Matches(IPAddress addr)
        {
            return this.Matches(addr.GetAddressBytes());
        }

        public bool Matches(byte[] bytes)
        {
            if (!this.CheckValid())
                return false;
            for (int index = 0; index < this.m_MaskBytes.Length; ++index)
            {
                byte num = bytes[index];
                if (!BanMgr.Matches(this.m_MaskBytes[index], (int) num))
                    return false;
            }

            return false;
        }

        public bool Matches(int[] bytes)
        {
            if (!this.CheckValid())
                return false;
            return BanMgr.Match(this.m_MaskBytes, bytes);
        }

        public override string ToString()
        {
            return string.Format("{0} (Created: {1}, Banned {2}{3})", (object) this.BanMask, (object) this.Created,
                this.Expires.HasValue ? (object) ("until: " + (object) this.Expires) : (object) "indefinitely",
                string.IsNullOrEmpty(this.Reason) ? (object) "" : (object) (", Reason: " + this.Reason));
        }

        public bool CheckValid()
        {
            if (!this.Expires.HasValue || !(this.Expires.Value <= DateTime.Now))
                return true;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                (IMessage) new Message(new Action(((ActiveRecordBase) this).DeleteAndFlush)));
            return false;
        }

        public override void Delete()
        {
            BanMgr.Lock.EnterWriteLock();
            try
            {
                BanMgr.m_bans.Remove(this);
                base.Delete();
            }
            finally
            {
                BanMgr.Lock.ExitWriteLock();
            }
        }

        public override void DeleteAndFlush()
        {
            BanMgr.Lock.EnterWriteLock();
            try
            {
                BanMgr.m_bans.Remove(this);
                base.DeleteAndFlush();
            }
            finally
            {
                BanMgr.Lock.ExitWriteLock();
            }
        }
    }
}