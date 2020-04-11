using Castle.ActiveRecord;
using NLog;
using System;
using System.Reflection;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer.Database
{
  /// <summary>
  /// TODO: Add method and command to re-create entire DB and resync server correspondingly
  /// </summary>
  [GlobalMgr]
  public static class RealmDBMgr
  {
    public static string DefaultCharset = "UTF8";
    private static Logger log = LogManager.GetCurrentClassLogger();
    [NotVariable]public static bool Initialized;

    public static void OnDBError(Exception e)
    {
      log.ErrorException(nameof(OnDBError), e);
    }

    [Initialization(InitializationPass.First, "Initialize database")]
    public static bool Initialize()
    {
      if(!Initialized)
      {
        Initialized = true;
        DatabaseUtil.DBErrorHook = exception => CharacterRecord.GetCount() < 100;
        DatabaseUtil.DBType = RealmServerConfiguration.DatabaseType;
        DatabaseUtil.ConnectionString = RealmServerConfiguration.DBConnectionString;
        DatabaseUtil.DefaultCharset = DefaultCharset;
        Assembly assembly = typeof(RealmDBMgr).Assembly;
        try
        {
          if(!DatabaseUtil.InitAR(assembly))
            return false;
        }
        catch(Exception ex1)
        {
          OnDBError(ex1);
          try
          {
            if(!DatabaseUtil.InitAR(assembly))
              return false;
          }
          catch(Exception ex2)
          {
            LogUtil.ErrorException(ex2, true, "Failed to initialize the Database.");
          }
        }
      }

      int num = 0;
      try
      {
        num = CharacterRecord.GetCount();
      }
      catch
      {
      }

      if(num == 0)
        DatabaseUtil.CreateSchema();
      NHIdGenerator.InitializeCreators(OnDBError);
      ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(RealmDBMgr));
      return true;
    }

    public static void UpdateLater(this ActiveRecordBase record)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => record.Update());
    }

    public static void SaveLater(this ActiveRecordBase record)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => record.Save());
    }

    public static void CreateLater(this ActiveRecordBase record)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => record.Create());
    }

    public static void DeleteLater(this ActiveRecordBase record)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() => record.Delete());
    }
  }
}