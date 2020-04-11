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
      m_factory = DatabaseUtil.SessionFactory;
      m_session = DatabaseUtil.Session;
      if(m_factory == null || m_session == null)
        throw new InvalidOperationException("ActiveRecord was not initialized.");
    }

    public void Prepare(LightDBMapper mapper)
    {
      TableDefinition[] tableDefinitions = mapper.Mapping.TableDefinitions;
      m_selectCommands = new IDbCommand[tableDefinitions.Length];
      for(int index = 0; index < tableDefinitions.Length; ++index)
      {
        TableDefinition tableDefinition = tableDefinitions[index];
        IDbCommand command =
          CreateCommand(SqlUtil.BuildSelect(tableDefinition.AllColumns, tableDefinition.Name));
        m_selectCommands[index] = command;
      }
    }

    public IDataReader CreateReader(TableDefinition def, int tableIndex)
    {
      return m_session.Batcher.ExecuteReader(m_selectCommands[tableIndex]);
    }

    public IDataReader Query(string query)
    {
      return Query(new SqlString(query));
    }

    public void Insert(KeyValueListBase list)
    {
      ExecuteComand(SqlUtil.BuildInsert(list));
    }

    public void Update(UpdateKeyValueList list)
    {
      ExecuteComand(SqlUtil.BuildUpdate(list));
    }

    public void Delete(KeyValueListBase list)
    {
      if(list.Pairs.Count == 0)
        return;
      ExecuteComand(SqlUtil.BuildDelete(list.TableName, SqlUtil.BuildWhere(list.Pairs)));
    }

    public IDataReader Query(SqlString query)
    {
      return m_session.Batcher.ExecuteReader(
        m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, query,
          EmptySqlTypeArr));
    }

    public IDbCommand CreateCommand(string sql)
    {
      return m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, new SqlString(sql),
        EmptySqlTypeArr);
    }

    public void ExecuteComand(string sql)
    {
      m_session.Batcher.ExecuteNonQuery(CreateCommand(sql));
    }

    /// <summary>
    /// Should return a version string in the format of a float.
    /// </summary>
    public string GetDatabaseVersion(string tableName, string columnName)
    {
      IDataReader dataReader = Query(SqlUtil.BuildSelect(new string[1]
      {
        columnName
      }, tableName));
      dataReader.Read();
      return dataReader.GetValue(0).ToString();
    }
  }
}