using System;
using System.Collections.Generic;
using System.Data;
using WCell.Util.Conversion;
using WCell.Util.Data;

namespace WCell.Util.DB
{
    /// <summary>Maps a table-column to a DataField</summary>
    public class SimpleDataColumn : BaseDataColumn
    {
        internal readonly List<IFlatDataFieldAccessor> FieldList = new List<IFlatDataFieldAccessor>();
        private object m_DefaultValue;
        private bool m_IsPrimaryKey;
        private IFieldReader m_reader;
        internal int m_index;

        public SimpleDataColumn(string column, object defaultValue)
            : base(column)
        {
            this.m_DefaultValue = defaultValue;
        }

        public SimpleDataColumn(string column, IFieldReader reader, int index)
            : base(column)
        {
            this.m_reader = reader;
            this.m_index = index;
        }

        public SimpleDataColumn(string column, IFieldReader reader)
            : base(column)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            this.m_reader = reader;
        }

        public object DefaultValue
        {
            get { return this.m_DefaultValue; }
            set { this.m_DefaultValue = value; }
        }

        /// <summary>The index of this column within the query-result</summary>
        public int Index
        {
            get { return this.m_index; }
        }

        public IFieldReader Reader
        {
            get { return this.m_reader; }
            internal set { this.m_reader = value; }
        }

        /// <summary>
        /// An empty DataColumn has no reader and thus is not necessarily mapped.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.m_reader == null; }
        }

        public IFlatDataFieldAccessor[] Fields
        {
            get { return this.FieldList.ToArray(); }
        }

        public bool IsPrimaryKey
        {
            get { return this.m_IsPrimaryKey; }
            internal set { this.m_IsPrimaryKey = value; }
        }

        public void SetSingleValue(object value, IDataHolder holder)
        {
            for (int index = 0; index < this.FieldList.Count; ++index)
                this.FieldList[index].Set(holder, value);
        }

        public override string ToString()
        {
            return this.m_ColumnName + " (" + (object) this.m_index + ")" +
                   (this.m_IsPrimaryKey ? (object) " (PrimaryKey)" : (object) "");
        }

        public object ReadValue(IDataReader rs)
        {
            if (this.m_DefaultValue != null)
                return this.m_DefaultValue;
            return this.Reader.Read(rs, this.m_index);
        }
    }
}