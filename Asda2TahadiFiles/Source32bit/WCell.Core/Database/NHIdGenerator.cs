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
      OnError = onError;
      foreach(NHIdGenerator creator in _creators)
        creator.Init();
      _DBInitialized = true;
    }

    public NHIdGenerator(Type type, string idMember, long minId = 1)
      : this(type, idMember, type.Name, minId)
    {
    }

    public NHIdGenerator(Type type, string idMember, string tableName, long minId = 1)
    {
      m_type = type;
      m_table = tableName;
      m_idMember = idMember;
      m_minId = minId;
      if(_DBInitialized)
        Init();
      else
        _creators.Add(this);
    }

    private void Init()
    {
      ScalarQuery<object> scalarQuery = new ScalarQuery<object>(m_type,
        string.Format("SELECT max(r.{0}) FROM {1} r", m_idMember, m_table));
      object obj;
      try
      {
        obj = scalarQuery.Execute();
      }
      catch(Exception ex)
      {
        OnError(ex);
        obj = scalarQuery.Execute();
      }

      m_highestId = obj != null ? (long) Convert.ChangeType(obj, typeof(long)) : 0L;
      if(m_highestId >= m_minId)
        return;
      m_highestId = m_minId;
    }

    public long LastId
    {
      get { return Interlocked.Read(ref m_highestId); }
    }

    public long Next()
    {
      return Interlocked.Increment(ref m_highestId);
    }
  }
}