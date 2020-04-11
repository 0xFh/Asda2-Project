using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using System;
using System.Data;
using WCell.Core.Database;
using WCell.Util.DB;

namespace WCell.RealmServer.Content
{
    /// <summary>
    /// <see cref="T:NHibernate.Persister.Entity.AbstractEntityPersister" />
    /// <see cref="T:NHibernate.Persister.Entity.SingleTableEntityPersister" />
    /// </summary>
    public class NHibernateDbWrapper : IDbWrapper
    {
        public static readonly SqlType[] EmptySqlTypeArr = new SqlType[0];
        private ISessionFactoryImplementor m_factory;
        private ISessionImplementor m_session;
        private IDbCommand[] m_selectCommands;

        /// <summary>
        /// Make sure to initialize ActiveRecord before calling this ctor
        /// </summary>
        public NHibernateDbWrapper()
        {
            this.m_factory = DatabaseUtil.SessionFactory;
            this.m_session = DatabaseUtil.Session;
            if (this.m_factory == null || this.m_session == null)
                throw new InvalidOperationException("ActiveRecord was not initialized.");
        }

        public void Prepare(LightDBMapper mapper)
        {
            TableDefinition[] tableDefinitions = mapper.Mapping.TableDefinitions;
            this.m_selectCommands = new IDbCommand[tableDefinitions.Length];
            for (int index = 0; index < tableDefinitions.Length; ++index)
            {
                TableDefinition tableDefinition = tableDefinitions[index];
                IDbCommand command =
                    this.CreateCommand(SqlUtil.BuildSelect(tableDefinition.AllColumns, tableDefinition.Name));
                this.m_selectCommands[index] = command;
            }
        }

        public IDataReader CreateReader(TableDefinition def, int tableIndex)
        {
            return this.m_session.Batcher.ExecuteReader(this.m_selectCommands[tableIndex]);
        }

        public IDataReader Query(string query)
        {
            return this.Query(new SqlString(query));
        }

        public void Insert(KeyValueListBase list)
        {
            this.ExecuteComand(SqlUtil.BuildInsert(list));
        }

        public void Update(UpdateKeyValueList list)
        {
            this.ExecuteComand(SqlUtil.BuildUpdate(list));
        }

        public void Delete(KeyValueListBase list)
        {
            if (list.Pairs.Count == 0)
                return;
            this.ExecuteComand(SqlUtil.BuildDelete(list.TableName, SqlUtil.BuildWhere(list.Pairs)));
        }

        public IDataReader Query(SqlString query)
        {
            return this.m_session.Batcher.ExecuteReader(
                this.m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, query,
                    NHibernateDbWrapper.EmptySqlTypeArr));
        }

        public IDbCommand CreateCommand(string sql)
        {
            return this.m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, new SqlString(sql),
                NHibernateDbWrapper.EmptySqlTypeArr);
        }

        public void ExecuteComand(string sql)
        {
            this.m_session.Batcher.ExecuteNonQuery(this.CreateCommand(sql));
        }

        /// <summary>
        /// Should return a version string in the format of a float.
        /// </summary>
        public string GetDatabaseVersion(string tableName, string columnName)
        {
            IDataReader dataReader = this.Query(SqlUtil.BuildSelect(new string[1]
            {
                columnName
            }, tableName));
            dataReader.Read();
            return dataReader.GetValue(0).ToString();
        }
    }
}