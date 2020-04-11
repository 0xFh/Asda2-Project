using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Util.Conversion;
using WCell.Util.Data;
using WCell.Util.DB.Xml;
using WCell.Util.Variables;

namespace WCell.Util.DB
{
  public class LightDBDefinitionSet
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    public readonly Dictionary<Type, DataHolderDefinition> DataHolderDefinitionMap =
      new Dictionary<Type, DataHolderDefinition>();

    public readonly Dictionary<string, TableDefinition> TableDefinitionMap =
      new Dictionary<string, TableDefinition>();

    /// <summary>
    /// DefaultTables are the tables that contain the core data of each DataHolder.
    /// It is ensured that a DataHolder is only valid if it exists in all its DefaultTables.
    /// </summary>
    public readonly Dictionary<string, TableDefinition[]> DefaultTables =
      new Dictionary<string, TableDefinition[]>();

    internal Dictionary<TableDefinition, List<DataHolderDefinition>> m_tableDataHolderMap =
      new Dictionary<TableDefinition, List<DataHolderDefinition>>();

    public readonly DataHolderDefinition[] DataHolderDefinitions;
    private DataHolderTableMapping[] m_mappings;
    private DefVersion m_DbVersionLocation;

    public LightDBDefinitionSet(DataHolderDefinition[] dataHolderDefinitions)
    {
      DataHolderDefinitions = dataHolderDefinitions;
      foreach(DataHolderDefinition holderDefinition in dataHolderDefinitions)
        DataHolderDefinitionMap.Add(holderDefinition.Type, holderDefinition);
    }

    public DataHolderTableMapping[] Mappings
    {
      get { return m_mappings; }
    }

    public DefVersion DBVersionLocation
    {
      get { return m_DbVersionLocation; }
    }

    public void Clear()
    {
      if(m_mappings == null)
        return;
      m_mappings = null;
      TableDefinitionMap.Clear();
      DefaultTables.Clear();
      m_tableDataHolderMap.Clear();
    }

    public DataHolderDefinition GetDefinition(Type t)
    {
      DataHolderDefinition holderDefinition;
      DataHolderDefinitionMap.TryGetValue(t, out holderDefinition);
      return holderDefinition;
    }

    public TableDefinition[] GetDefaultTables(DataHolderDefinition def)
    {
      TableDefinition[] tableDefinitionArray;
      DefaultTables.TryGetValue(def.Name, out tableDefinitionArray);
      return tableDefinitionArray;
    }

    public TableDefinition[] GetDefaultTables(string dataHolderName)
    {
      TableDefinition[] tableDefinitionArray;
      DefaultTables.TryGetValue(dataHolderName, out tableDefinitionArray);
      return tableDefinitionArray;
    }

    public TableDefinition GetTable(string tableName)
    {
      TableDefinition tableDefinition;
      TableDefinitionMap.TryGetValue(tableName, out tableDefinition);
      return tableDefinition;
    }

    public TableDefinition[] EnsureTables(string tableName, DataHolderDefinition def)
    {
      return EnsureTables(tableName, GetDefaultTables(def));
    }

    public TableDefinition[] EnsureTables(string tableName, TableDefinition[] defaultTables)
    {
      TableDefinition[] tableDefinitionArray;
      if(defaultTables.Length == 0)
        tableDefinitionArray = new TableDefinition[1]
        {
          EnsureTable(tableName)
        };
      else
        tableDefinitionArray = new TableDefinition[defaultTables.Length];
      for(int index = 0; index < defaultTables.Length; ++index)
        tableDefinitionArray[index] = EnsureTable(tableName, defaultTables[index]);
      return tableDefinitionArray;
    }

    public TableDefinition EnsureTable(string tableName, TableDefinition defaultTable)
    {
      return tableName == null ? defaultTable : EnsureTable(tableName);
    }

    private TableDefinition EnsureTable(string name)
    {
      TableDefinition tableDefinition;
      if(!TableDefinitionMap.TryGetValue(name, out tableDefinition))
        throw new DataHolderException(
          "Invalid DataHolder-definition refers to undefined Table \"{0}\" (use the <Table> node to define it in the Table.xml file): " +
          name, (object) name);
      return tableDefinition;
    }

