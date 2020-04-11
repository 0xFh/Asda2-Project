using System.Collections.Generic;
using System.Data;
using System.Linq;
using WCell.Util.Data;
using WCell.Util.DB.Xml;
using WCell.Util.Variables;

namespace WCell.Util.DB
{
    /// <summary>
    /// A table definition has an array of Columns and an array of DataFields whose indices
    /// correspond to each other.
    /// </summary>
    public class TableDefinition
    {
        public readonly Dictionary<string, ArrayConstraint> ArrayConstraints;
        private string m_Name;
        private string[] m_allColumns;
        private SimpleDataColumn[] m_ColumnDefinitions;
        private DataHolderDefinition m_mainDataHolder;
        private DataHolderDefinition[] m_dataHolderDefinitions;
        private PrimaryColumn[] m_primaryColumns;
        private bool m_singlePrimaryCol;
        private bool m_isDefaultTable;

        /// <summary>
        /// The handler that returns the Id (or compound Id) for each row, read from the DB.
        /// </summary>
        public TableDefinition.GetIdHandler GetId;

        public VariableDefinition[] Variables;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryColumns"></param>
        /// <param name="arrayConstraints"></param>
        public TableDefinition(string name, PrimaryColumn[] primaryColumns,
            Dictionary<string, ArrayConstraint> arrayConstraints, VariableDefinition[] variables)
        {
            this.m_Name = name;
            this.m_primaryColumns = primaryColumns;
            this.ArrayConstraints = arrayConstraints;
            this.GetId = new TableDefinition.GetIdHandler(this.GetPrimaryId);
            this.m_singlePrimaryCol = ((IEnumerable<PrimaryColumn>) this.m_primaryColumns).Count<PrimaryColumn>() == 1;
            this.Variables = variables;
        }

        /// <summary>
        /// Whether this is a DefaultTable of its <see cref="P:WCell.Util.DB.TableDefinition.MainDataHolder" />.
        /// DefaultTables are the tables that contain the core data of each DataHolder.
        /// It is ensured that a DataHolder is only valid if it exists in all its DefaultTables.
        /// </summary>
        public bool IsDefaultTable
        {
            get { return this.m_isDefaultTable; }
        }

        internal string MainDataHolderName { get; set; }

        /// <summary>
        /// The DataHolder to which this table primarily belongs.
        /// It is used for variables and undefined key-references.
        /// </summary>
        public DataHolderDefinition MainDataHolder
        {
            get { return this.m_mainDataHolder; }
        }

        public string[] AllColumns
        {
            get
            {
                if (this.m_allColumns == null)
                {
                    List<string> stringList = new List<string>();
                    for (int index = 0; index < this.m_ColumnDefinitions.Length; ++index)
                    {
                        if (!this.m_ColumnDefinitions[index].IsEmpty)
                            stringList.Add(this.m_ColumnDefinitions[index].ColumnName);
                    }

                    this.m_allColumns = stringList.ToArray();
                }

                return this.m_allColumns;
            }
        }

        public string Name
        {
            get { return this.m_Name; }
            internal set { this.m_Name = value; }
        }

        public PrimaryColumn[] PrimaryColumns
        {
            get { return this.m_primaryColumns; }
        }

        public DataHolderDefinition[] DataHolderDefinitions
        {
            get { return this.m_dataHolderDefinitions; }
            internal set { this.m_dataHolderDefinitions = value; }
        }

        /// <summary>
        /// Set of columns and corresponding DataFields.
        /// Array must not be empty and the PrimaryKey must always be the first column.
        /// </summary>
        public SimpleDataColumn[] ColumnDefinitions
        {
            get { return this.m_ColumnDefinitions; }
            internal set
            {
                this.m_ColumnDefinitions = value;
                this.m_allColumns = (string[]) null;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public object GetPrimaryId(IDataReader rs)
        {
            if (this.m_singlePrimaryCol)
                return this.m_primaryColumns[0].DataColumn.ReadValue(rs);
            int length = ((IEnumerable<PrimaryColumn>) this.m_primaryColumns).Count<PrimaryColumn>();
            object[] objArray = new object[length];
            for (int index = 0; index < length; ++index)
            {
                SimpleDataColumn dataColumn = this.m_primaryColumns[index].DataColumn;
                objArray[index] = dataColumn.ReadValue(rs);
            }

            return (object) objArray;
        }

        internal void SetDefaults(object id, IDataHolder holder)
        {
            if (id is object[])
            {
                object[] objArray = (object[]) id;
                for (int index = 0; index < this.PrimaryColumns.Length; ++index)
                    this.PrimaryColumns[index].DataColumn.SetSingleValue(objArray[index], holder);
            }
            else
                this.PrimaryColumns[0].DataColumn.SetSingleValue(id, holder);
        }

        internal void SetMainDataHolder(DataHolderDefinition dataDef, bool isDefaultTable)
        {
            if (this.MainDataHolder != null)
                return;
            this.m_mainDataHolder = dataDef;
            this.m_isDefaultTable = isDefaultTable;
        }

        public delegate object GetIdHandler(IDataReader reader);
    }
}