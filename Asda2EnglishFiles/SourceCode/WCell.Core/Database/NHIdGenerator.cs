using Castle.ActiveRecord.Queries;
using System;
using System.Collections.Generic;
using System.Threading;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// Gives out next Primary Key for a table with assigned Primary Keys
    /// </summary>
    public class NHIdGenerator
    {
        private static readonly List<NHIdGenerator> _creators = new List<NHIdGenerator>();
        private static bool _DBInitialized;
        private static Action<Exception> OnError;
        private string m_table;
        private string m_idMember;
        private Type m_type;
        private long m_highestId;
        private long m_minId;

        public static void InitializeCreators(Action<Exception> onError)
        {
            NHIdGenerator.OnError = onError;
            foreach (NHIdGenerator creator in NHIdGenerator._creators)
                creator.Init();
            NHIdGenerator._DBInitialized = true;
        }

        public NHIdGenerator(Type type, string idMember, long minId = 1)
            : this(type, idMember, type.Name, minId)
        {
        }

        public NHIdGenerator(Type type, string idMember, string tableName, long minId = 1)
        {
            this.m_type = type;
            this.m_table = tableName;
            this.m_idMember = idMember;
            this.m_minId = minId;
            if (NHIdGenerator._DBInitialized)
                this.Init();
            else
                NHIdGenerator._creators.Add(this);
        }

        private void Init()
        {
            ScalarQuery<object> scalarQuery = new ScalarQuery<object>(this.m_type,
                string.Format("SELECT max(r.{0}) FROM {1} r", (object) this.m_idMember, (object) this.m_table));
            object obj;
            try
            {
                obj = scalarQuery.Execute();
            }
            catch (Exception ex)
            {
                NHIdGenerator.OnError(ex);
                obj = scalarQuery.Execute();
            }

            this.m_highestId = obj != null ? (long) Convert.ChangeType(obj, typeof(long)) : 0L;
            if (this.m_highestId >= this.m_minId)
                return;
            this.m_highestId = this.m_minId;
        }

        public long LastId
        {
            get { return Interlocked.Read(ref this.m_highestId); }
        }

        public long Next()
        {
            return Interlocked.Increment(ref this.m_highestId);
        }
    }
}