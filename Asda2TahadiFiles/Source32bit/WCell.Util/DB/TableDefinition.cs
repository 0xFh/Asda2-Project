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
    public GetIdHandler GetId;

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
      m_Name = name;
      m_primaryColumns = primaryColumns;
      ArrayConstraints = arrayConstraints;
      GetId = GetPrimaryId;
      m_singlePrimaryCol = m_primaryColumns.Count() == 1;
      Variables = variables;
    }

    /// <summary>
    /// Whether this is a DefaultTable of its <see cref="P:WCell.Util.DB.TableDefinition.MainDataHolder" />.
    /// DefaultTables are the tables that contain the core data of each DataHolder.
    /// It is ensured that a DataHolder is only valid if it exists in all its DefaultTables.
    /// </summary>
    public bool IsDefaultTable
    {
      get { return m_isDefaultTable; }
    }

    internal string MainDataHolderName { get; set; }

    /// <summary>
    /// The DataHolder to which this table primarily belongs.
    /// It is used for variables and undefined key-references.
    /// </summary>
    public DataHolderDefinition MainDataHolder
    {
      get { return m_mainDataHolder; }
    }

    public string[] AllColumns
    {
      get
      {
        if(m_allColumns == null)
        {
          List<string> stringList = new List<string>();
          for(int index = 0; index < m_ColumnDefinitions.Length; ++index)
          {
            if(!m_ColumnDefinitions[index].IsEmpty)
              stringList.Add(m_ColumnDefinitions[index].ColumnName);
          }

          m_allColumns = stringList.ToArray();
        }

        return m_allColumns;
      }
    }

    public string Name
    {
      get { return m_Name; }
      internal set { m_Name = value; }
    }

    public PrimaryColumn[] PrimaryColumns
    {
      get { return m_primaryColumns; }
    }

    public DataHolderDefinition[] DataHolderDefinitions
    {
      get { return m_dataHolderDefinitions; }
      internal set { m_dataHolderDefinitions = value; }
    }

    /// <summary>
    /// Set of columns and corresponding DataFields.
    /// Array must not be empty and the PrimaryKey must always be the first column.
    /// </summary>
    public SimpleDataColumn[] ColumnDefinitions
    {
      get { return m_ColumnDefinitions; }
      internal set
      {
        m_ColumnDefinitions = value;
        m_allColumns = null;
      }
    }

    public override string ToString()
    {
      return Name;
    }

    public object GetPrimaryId(IDataReader rs)
    {
      if(m_singlePrimaryCol)
        return m_primaryColumns[0].DataColumn.ReadValue(rs);
      int length = m_primaryColumns.Count();
      object[] objArray = new object[length];
      for(int index = 0; index < length; ++index)
      {
        SimpleDataColumn dataColumn = m_primaryColumns[index].DataColumn;
        objArray[index] = dataColumn.ReadValue(rs);
      }

      return objArray;
    }

    internal void SetDefaults(object id, IDataHolder holder)
    {
      if(id is object[])
      {
        object[] objArray = (object[]) id;
        for(int index = 0; index < PrimaryColumns.Length; ++index)
          PrimaryColumns[index].DataColumn.SetSingleValue(objArray[index], holder);
      }
      else
        PrimaryColumns[0].DataColumn.SetSingleValue(id, holder);
    }

    internal void SetMainDataHolder(DataHolderDefinition dataDef, bool isDefaultTable)
    {
      if(MainDataHolder != null)
        return;
      m_mainDataHolder = dataDef;
      m_isDefaultTable = isDefaultTable;
    }

    public delegate object GetIdHandler(IDataReader reader);
  }
}