using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WCell.Util.Conversion;
using WCell.Util.Data;
using WCell.Util.DB.Xml;

namespace WCell.Util.DB
{
    /// <summary>
    /// Static container and utility class for Table- and DataHolder-mappings
    /// </summary>
    public static class LightDBMgr
    {
        public static readonly LightDBMgr.DataFieldHandler[] DataFieldHandlers = new LightDBMgr.DataFieldHandler[4];
        public static readonly LightDBMgr.DataFieldCreator[] DataFieldCreators = new LightDBMgr.DataFieldCreator[4];
        public static Action<string> InvalidDataHandler;

        static LightDBMgr()
        {
            LightDBMgr.DataFieldHandlers[0] = new LightDBMgr.DataFieldHandler(LightDBMgr.MapFlatSimple);
            LightDBMgr.DataFieldHandlers[2] = new LightDBMgr.DataFieldHandler(LightDBMgr.MapNestedSimple);
            LightDBMgr.DataFieldHandlers[1] = new LightDBMgr.DataFieldHandler(LightDBMgr.MapFlatArray);
            LightDBMgr.DataFieldHandlers[3] = new LightDBMgr.DataFieldHandler(LightDBMgr.MapNestedArray);
            LightDBMgr.DataFieldCreators[0] = new LightDBMgr.DataFieldCreator(LightDBMgr.CreateFlatSimple);
            LightDBMgr.DataFieldCreators[2] = new LightDBMgr.DataFieldCreator(LightDBMgr.CreatedNestedSimple);
            LightDBMgr.DataFieldCreators[1] = new LightDBMgr.DataFieldCreator(LightDBMgr.CreateFlatArray);
            LightDBMgr.DataFieldCreators[3] = new LightDBMgr.DataFieldCreator(LightDBMgr.CreateNestedArray);
        }

        public static void MapFlatSimple(this LightDBDefinitionSet defs, IDataField dataField,
            IDataFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields)
        {
            SimpleFlatFieldDefinition fieldDef1 = (SimpleFlatFieldDefinition) fieldDef;
            LightDBMgr.AddMapping(defs, defs.GetDefaultTables(dataField.DataHolderDefinition), fieldDef1, mappedFields,
                (IFlatDataFieldAccessor) dataField, dataField.MappedMember);
        }

        public static void MapNestedSimple(this LightDBDefinitionSet defs, IDataField dataField,
            IDataFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields)
        {
            INestedFieldDefinition field = (INestedFieldDefinition) fieldDef;
            field.EnsureFieldsNotNull(dataField.DataHolderDefinition.Name);
            defs.AddFieldMappings((IEnumerable<IDataFieldDefinition>) field.Fields,
                (IDictionary<string, IDataField>) ((NestedDataField) dataField).InnerFields, mappedFields);
        }

        public static void MapFlatArray(this LightDBDefinitionSet defs, IDataField dataField,
            IDataFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields)
        {
            FlatArrayFieldDefinition arrayFieldDefinition = (FlatArrayFieldDefinition) fieldDef;
            FlatArrayDataField flatArrayDataField = (FlatArrayDataField) dataField;
            TableDefinition[] defaultTables =
                defs.EnsureTables(arrayFieldDefinition.Table, dataField.DataHolderDefinition);
            SimpleFlatFieldDefinition[] columns = arrayFieldDefinition.GetColumns(flatArrayDataField.Length);
            IDataFieldAccessor[] arrayAccessors = flatArrayDataField.ArrayAccessors;
            for (int index = 0; index < columns.Length; ++index)
            {
                SimpleFlatFieldDefinition fieldDef1 = columns[index];
                LightDBMgr.AddMapping(defs, defaultTables, fieldDef1, mappedFields,
                    (IFlatDataFieldAccessor) arrayAccessors[index], dataField.MappedMember);
            }
        }

