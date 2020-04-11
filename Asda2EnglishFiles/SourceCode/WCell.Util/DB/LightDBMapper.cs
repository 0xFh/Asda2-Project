using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WCell.Util.Data;
using WCell.Util.DB.Xml;

namespace WCell.Util.DB
{
    public class LightDBMapper
    {
        private readonly Dictionary<Type, Dictionary<object, IDataHolder>> dataHolderMap =
            new Dictionary<Type, Dictionary<object, IDataHolder>>();

        private readonly List<IDataHolder> m_toUpdate = new List<IDataHolder>();
        private readonly List<IDataHolder> m_toInsert = new List<IDataHolder>();
        private readonly List<IDataHolder> m_toDelete = new List<IDataHolder>();
        public const int CacheVersion = 5;
        private DataHolderTableMapping m_mapping;
        private IDbWrapper m_db;
        private bool m_fetched;

        public LightDBMapper(DataHolderTableMapping mapping, IDbWrapper db)
        {
            this.m_mapping = mapping;
            this.m_db = db;
            db.Prepare(this);
        }

        public DataHolderTableMapping Mapping
        {
            get { return this.m_mapping; }
        }

        public IDbWrapper Wrapper
        {
            get { return this.m_db; }
        }

        /// <summary>Whether this Mapper has already fetched its contents</summary>
        public bool Fetched
        {
            get { return this.m_fetched; }
        }

        public Dictionary<object, IDataHolder> GetObjectMap<T>() where T : IDataHolder
        {
            return this.dataHolderMap.GetOrCreate<Type, object, IDataHolder>(typeof(T));
        }

        public void AddObject<T>(object id, T obj) where T : IDataHolder
        {
            this.dataHolderMap.GetOrCreate<Type, object, IDataHolder>(typeof(T)).Add(id, (IDataHolder) obj);
        }

        /// <summary>
        /// Adds an Array of DataHolders where the index (int)
        /// within the Array is the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void AddObjectsInt<T>(T[] objs) where T : class, IDataHolder
        {
            Dictionary<object, IDataHolder> dictionary =
                this.dataHolderMap.GetOrCreate<Type, object, IDataHolder>(typeof(T));
            for (int index = 0; index < objs.Length; ++index)
            {
                T obj = objs[index];
                if ((object) obj != null)
                    dictionary.Add((object) index, (IDataHolder) obj);
            }
        }

        /// <summary>
        /// Adds an Array of DataHolders where the index (uint)
        /// within the Array is the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void AddObjectsUInt<T>(T[] objs) where T : class, IDataHolder
        {
            Dictionary<object, IDataHolder> dictionary =
                this.dataHolderMap.GetOrCreate<Type, object, IDataHolder>(typeof(T));
            for (int index = 0; index < objs.Length; ++index)
            {
                T obj = objs[index];
                if ((object) obj != null)
                    dictionary.Add((object) (uint) index, (IDataHolder) obj);
            }
        }