    public void LoadTableDefinitions(string file)
    {
      BasicTableDefinitions tableDefinitions = XmlFile<BasicTableDefinitions>.Load(file);
      m_DbVersionLocation = tableDefinitions.DBVersion;
      foreach(BasicTableDefinition table in tableDefinitions.Tables)
      {
        if(table.Name == null)
          throw new ArgumentNullException("tableDef.Name",
            "Did you mis-type the Name attribute of the table with MainDataHolder = " +
            table.MainDataHolder + " ?");
        if(TableDefinitionMap.ContainsKey(table.Name))
          throw new DataHolderException("Duplicate Table definition \"{0}\" in File {1}", (object) table.Name,
            (object) file);
        Dictionary<string, ArrayConstraint> arrayConstraints = new Dictionary<string, ArrayConstraint>();
        if(table.ArrayConstraints != null)
        {
          foreach(ArrayConstraint arrayConstraint in table.ArrayConstraints)
            arrayConstraints.Add(arrayConstraint.Column, arrayConstraint);
        }

        PrimaryColumn[] primaryColumns = table.PrimaryColumns;
        if(primaryColumns == null || primaryColumns.Length == 0)
          throw new DataHolderException(
            "TableDefinition did not define any PrimaryColumns: " + table);
        TableDefinition tableDefinition = new TableDefinition(table.Name, primaryColumns, arrayConstraints,
          table.Variables ?? VariableDefinition.EmptyArray)
        {
          MainDataHolderName = table.MainDataHolder
        };
        TableDefinitionMap.Add(table.Name, tableDefinition);
      }
    }

    public void LoadDataHolderDefinitions(string dir)
    {
      LoadDataHolderDefinitions(new DirectoryInfo(dir));
    }

    /// <summary>
    /// Make sure to call <see cref="M:WCell.Util.DB.LightDBDefinitionSet.LoadTableDefinitions(System.String)" /> prior to this.
    /// </summary>
    /// <param name="dir"></param>
    public void LoadDataHolderDefinitions(DirectoryInfo dir)
    {
      Dictionary<string, List<SimpleDataColumn>> fieldMap = new Dictionary<string, List<SimpleDataColumn>>();
      foreach(LightRecordXmlConfig cfg in XmlFile<LightRecordXmlConfig>
        .LoadAll(dir))
        RegisterDefintion(cfg, fieldMap);
      FinishLoading(fieldMap);
    }