        public static void MapNestedArray(this LightDBDefinitionSet defs, IDataField dataField,
            IDataFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields)
        {
            NestedArrayFieldDefinition arrayFieldDefinition = (NestedArrayFieldDefinition) fieldDef;
            NestedArrayDataField nestedArrayDataField = (NestedArrayDataField) dataField;
            TableDefinition[] defaultTables =
                defs.EnsureTables(arrayFieldDefinition.Table, dataField.DataHolderDefinition);
            foreach (FlatArrayFieldDefinition segment in arrayFieldDefinition.Segments)
            {
                SimpleFlatFieldDefinition[] columns = segment.GetColumns(nestedArrayDataField.Length);
                for (int index = 0; index < columns.Length; ++index)
                {
                    SimpleFlatFieldDefinition fieldDef1 = columns[index];
                    IDataField dataField1;
                    if (!((NestedArrayAccessor) nestedArrayDataField.ArrayAccessors[index]).InnerFields.TryGetValue(
                        segment.Name, out dataField1))
                        throw new DataHolderException("NestedArray definition {0} refered to non-existing field {1}",
                            new object[2]
                            {
                                (object) nestedArrayDataField,
                                (object) segment
                            });
                    LightDBMgr.AddMapping(defs, defaultTables, fieldDef1, mappedFields,
                        (IFlatDataFieldAccessor) dataField1, dataField1.MappedMember);
                }
            }
        }

        private static void AddMapping(LightDBDefinitionSet defs, TableDefinition[] defaultTables,
            SimpleFlatFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields,
            IFlatDataFieldAccessor accessor, MemberInfo member)
        {
            string column = fieldDef.Column;
            TableDefinition[] tableDefinitionArray = defs.EnsureTables(fieldDef.Table, defaultTables);
            object defaultValue;
            if (!string.IsNullOrEmpty(fieldDef.DefaultStringValue))
            {
                defaultValue = StringParser.Parse(fieldDef.DefaultStringValue, member.GetVariableType());
            }
            else
            {
                if (string.IsNullOrEmpty(column))
                    return;
                defaultValue = (object) null;
            }

            foreach (TableDefinition key in tableDefinitionArray)
            {
                List<DataHolderDefinition> holderDefinitionList =
                    defs.m_tableDataHolderMap.GetOrCreate<TableDefinition, DataHolderDefinition>(key);
                if (!holderDefinitionList.Contains(accessor.DataHolderDefinition))
                    holderDefinitionList.Add(accessor.DataHolderDefinition);
                List<SimpleDataColumn> simpleDataColumnList =
                    mappedFields.GetOrCreate<string, SimpleDataColumn>(key.Name);
                PersistentAttribute persistentAttribute =
                    ((IEnumerable<DBAttribute>) member.GetCustomAttributes<DBAttribute>())
                    .Where<DBAttribute>((Func<DBAttribute, bool>) (attribute => attribute is PersistentAttribute))
                    .FirstOrDefault<DBAttribute>() as PersistentAttribute;
                SimpleDataColumn simpleDataColumn;
                if (string.IsNullOrEmpty(column))
                {
                    simpleDataColumnList.Add(simpleDataColumn = new SimpleDataColumn(fieldDef.Name, defaultValue));
                }
                else
                {
                    simpleDataColumn =
                        simpleDataColumnList.Find(
                            (Predicate<SimpleDataColumn>) (cmpField => cmpField.ColumnName == column));
                    if (simpleDataColumn == null)
                    {
                        Type type1 = member.GetActualType();
                        if (persistentAttribute != null)
                        {
                            Type type2 = persistentAttribute.ReadType;
                            if ((object) type2 == null)
                                type2 = type1;
                            type1 = type2;
                        }

                        IFieldReader reader = Converters.GetReader(type1);
                        simpleDataColumnList.Add(simpleDataColumn = new SimpleDataColumn(column, reader));
                    }
                }

                simpleDataColumn.FieldList.Add(accessor);
            }
        }

        public static void EnsureFieldsNotNull(this IHasDataFieldDefinitions container)
        {
            if (container.Fields == null)
                throw new DataHolderException(container.GetType().Name + " \"{0}\" did not define any fields.",
                    new object[1]
                    {
                        (object) container
                    });
        }

        public static void EnsureFieldsNotNull(this IHasDataFieldDefinitions field, string dataHolderName)
        {
            if (field.Fields == null)
                throw new DataHolderException("Field \"{0}\" of DataHolder \"{1}\" did not define any fields.",
                    new object[2]
                    {
                        (object) field,
                        (object) dataHolderName
                    });
        }

