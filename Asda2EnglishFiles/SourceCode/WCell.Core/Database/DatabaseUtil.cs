using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Framework.Config;
using NHibernate.Cfg;
using NHibernate.Engine;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Strings;

namespace WCell.Core.Database
{
    /// <summary>Temporary class - Will need cleanup.</summary>
    public static class DatabaseUtil
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>The TextReader from which to read Input</summary>
        public static SingleStringMover Input = (SingleStringMover) new ConsoleStringMover();

        public static readonly Dictionary<Assembly, Type[]> Types = new Dictionary<Assembly, Type[]>();
        private static TextReader s_configReader;
        private static XmlConfigurationSource s_config;
        private static NHibernate.Dialect.Dialect s_dialect;

        /// <summary>
        /// Is called when the DB creates an error and asks the User whether or not
        /// to auto-recreate the DB. Will not query the user and directly throw the Exception
        /// if false is returned (in order to avoid DB-deletion of production systems).
        /// </summary>
        public static Predicate<Exception> DBErrorHook;

        public static NHibernate.Dialect.Dialect Dialect
        {
            get { return DatabaseUtil.s_dialect; }
        }

        /// <summary>Whether it is currently waiting for user-input.</summary>
        public static bool IsWaiting { get; set; }

        public static Configuration Config
        {
            get
            {
                if (DatabaseUtil.Holder != null)
                    return DatabaseUtil.Holder.GetConfiguration(typeof(ActiveRecordBase));
                return (Configuration) null;
            }
        }

        public static ISessionFactoryImplementor SessionFactory
        {
            get
            {
                if (DatabaseUtil.Holder != null)
                    return (ISessionFactoryImplementor) DatabaseUtil.Holder.GetSessionFactory(typeof(ActiveRecordBase));
                return (ISessionFactoryImplementor) null;
            }
        }

        private static ISessionFactoryHolder Holder
        {
            get { return ActiveRecordMediator.GetSessionFactoryHolder(); }
        }

        public static Settings Settings
        {
            get { return DatabaseUtil.SessionFactory.Settings; }
        }

        public static bool IsConnected
        {
            get
            {
                ISessionImplementor session = DatabaseUtil.Session;
                return session != null && session.IsConnected;
            }
        }

        public static ISessionImplementor Session
        {
            get
            {
                if (DatabaseUtil.Holder != null)
                    return (ISessionImplementor) DatabaseUtil.Holder.CreateSession(typeof(ActiveRecordBase));
                return (ISessionImplementor) null;
            }
        }

        public static string DBType { get; set; }

        public static string ConnectionString { get; set; }

        public static string DefaultCharset
        {
            get { return "UTF8"; }
            set { }
        }

        /// <summary>
        /// Console should not be read from anymore at this point.
        /// </summary>
        public static void ReleaseConsole()
        {
            DatabaseUtil.Input = new SingleStringMover();
        }

        public static void OnDBError(Exception e, string warning)
        {
            try
            {
                if (DatabaseUtil.DBErrorHook != null && !DatabaseUtil.DBErrorHook(e))
                    throw e;
            }
            catch (Exception ex)
            {
                DatabaseUtil.log.ErrorException("", ex);
            }

            string msg = "Database Error occured";
            LogUtil.ErrorException(e, false, msg, new object[0]);
            DatabaseUtil.log.Warn("");
            foreach (string allMessage in e.GetAllMessages())
                DatabaseUtil.log.Warn(allMessage);
            DatabaseUtil.log.Warn("");
            DatabaseUtil.log.Warn("Database could not be initialized!");
            DatabaseUtil.log.Warn("Re-create Database schema? (y/n)");
            DatabaseUtil.log.Warn("WARNING: " + warning);
            DatabaseUtil.IsWaiting = true;
            bool flag;
            try
            {
                flag = StringStream.GetBool(DatabaseUtil.Input.Read());
            }
            catch
            {
                flag = true;
            }

            DatabaseUtil.IsWaiting = false;
            if (!flag)
                throw new InvalidOperationException("", e);
            DatabaseUtil.log.Warn("Dropping database schema...");
            DatabaseUtil.DropSchema();
            DatabaseUtil.log.Warn("Done.");
            DatabaseUtil.log.Warn("Re-creating database schema...");
            try
            {
                DatabaseUtil.CreateSchema();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("", ex);
            }

            DatabaseUtil.log.Warn("Done.");
        }

        /// <summary>
        /// Called to initialize setup NHibernate and ActiveRecord
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="dbType"></param>
        /// <param name="connStr"></param>
        /// <returns>Whether its a fatal error</returns>
        public static bool InitAR(Assembly asm)
        {
            if (DatabaseUtil.s_configReader == null)
            {
                DatabaseUtil.s_configReader =
                    DatabaseConfiguration.GetARConfiguration(DatabaseUtil.DBType, DatabaseUtil.ConnectionString);
                if (DatabaseUtil.s_configReader == null)
                    throw new Exception("Invalid Database Type: " + DatabaseUtil.DBType);
            }

            DatabaseUtil.s_config = new XmlConfigurationSource(DatabaseUtil.s_configReader);
            NHibernate.Cfg.Environment.UseReflectionOptimizer = true;
            ActiveRecordStarter.Initialize(asm, (IConfigurationSource) DatabaseUtil.s_config);
            if (!DatabaseUtil.IsConnected)
                throw new Exception(string.Format("Failed to connect to Database."));
            DatabaseUtil.s_dialect = NHibernate.Dialect.Dialect.GetDialect(DatabaseUtil.Config.Properties) ??
                                     NHibernate.Dialect.Dialect.GetDialect();
            return true;
        }

        /// <summary>
        /// (Drops and re-)creates the Schema of all tables that this has originally initialized with.
        /// </summary>
        public static void CreateSchema()
        {
            ActiveRecordStarter.CreateSchema();
        }

        /// <summary>
        /// Drops the Schema of all tables that this has originally initialized with
        /// </summary>
        public static void DropSchema()
        {
            ActiveRecordStarter.DropSchema();
        }

        public static string ToSqlValueString(string str)
        {
            return "'" + str + "'";
        }
    }
}