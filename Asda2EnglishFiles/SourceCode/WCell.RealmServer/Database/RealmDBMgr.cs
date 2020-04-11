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
        [NotVariable] public static bool Initialized;

        public static void OnDBError(Exception e)
        {
            RealmDBMgr.log.ErrorException(nameof(OnDBError), e);
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.First, "Initialize database")]
        public static bool Initialize()
        {
            if (!RealmDBMgr.Initialized)
            {
                RealmDBMgr.Initialized = true;
                DatabaseUtil.DBErrorHook = (Predicate<Exception>) (exception => CharacterRecord.GetCount() < 100);
                DatabaseUtil.DBType = RealmServerConfiguration.DatabaseType;
                DatabaseUtil.ConnectionString = RealmServerConfiguration.DBConnectionString;
                DatabaseUtil.DefaultCharset = RealmDBMgr.DefaultCharset;
                Assembly assembly = typeof(RealmDBMgr).Assembly;
                try
                {
                    if (!DatabaseUtil.InitAR(assembly))
                        return false;
                }
                catch (Exception ex1)
                {
                    RealmDBMgr.OnDBError(ex1);
                    try
                    {
                        if (!DatabaseUtil.InitAR(assembly))
                            return false;
                    }
                    catch (Exception ex2)
                    {
                        LogUtil.ErrorException(ex2, true, "Failed to initialize the Database.", new object[0]);
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

            if (num == 0)
                DatabaseUtil.CreateSchema();
            NHIdGenerator.InitializeCreators(new Action<Exception>(RealmDBMgr.OnDBError));
            ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(RealmDBMgr));
            return true;
        }

        public static void UpdateLater(this ActiveRecordBase record)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => record.Update()));
        }

        public static void SaveLater(this ActiveRecordBase record)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => record.Save()));
        }

        public static void CreateLater(this ActiveRecordBase record)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => record.Create()));
        }

        public static void DeleteLater(this ActiveRecordBase record)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => record.Delete()));
        }
    }
}