        /// <summary>
        /// Fetches all sets of objects, defined through the given mapping.
        /// </summary>
        /// <returns></returns>
        public void Fetch()
        {
            List<Action> actionList = new List<Action>(10000);
            TableDefinition[] tableDefinitions = this.m_mapping.TableDefinitions;
            DataHolderDefinition holderDefinition1 = (DataHolderDefinition) null;
            for (int tableIndex = 0; tableIndex < tableDefinitions.Length; ++tableIndex)
            {
                TableDefinition def = tableDefinitions[tableIndex];
                try
                {
                    using (IDataReader reader = this.m_db.CreateReader(def, tableIndex))
                    {
                        SimpleDataColumn[] columnDefinitions = def.ColumnDefinitions;
                        while (reader.Read())
                        {
                            object key = def.GetId(reader);
                            DataHolderDefinition holderDefinition2 = (DataHolderDefinition) null;
                            IDataHolder dataHolder1 = (IDataHolder) null;
                            object firstValue = (object) null;
                            SimpleDataColumn simpleDataColumn = (SimpleDataColumn) null;
                            try
                            {
                                for (int index1 = 0; index1 < columnDefinitions.Length; ++index1)
                                {
                                    simpleDataColumn = columnDefinitions[index1];
                                    firstValue = (object) null;
                                    firstValue = simpleDataColumn.ReadValue(reader);
                                    for (int index2 = 0; index2 < simpleDataColumn.FieldList.Count; ++index2)
                                    {
                                        IFlatDataFieldAccessor field = simpleDataColumn.FieldList[index2];
                                        holderDefinition1 = field.DataHolderDefinition;
                                        if (holderDefinition1 == null)
                                            throw new DataHolderException(
                                                "Invalid DataField did not have a DataHolderDefinition: " +
                                                (object) field, new object[0]);
                                        IDataHolder dataHolder2;
                                        if (holderDefinition2 != field.DataHolderDefinition || dataHolder1 == null)
                                        {
                                            Dictionary<object, IDataHolder> dictionary =
                                                this.dataHolderMap.GetOrCreate<Type, object, IDataHolder>(
                                                    holderDefinition1.Type);
                                            if (!dictionary.TryGetValue(key, out dataHolder2))
                                            {
                                                if (!def.IsDefaultTable)
                                                {
                                                    LightDBMgr.OnInvalidData(
                                                        "Invalid DataHolder was not defined in its Default-Tables - DataHolder: {0}; Id(s): {1}; Table: {2}",
                                                        (object) holderDefinition1, key, (object) def);
                                                    index1 = columnDefinitions.Length;
                                                    break;
                                                }

                                                dictionary.Add(key,
                                                    dataHolder2 =
                                                        (IDataHolder) holderDefinition1.CreateHolder(firstValue));
                                                actionList.Add(new Action(dataHolder2.FinalizeDataHolder));
                                            }

                                            dataHolder1 = dataHolder2;
                                            holderDefinition2 = holderDefinition1;
                                        }
                                        else
                                            dataHolder2 = dataHolder1;

                                        field.Set(dataHolder2, firstValue);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                object[] objArray1;
                                if (!(key is Array))
                                    objArray1 = new object[1] {key};
                                else
                                    objArray1 = (object[]) key;
                                object[] objArray2 = objArray1;
                                throw new DataHolderException(ex,
                                    "Unable to parse data-cell (Column: {0}, Id(s): {1}{2})", new object[3]
                                    {
                                        (object) simpleDataColumn,
                                        (object) ((IEnumerable<object>) objArray2).ToString<object>(", ",
                                            (Func<object, object>) (idObj =>
                                                idObj.GetType().IsEnum
                                                    ? Convert.ChangeType(idObj, Enum.GetUnderlyingType(idObj.GetType()))
                                                    : idObj)),
                                        firstValue != null ? (object) (", Value: " + firstValue) : (object) ""
                                    });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DataHolderException(ex, "Failed to read from Table \"{0}\" {1}", new object[2]
                    {
                        (object) def,
                        holderDefinition1 != null ? (object) ("DataHolder: " + (object) holderDefinition1) : (object) ""
                    });
                }
            }

            for (int index = 0; index < actionList.Count; ++index)
                actionList[index]();
            this.dataHolderMap.Clear();
            this.m_fetched = true;
        }

        public List<UpdateKeyValueList> GetUpdateList(IDataHolder obj)
        {
            return this.GetKeyValuePairs<UpdateKeyValueList>(obj,
                (Func<TableDefinition, UpdateKeyValueList>) (table =>
                    new UpdateKeyValueList(table, this.GetWherePairs(table, obj))));
        }

        public List<KeyValueListBase> GetInsertList(IDataHolder obj)
        {
            return this.GetKeyValuePairs<KeyValueListBase>(obj,
                (Func<TableDefinition, KeyValueListBase>) (table => new KeyValueListBase(table)));
        }

        private List<T> GetKeyValuePairs<T>(IDataHolder obj, Func<TableDefinition, T> listCreator)
            where T : KeyValueListBase
        {
            DataHolderDefinition holderDefinition = this.m_mapping.GetDataHolderDefinition(obj.GetType());
            List<T> source = new List<T>(this.m_mapping.TableDefinitions.Length);
            for (int index1 = 0; index1 < this.m_mapping.TableDefinitions.Length; ++index1)
            {
                TableDefinition table = this.m_mapping.TableDefinitions[index1];
                for (int index2 = 0; index2 < table.ColumnDefinitions.Length; ++index2)
                {
                    SimpleDataColumn columnDefinition = table.ColumnDefinitions[index2];
                    for (int index3 = 0; index3 < columnDefinition.FieldList.Count; ++index3)
                    {
                        IFlatDataFieldAccessor field = columnDefinition.FieldList[index3];
                        if (field.DataHolderDefinition == holderDefinition)
                        {
                            T obj1 = source.FirstOrDefault<T>((Func<T, bool>) (l => l.TableName == table.Name));
                            if ((object) obj1 == null)
                                source.Add(obj1 = listCreator(table));
                            object obj2 = field.Get(obj);
                            obj1.AddPair(columnDefinition.ColumnName, (object) obj2.ToString());
                        }
                    }
                }
            }

            return source;
        }

        /// <summary>Marks this Object to be updated upon next flush</summary>
        public void Update(IDataHolder obj)
        {
            lock (this.m_toUpdate)
                this.m_toUpdate.Add(obj);
        }

        /// <summary>Marks this Object to be inserted upon next flush</summary>
        public void Insert(IDataHolder obj)
        {
            lock (this.m_toInsert)
                this.m_toInsert.Add(obj);
        }

        /// <summary>Marks this Object to be deleted upon next flush</summary>
        public void Delete(IDataHolder obj)
        {
            lock (this.m_toDelete)
                this.m_toDelete.Add(obj);
        }

        /// <summary>Ignores all changes that have not been commited yet.</summary>
        public void IgnoreUnflushedChanges()
        {
            bool lockTaken1 = false;
            List<IDataHolder> dataHolderList = null;
            try
            {
                Monitor.Enter((object) (dataHolderList = this.m_toUpdate), ref lockTaken1);
                this.m_toUpdate.Clear();
            }
            finally
            {
                if (lockTaken1)
                    Monitor.Exit((object) dataHolderList);
            }

            bool lockTaken2 = false;
            try
            {
                Monitor.Enter((object) (dataHolderList = this.m_toInsert), ref lockTaken2);
                this.m_toInsert.Clear();
            }
            finally
            {
                if (lockTaken2)
                    Monitor.Exit((object) dataHolderList);
            }

            bool lockTaken3 = false;
            try
            {
                Monitor.Enter((object) (dataHolderList = this.m_toDelete), ref lockTaken3);
                this.m_toDelete.Clear();
            }
            finally
            {
                if (lockTaken3)
                    Monitor.Exit((object) dataHolderList);
            }
        }

        /// <summary>
        /// Commits all inserts and updates to the underlying Database
        /// </summary>
        public void Flush()
        {
            this.Flush(this.m_toUpdate,
                (Action<IDataHolder>) (obj =>
                    this.GetUpdateList(obj).ForEach((Action<UpdateKeyValueList>) (list => this.m_db.Update(list)))));
            this.Flush(this.m_toInsert,
                (Action<IDataHolder>) (obj =>
                    this.GetInsertList(obj).ForEach(new Action<KeyValueListBase>(this.m_db.Insert))));
            this.Flush(this.m_toDelete,
                (Action<IDataHolder>) (obj =>
                    this.GetWherePairs(obj).ForEach(new Action<KeyValueListBase>(this.m_db.Delete))));
        }

        private void Flush(List<IDataHolder> list, Action<IDataHolder> callback)
        {
            lock (list)
            {
                int count = 0;
                try
                {
                    for (; count < list.Count; ++count)
                    {
                        IDataHolder dataHolder = list[count];
                        callback(dataHolder);
                    }
                }
                finally
                {
                    if (count < list.Count)
                        list.RemoveRange(0, count);
                    else
                        list.Clear();
                }
            }
        }

        private List<KeyValueListBase> GetWherePairs(IDataHolder obj)
        {
            Type type = obj.GetType();
            List<KeyValueListBase> keyValueListBaseList = new List<KeyValueListBase>(3);
            for (int index = 0; index < this.m_mapping.TableDefinitions.Length; ++index)
            {
                TableDefinition tableDefinition = this.m_mapping.TableDefinitions[index];
                if (((IEnumerable<DataHolderDefinition>) tableDefinition.DataHolderDefinitions)
                    .Contains<DataHolderDefinition>((Func<DataHolderDefinition, bool>) (def => def.Type == type)))
                    keyValueListBaseList.Add(new KeyValueListBase(tableDefinition,
                        this.GetWherePairs(tableDefinition, obj)));
            }

            return keyValueListBaseList;
        }

        private List<KeyValuePair<string, object>> GetWherePairs(TableDefinition table, IDataHolder obj)
        {
            DataHolderDefinition holderDefinition = this.m_mapping.GetDataHolderDefinition(obj.GetType());
            List<KeyValuePair<string, object>> keyValuePairList = new List<KeyValuePair<string, object>>(2);
            foreach (PrimaryColumn primaryColumn in table.PrimaryColumns)
            {
                foreach (IFlatDataFieldAccessor field in primaryColumn.DataColumn.FieldList)
                {
                    if (field.DataHolderDefinition == holderDefinition)
                    {
                        object obj1 = field.Get(obj);
                        if (obj1 != null)
                            keyValuePairList.Add(
                                new KeyValuePair<string, object>(primaryColumn.Name, (object) obj1.ToString()));
                    }
                }
            }

            return keyValuePairList;
        }

        public bool SupportsCaching
        {
            get
            {
                for (int index = 0; index < this.m_mapping.DataHolderDefinitions.Length; ++index)
                {
                    if (!this.m_mapping.DataHolderDefinitions[index].SupportsCaching)
                        return false;
                }

                return true;
            }
        }

        public void SaveCache(string filename)
        {
            using (BinaryWriter writer =
                new BinaryWriter((Stream) new FileStream(filename, FileMode.Create, FileAccess.Write)))
            {
                this.WriteHeader(writer);
                for (int index = 0; index < this.m_mapping.DataHolderDefinitions.Length; ++index)
                {
                    DataHolderDefinition holderDefinition = this.m_mapping.DataHolderDefinitions[index];
                    object obj = holderDefinition.CacheGetter.Invoke((object) null, new object[0]);
                    new BinaryContentStream(holderDefinition).WriteAll(writer, (IEnumerable) obj);
                }
            }
        }

        public bool LoadCache(string filename)
        {
            using (BinaryReader reader =
                new BinaryReader((Stream) new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                if (!this.ReadCacheHeader(reader))
                    return false;
                List<Action> initors = new List<Action>((int) reader.BaseStream.Length / 1000);
                for (int index = 0; index < this.m_mapping.DataHolderDefinitions.Length; ++index)
                    new BinaryContentStream(this.m_mapping.DataHolderDefinitions[index]).LoadAll(reader, initors);
                if (initors.Count == 0 || reader.BaseStream.Position != reader.BaseStream.Length)
                    return false;
                foreach (Action action in initors)
                    action();
            }

            return true;
        }

        internal void WriteHeader(BinaryWriter writer)
        {
            writer.Write(5);
            writer.Write(this.m_mapping.DataHolderDefinitions.Length);
            for (int index = 0; index < this.m_mapping.DataHolderDefinitions.Length; ++index)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(this.m_mapping.DataHolderDefinitions[index].CreateIdString());
                writer.Write((ushort) bytes.Length);
                writer.Write(bytes);
            }
        }

        /// <summary>
        /// Reads the (semi-)unique signature of all DataHolders to prevent the worst
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal bool ReadCacheHeader(BinaryReader reader)
        {
            if (reader.ReadInt32() != 5 || reader.BaseStream.Position == reader.BaseStream.Length ||
                reader.ReadInt32() != this.m_mapping.DataHolderDefinitions.Length)
                return false;
            for (int index = 0; index < this.m_mapping.DataHolderDefinitions.Length; ++index)
            {
                string idString = this.m_mapping.DataHolderDefinitions[index].CreateIdString();
                ushort num = reader.ReadUInt16();
                string str = Encoding.UTF8.GetString(reader.ReadBytes((int) num));
                if (idString != str)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return "Mapper for: " +
                   ((IEnumerable<string>) ((IEnumerable<DataHolderDefinition>) this.m_mapping.DataHolderDefinitions)
                       .TransformArray<DataHolderDefinition, string>(
                           (Func<DataHolderDefinition, string>) (def => def.Name))).ToString<string>(", ");
        }
    }
}