        private static DataFieldDefinition CreateNestedArray(IDataField dataField)
        {
            Dictionary<string, IDataField> innerFields =
                ((NestedArrayAccessor[]) ((ArrayDataField) dataField).ArrayAccessors)[0].InnerFields;
            FlatArrayFieldDefinition[] arrayFieldDefinitionArray = new FlatArrayFieldDefinition[innerFields.Count];
            int num = 0;
            foreach (IDataField dataField1 in innerFields.Values)
            {
                FlatArrayFieldDefinition arrayFieldDefinition1 = new FlatArrayFieldDefinition();
                arrayFieldDefinition1.Name = dataField1.Name;
                arrayFieldDefinition1.Offset = 1;
                arrayFieldDefinition1.Pattern = "";
                FlatArrayFieldDefinition arrayFieldDefinition2 = arrayFieldDefinition1;
                arrayFieldDefinitionArray[num++] = arrayFieldDefinition2;
            }

            NestedArrayFieldDefinition arrayFieldDefinition = new NestedArrayFieldDefinition();
            arrayFieldDefinition.Name = dataField.Name;
            arrayFieldDefinition.Segments = arrayFieldDefinitionArray;
            return (DataFieldDefinition) arrayFieldDefinition;
        }

        private static DataFieldDefinition CreateFlatArray(IDataField dataField)
        {
            FlatArrayFieldDefinition arrayFieldDefinition = new FlatArrayFieldDefinition();
            arrayFieldDefinition.Name = dataField.Name;
            arrayFieldDefinition.Offset = 1;
            arrayFieldDefinition.Pattern = "";
            return (DataFieldDefinition) arrayFieldDefinition;
        }

        private static DataFieldDefinition CreatedNestedSimple(IDataField dataField)
        {
            NestedSimpleDataField nestedSimpleDataField = (NestedSimpleDataField) dataField;
            DataFieldDefinition[] dataFieldDefinitionArray =
                new DataFieldDefinition[nestedSimpleDataField.InnerFields.Count];
            int num = 0;
            foreach (IDataField field in nestedSimpleDataField.InnerFields.Values)
                dataFieldDefinitionArray[num++] = LightDBMgr.DataFieldCreators[(int) field.DataFieldType](field);
            NestedSimpleFieldDefinition simpleFieldDefinition = new NestedSimpleFieldDefinition();
            simpleFieldDefinition.Name = dataField.Name;
            simpleFieldDefinition.Fields = dataFieldDefinitionArray;
            return (DataFieldDefinition) simpleFieldDefinition;
        }

        private static DataFieldDefinition CreateFlatSimple(IDataField dataField)
        {
            SimpleFlatFieldDefinition flatFieldDefinition = new SimpleFlatFieldDefinition();
            flatFieldDefinition.Name = dataField.Name;
            flatFieldDefinition.Column = "";
            return (DataFieldDefinition) flatFieldDefinition;
        }

        public static void SaveAllStubs(string dir, IEnumerable<DataHolderDefinition> dataHolderDefs)
        {
            foreach (DataHolderDefinition dataHolderDef in dataHolderDefs)
                LightDBMgr.SaveDefinitionStub(Path.Combine(dir, dataHolderDef.Name + ".xml"), dataHolderDef);
        }

        public static void SaveDefinitionStub(string file, DataHolderDefinition dataHolderDef)
        {
            XmlDataHolderDefinition holderDefinition = new XmlDataHolderDefinition()
            {
                Name = dataHolderDef.Name,
                DefaultTables = new string[1] {" "},
                Fields = new DataFieldDefinition[dataHolderDef.Fields.Count]
            };
            int num = 0;
            foreach (IDataField field in dataHolderDef.Fields.Values)
                holderDefinition.Fields[num++] = LightDBMgr.DataFieldCreators[(int) field.DataFieldType](field);
            LightRecordXmlConfig lightRecordXmlConfig = new LightRecordXmlConfig();
            lightRecordXmlConfig.FileName = file;
            lightRecordXmlConfig.DataHolders = new XmlDataHolderDefinition[1]
            {
                holderDefinition
            };
            lightRecordXmlConfig.Save();
        }

        public static void OnInvalidData(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (LightDBMgr.InvalidDataHandler == null)
                throw new DataHolderException(msg, new object[0]);
            LightDBMgr.InvalidDataHandler(msg);
        }

        public delegate void DataFieldHandler(LightDBDefinitionSet defs, IDataField dataField,
            IDataFieldDefinition fieldDef, Dictionary<string, List<SimpleDataColumn>> mappedFields);

        public delegate DataFieldDefinition DataFieldCreator(IDataField field);
    }
}