    private void FinishLoading(Dictionary<string, List<SimpleDataColumn>> fieldMap)
    {
      foreach(KeyValuePair<string, List<SimpleDataColumn>> field1 in fieldMap)
      {
        TableDefinition table = GetTable(field1.Key);
        SimpleDataColumn[] fields = field1.Value.ToArray();
        if(!string.IsNullOrEmpty(table.MainDataHolderName))
        {
          DataHolderDefinition dataDef = DataHolderDefinitions
            .Where(dataHolderDef =>
              dataHolderDef.Name.Equals(table.MainDataHolderName,
                StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
          if(dataDef == null)
            throw new DataHolderException("Table \"{0}\" refered to invalid MainDataHolder: {1}", (object) table,
              (object) table.MainDataHolderName);
          table.SetMainDataHolder(dataDef, false);
        }

        try
        {
          List<PrimaryColumn> primaryColumnList = new List<PrimaryColumn>();
          primaryColumnList.AddRange(table.PrimaryColumns);
          for(int index = 0; index < fields.Length; ++index)
          {
            SimpleDataColumn field = fields[index];
            PrimaryColumn primaryColumn = primaryColumnList
              .Where(
                primCol => primCol.Name == field.ColumnName)
              .FirstOrDefault();
            if(primaryColumn != null)
            {
              primaryColumn.DataColumn = field;
              field.IsPrimaryKey = true;
              primaryColumnList.Remove(primaryColumn);
            }

            if(field.ColumnName == null && field.DefaultValue == null)
              LightDBMgr.OnInvalidData(
                "Field-definition \"{0}\" did not define a Column nor a DefaultValue.", (object) field);
          }

          for(int index1 = 0; index1 < fields.Length; ++index1)
          {
            SimpleDataColumn simpleDataColumn1 = fields[index1];
            for(int index2 = 0; index2 < simpleDataColumn1.FieldList.Count; ++index2)
            {
              IFlatDataFieldAccessor field2 = simpleDataColumn1.FieldList[index2];
              if(field2.DataHolderDefinition.DependingField == field2)
              {
                SimpleDataColumn simpleDataColumn2 = fields[0];
                fields[0] = simpleDataColumn1;
                fields[index1] = simpleDataColumn2;
              }
            }
          }

          if(primaryColumnList.Count > 0 && table.MainDataHolder == null)
            throw new DataHolderException(
              "Table \"{0}\" referenced PrimaryColumn(s) ({1}) but did not define a MainDataHolder explicitely.",
              (object) table, (object) primaryColumnList.ToString(", "));
          if(primaryColumnList.Count > 0 || table.Variables != null && table.Variables.Length > 0)
          {
            int num1 = table.Variables != null ? table.Variables.Length : 0;
            int destinationIndex = num1 + primaryColumnList.Count;
            SimpleDataColumn[] simpleDataColumnArray =
              new SimpleDataColumn[fields.Length + destinationIndex];
            Array.Copy(fields, 0, simpleDataColumnArray, destinationIndex, fields.Length);
            fields = simpleDataColumnArray;
            if(num1 > 0)
              InitVars(table, fields);
            int num2 = num1;
            foreach(PrimaryColumn primaryColumn in primaryColumnList)
            {
              DataFieldProxy dataFieldProxy =
                new DataFieldProxy(primaryColumn.Name, table.MainDataHolder);
              IFieldReader reader = Converters.GetReader(primaryColumn.TypeName);
              if(reader == null)
                throw new DataHolderException(
                  "Invalid Type \"" + primaryColumn.TypeName + "\" for PrimaryColumn \"" +
                  primaryColumn.Name + "\" in definition for Table: " + table +
                  " - You must explicitely define the TypeName attribute inside the PrimaryColumn node, if it is only an additional table for a DataHolder!");
              SimpleDataColumn simpleDataColumn = new SimpleDataColumn(primaryColumn.Name, reader, 0)
              {
                IsPrimaryKey = true
              };
              simpleDataColumn.FieldList.Add(dataFieldProxy);
              fields[num2++] = primaryColumn.DataColumn = simpleDataColumn;
            }
          }

          int num = 0;
          for(int index = 0; index < fields.Length; ++index)
          {
            SimpleDataColumn simpleDataColumn = fields[index];
            if(simpleDataColumn.DefaultValue == null)
              simpleDataColumn.m_index = num++;
            else
              simpleDataColumn.Reader = null;
          }

          TableDefinitionMap[field1.Key].ColumnDefinitions = fields;
        }
        catch(Exception ex)
        {
          throw new DataHolderException(ex, "Unable to setup Table \"{0}\" (MainDataHolder: \"{1}\")", (object) table,
            (object) table.MainDataHolder);
        }
      }

      m_mappings =
        CreateDataHolderTableMappings(m_tableDataHolderMap,
          DataHolderDefinitions);
    }

    private static void InitVars(TableDefinition table, SimpleDataColumn[] fields)
    {
      int num = 0;
      foreach(VariableDefinition variable in table.Variables)
      {
        IDataField dataField;
        if(!table.MainDataHolder.Fields.TryGetValue(variable.Name, out dataField) ||
           !(dataField is IFlatDataFieldAccessor))
          throw new DataHolderException(
            "Table \"{0}\" defined invalid Variable {1}. Name does not refer to an actual property within DataHolder {2}.",
            (object) table, (object) variable, (object) table.MainDataHolder);
        try
        {
          object defaultValue = variable.Eval(dataField.MappedMember.GetVariableType());
          fields[num++] = new SimpleDataColumn(variable.Name, defaultValue)
          {
            FieldList =
            {
              (IFlatDataFieldAccessor) dataField
            }
          };
        }
        catch(Exception ex)
        {
          throw new DataHolderException(ex,
            "Unable to parse default-value \"{0}\" to Type \"{1}\" from Variable \"{2}\"",
            (object) variable.StringValue, (object) dataField.MappedMember.Name, (object) variable.Name);
        }
      }
    }

    public static DataHolderTableMapping[] CreateDataHolderTableMappings(
      Dictionary<TableDefinition, List<DataHolderDefinition>> tableDataHolderMap,
      DataHolderDefinition[] dataHolderDefinitions)
    {
      Dictionary<DataHolderDefinition, List<TableDefinition>> dictionary =
        new Dictionary<DataHolderDefinition, List<TableDefinition>>();
      foreach(KeyValuePair<TableDefinition, List<DataHolderDefinition>> tableDataHolder in tableDataHolderMap)
      {
        if(tableDataHolder.Value == null)
        {
          log.Warn(
            "Table-definition \"{0}\" has no used columns (and can possibly be removed from the config).");
        }
        else
        {
          tableDataHolder.Key.DataHolderDefinitions = tableDataHolder.Value.ToArray();
          foreach(DataHolderDefinition holderDefinition in tableDataHolder.Key.DataHolderDefinitions)
            dictionary.GetOrCreate(holderDefinition)
              .Add(tableDataHolder.Key);
        }
      }

      List<DataHolderTableMapping> holderTableMappingList = new List<DataHolderTableMapping>();
      HashSet<DataHolderDefinition> holderDefinitionSet1 =
        new HashSet<DataHolderDefinition>(
          dictionary.Keys.ToArray());
      HashSet<DataHolderDefinition> holderDefinitionSet2 = new HashSet<DataHolderDefinition>();
      HashSet<TableDefinition> tableDefinitionSet = new HashSet<TableDefinition>();
      foreach(DataHolderDefinition key in dictionary.Keys)
      {
        if(AddTables(holderDefinitionSet1, key,
          dictionary, holderDefinitionSet2, tableDefinitionSet))
        {
          DataHolderTableMapping holderTableMapping = new DataHolderTableMapping(
            holderDefinitionSet2.ToArray(),
            tableDefinitionSet.ToArray());
          holderTableMappingList.Add(holderTableMapping);
          holderDefinitionSet2.Clear();
          tableDefinitionSet.Clear();
        }
      }

      foreach(TableDefinition key in tableDataHolderMap.Keys)
      {
        TableDefinition table = key;
        foreach(SimpleDataColumn columnDefinition in table.ColumnDefinitions)
        {
          if(columnDefinition is IDataFieldBase)
          {
            DataHolderDefinition holderDefinition =
              ((IDataFieldBase) columnDefinition).DataHolderDefinition;
            if(!holderDefinitionSet2.Contains(holderDefinition))
            {
              DataHolderTableMapping holderTableMapping =
                holderTableMappingList.Find(map =>
                  map.TableDefinitions.Contains(
                    table));
              DataHolderDefinition[] holderDefinitions = holderTableMapping.DataHolderDefinitions;
              holderTableMapping.DataHolderDefinitions =
                new DataHolderDefinition[holderDefinitions.Length + 1];
              Array.Copy(holderDefinitions, holderTableMapping.TableDefinitions,
                holderDefinitions.Length);
              holderTableMapping.DataHolderDefinitions[holderDefinitions.Length] = holderDefinition;
            }
          }
        }
      }

      return holderTableMappingList.ToArray();
    }

    private static bool AddTables(ICollection<DataHolderDefinition> allDefs, DataHolderDefinition def,
      Dictionary<DataHolderDefinition, List<TableDefinition>> dataHolderToTable,
      HashSet<DataHolderDefinition> dataHolders, HashSet<TableDefinition> tables)
    {
      if(!allDefs.Contains(def))
        return false;
      allDefs.Remove(def);
      dataHolders.Add(def);
      foreach(TableDefinition tableDefinition in dataHolderToTable[def])
      {
        tables.Add(tableDefinition);
        foreach(DataHolderDefinition holderDefinition in tableDefinition.DataHolderDefinitions)
        {
          dataHolders.Add(holderDefinition);
          AddTables(allDefs, holderDefinition, dataHolderToTable, dataHolders, tables);
        }
      }

      return true;
    }

    private void RegisterDefintion(LightRecordXmlConfig cfg, Dictionary<string, List<SimpleDataColumn>> fieldMap)
    {
      DataHolderDefinition[] holderDefinitions = DataHolderDefinitions;
      XmlDataHolderDefinition holderDefinition1 = null;
      foreach(XmlDataHolderDefinition holderDefinition2 in cfg)
      {
        XmlDataHolderDefinition dataRawDef = holderDefinition2;
        try
        {
          if(dataRawDef.Name == null)
            throw new DataHolderException("Invalid DataHolder-definition has no name ({0}).", holderDefinition1 == null
              ? (object) "First in file"
              : (object) ("After: " + (object) holderDefinition1));
          dataRawDef.EnsureFieldsNotNull();
          DataHolderDefinition dataDef = holderDefinitions
            .Where(def =>
              def.Name.Equals(dataRawDef.Name, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();
          if(dataDef == null)
          {
            LightDBMgr.OnInvalidData("Invalid DataHolder-definition refers to non-existing DataHolder: " +
                                     dataRawDef.Name);
          }
          else
          {
            TableDefinition[] tableDefinitionArray;
            TableDefinition[] array;
            if(DefaultTables.TryGetValue(dataDef.Name, out tableDefinitionArray))
            {
              array = tableDefinitionArray;
            }
            else
            {
              array = null;
              tableDefinitionArray = dataRawDef.DefaultTables == null
                ? new TableDefinition[0]
                : new TableDefinition[dataRawDef.DefaultTables.Length];
            }

            for(int index = 0; index < tableDefinitionArray.Length; ++index)
            {
              string key = dataRawDef.DefaultTables[index].Trim();
              TableDefinition tableDefinition;
              if(!TableDefinitionMap.TryGetValue(key, out tableDefinition))
                throw new DataHolderException(
                  "DefaultTable \"{0}\" of DataHolder \"{1}\" is not defined - Make sure to define the table in the Table collection.",
                  (object) key, (object) dataRawDef);
              tableDefinitionArray[index] = tableDefinition;
              tableDefinition.SetMainDataHolder(dataDef, true);
            }

            DefaultTables[dataDef.Name] = tableDefinitionArray;
            if(dataRawDef.DataHolderName.Contains("Trainer"))
              ToString();
            AddFieldMappings(dataRawDef.Fields,
              dataDef.Fields, fieldMap);
            if(array != null)
            {
              int length = array.Length;
              Array.Resize(ref array, array.Length + tableDefinitionArray.Length);
              Array.Copy(tableDefinitionArray, 0, array, length,
                tableDefinitionArray.Length);
              DefaultTables[dataDef.Name] = array;
            }

            holderDefinition1 = dataRawDef;
          }
        }
        catch(Exception ex)
        {
          throw new DataHolderException(ex,
            "Failed to parse DataHolder-definition \"" + dataRawDef + "\" from {0}", (object) cfg.FileName);
        }
      }
    }

    internal void AddFieldMappings(IEnumerable<IDataFieldDefinition> fieldDefs,
      IDictionary<string, IDataField> dataFields, Dictionary<string, List<SimpleDataColumn>> mappedFields)
    {
      foreach(IDataField dataField in dataFields.Values)
        AddFieldMapping(fieldDefs, dataField, mappedFields);
    }

    internal void AddFieldMapping(IEnumerable<IDataFieldDefinition> fieldDefs, IDataField dataField,
      Dictionary<string, List<SimpleDataColumn>> mappedFields)
    {
      IDataFieldDefinition fieldDef = fieldDefs
        .Where(def =>
          def.Name.Equals(dataField.Name, StringComparison.InvariantCultureIgnoreCase))
        .FirstOrDefault();
      if(fieldDef == null)
      {
        string name = dataField.MappedMember.DeclaringType.Name;
        LightDBMgr.OnInvalidData(
          "DataField \"" + dataField + "\" in Type " + name + " (DataHolder: {0}) is not mapped.",
          (object) dataField.DataHolderDefinition);
      }
      else
      {
        if(fieldDef.DataFieldType != dataField.DataFieldType)
        {
          string fullName = dataField.MappedMember.DeclaringType.FullName;
          throw new DataHolderException(
            "DataField \"" + dataField + "\" in Type " + fullName +
            " is {0}, but was defined as: {1}", (object) dataField.DataFieldType, (object) fieldDef.DataFieldType);
        }

        LightDBMgr.DataFieldHandlers[(int) dataField.DataFieldType](this, dataField, fieldDef, mappedFields);
      }
    }
  }
}