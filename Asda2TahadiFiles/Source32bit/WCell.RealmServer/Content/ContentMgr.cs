using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.Util;
using WCell.Util.Conversion;
using WCell.Util.Data;
using WCell.Util.DB;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer.Content
{
  /// <summary>TODO: Make it simple to add content from outside</summary>
  [GlobalMgr]
  public static class ContentMgr
  {
    public static readonly List<Assembly> AdditionalAssemblies = new List<Assembly>();
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>Determines how to interact with invalid content data.</summary>
    [Variable("ContentErrorResponse")]public static ErrorResponse ErrorResponse = ErrorResponse.None;

    /// <summary>
    /// Causes an error to be thrown if certain data is not present when requested
    /// </summary>
    [NotVariable]public static bool ForceDataPresence = false;

    public static bool EnableCaching = false;

    /// <summary>
    /// The name of the ContentProvider, which is also the name of the folder within the Content/Impl/ folder,
    /// in which to find the Table-definitions.
    /// </summary>
    public static string ContentProviderName = "UDB";

    private const string CacheFileSuffix = ".cache";
    private static string s_implementationFolder;
    private static string s_implementationRoot;
    private static LightDBDefinitionSet s_definitions;
    private static Dictionary<Type, LightDBMapper> s_mappersByType;
    private static bool inited;

    public static string ImplementationRoot
    {
      get { return s_implementationRoot; }
    }

    public static string ImplementationFolder
    {
      get { return s_implementationFolder; }
    }

    public static LightDBDefinitionSet Definitions
    {
      get { return s_definitions; }
    }

    public static LightDBMapper GetMapper<T>() where T : IDataHolder
    {
      return GetMapper(typeof(T));
    }

    public static LightDBMapper GetMapper(Type t)
    {
      EnsureInitialized();
      LightDBMapper lightDbMapper;
      if(!s_mappersByType.TryGetValue(t, out lightDbMapper))
        throw new Exception(string.Format(
          "DataHolder Type \"{0}\" was not registered - Make sure that it's XML definition was defined and associated correctly. If the Type is not in the Core, call ContentHandler.Initialize(Assembly) on its Assembly first.",
          t.FullName));
      return lightDbMapper;
    }

    public static Dictionary<object, IDataHolder> GetObjectMap<T>() where T : IDataHolder
    {
      return GetMapper<T>().GetObjectMap<T>();
    }

    /// <summary>
    /// Reports incorrect data, that is not Database-provider dependent.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public static void OnInvalidClientData(string msg, params object[] args)
    {
      OnInvalidClientData(string.Format(msg, args));
    }

    /// <summary>
    /// Reports incorrect data, that is not Database-provider dependent.
    /// </summary>
    public static void OnInvalidClientData(string msg)
    {
      switch(ErrorResponse)
      {
        case ErrorResponse.Warn:
          log.Warn(msg);
          break;
        case ErrorResponse.Exception:
          throw new ContentException("Error encountered when loading Content: " + msg);
      }
    }

    /// <summary>
    /// Reports incorrect data, caused by the Database-provider.
    /// </summary>
    public static void OnInvalidDBData(string msg, params object[] args)
    {
      OnInvalidDBData(string.Format(msg, args));
    }

    /// <summary>
    /// Reports incorrect data, caused by the Database-provider.
    /// </summary>
    public static void OnInvalidDBData(string msg)
    {
      switch(ErrorResponse)
      {
        case ErrorResponse.Warn:
          log.Warn("<" + ContentProviderName + ">" + msg);
          break;
        case ErrorResponse.Exception:
          throw new ContentException("Error encountered when loading Content: " + msg);
      }
    }

    public static void EnsureInitialized()
    {
      if(s_mappersByType != null)
        return;
      InitializeDefault();
    }

    [DependentInitialization(typeof(RealmDBMgr))]
    [Initialization]
    public static void Initialize()
    {
      InitializeAndLoad(typeof(ContentMgr).Assembly);
      ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(ContentMgr));
    }

    public static void InitializeDefault()
    {
      RealmDBMgr.Initialize();
      InitializeAndLoad(typeof(ContentMgr).Assembly);
    }

    public static void InitializeAndLoad(Assembly asm)
    {
      if(inited)
        return;
      Converters.Provider = new NHibernateConverterProvider();
      Initialize(asm);
      Load();
      inited = true;
    }

    public static void Initialize(Assembly asm)
    {
      s_implementationRoot = Path.Combine(RealmServerConfiguration.ContentDir, "Impl");
      s_implementationFolder =
        Path.Combine(s_implementationRoot, ContentProviderName);
      DataHolderDefinition[] holderDefinitionArray = DataHolderMgr.CreateDataHolderDefinitionArray(asm);
      LightDBMgr.InvalidDataHandler = OnInvalidDBData;
      s_definitions = new LightDBDefinitionSet(holderDefinitionArray);
    }

    /// <summary>Checks the validity of all Table definitions</summary>
    /// <returns>The amount of invalid columns.</returns>
    public static int Check(Action<string> feedbackCallback)
    {
      EnsureInitialized();
      int num = 0;
      foreach(LightDBMapper lightDbMapper in s_mappersByType.Values)
      {
        foreach(TableDefinition tableDefinition in lightDbMapper.Mapping.TableDefinitions)
        {
          foreach(SimpleDataColumn columnDefinition in tableDefinition.ColumnDefinitions)
          {
            if(!columnDefinition.IsEmpty)
            {
              try
              {
                using(lightDbMapper.Wrapper.Query(SqlUtil.BuildSelect(new string[1]
                {
                  columnDefinition.ColumnName
                }, tableDefinition.Name, "LIMIT 1")))
                  ;
              }
              catch(Exception ex)
              {
                feedbackCallback(string.Format("Invalid column \"{0}\" in table \"{1}\": {2}",
                  columnDefinition, tableDefinition.Name,
                  ex.GetAllMessages().ToString("\n\t")));
                ++num;
              }
            }
          }
        }
      }

      return num;
    }

    public static void Load()
    {
      s_definitions.Clear();
      string file = Path.Combine(s_implementationFolder, "Tables.xml");
      string dir = Path.Combine(s_implementationFolder, "Data");
      s_definitions.LoadTableDefinitions(file);
      CheckVersion();
      s_definitions.LoadDataHolderDefinitions(dir);
      if(!ActiveRecordStarter.IsInitialized)
        throw new InvalidOperationException("ActiveRecord must be initialized.");
      s_mappersByType = CreateMappersByType();
    }

    public static void CheckVersion()
    {
    }

    public static Dictionary<Type, LightDBMapper> CreateMappersByType()
    {
      Dictionary<Type, LightDBMapper> dictionary = new Dictionary<Type, LightDBMapper>();
      foreach(DataHolderTableMapping mapping in s_definitions.Mappings)
      {
        LightDBMapper lightDbMapper = new LightDBMapper(mapping, new NHibernateDbWrapper());
        foreach(DataHolderDefinition holderDefinition in mapping.DataHolderDefinitions)
          dictionary.Add(holderDefinition.Type, lightDbMapper);
      }

      return dictionary;
    }

    /// <summary>
    /// Ensures that the DataHolder of the given type and those that are connected with it, are loaded.
    /// 
    /// </summary>
    public static bool Load<T>() where T : IDataHolder
    {
      return Load<T>(false);
    }

    /// <summary>
    /// Ensures that the DataHolder of the given type and those that are connected with it, are loaded.
    /// 
    /// </summary>
    /// <param name="force">Whether to re-load if already loaded.</param>
    public static bool Load<T>(bool force) where T : IDataHolder
    {
      EnsureInitialized();
      LightDBMapper mapper = GetMapper<T>();
      if(!force && mapper.Fetched)
        return false;
      if(force && mapper.IsCached())
        mapper.FlushCache();
      Load(mapper);
      return true;
    }

    public static void Load(LightDBMapper mapper)
    {
      Load(mapper, true);
    }

    public static void Load(LightDBMapper mapper, bool failException)
    {
      try
      {
        if(EnableCaching && mapper.SupportsCaching && mapper.LoadCache())
          return;
        mapper.Fetch();
        if(!EnableCaching || !mapper.SupportsCaching)
          return;
        log.Info("Saving cache for: " +
                 mapper.Mapping.DataHolderDefinitions
                   .ToString(", "));
        mapper.SaveCache();
      }
      catch(Exception ex)
      {
        if(failException)
          throw new ContentException(ex, "Unable to load entries using \"{0}\"", (object) mapper);
      }
    }

    /// <summary>Fetches all content of all registered DataHolders.</summary>
    public static void FetchAll()
    {
      EnsureInitialized();
      foreach(LightDBMapper lightDbMapper in s_mappersByType.Values)
        lightDbMapper.Fetch();
    }

    /// <summary>
    /// Updates changes to the Object in the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// </summary>
    public static void CommitUpdate(this IDataHolder obj)
    {
      GetMapper(obj.GetType()).Update(obj);
    }

    /// <summary>
    /// Inserts the Object into the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// </summary>
    public static void CommitInsert(this IDataHolder obj)
    {
      GetMapper(obj.GetType()).Insert(obj);
    }

    /// <summary>
    /// Deletes the Object from the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// </summary>
    public static void CommitDelete(this IDataHolder obj)
    {
      GetMapper(obj.GetType()).Delete(obj);
    }

    /// <summary>
    /// Updates changes to the Object in the underlying Database.
    /// </summary>
    public static void CommitUpdateAndFlush(this IDataHolder obj)
    {
      obj.CommitUpdate();
      FlushCommit(obj.GetType());
    }

    /// <summary>
    /// Inserts the Object into the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// </summary>
    public static void CommitInsertAndFlush(this IDataHolder obj)
    {
      obj.CommitInsert();
      FlushCommit(obj.GetType());
    }

    /// <summary>
    /// Deletes the Object from the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// </summary>
    public static void CommitDeleteAndFlush(this IDataHolder obj)
    {
      obj.CommitDelete();
      FlushCommit(obj.GetType());
    }

    /// <summary>
    /// Ignore all changes before last FlushCommit() (will not change the Object's state).
    /// </summary>
    public static void IgnoreUnflushedChanges<T>() where T : IDataHolder
    {
      GetMapper(typeof(T)).IgnoreUnflushedChanges();
    }

    /// <summary>
    /// Flush all commited changes to the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// Will be executed in the global IO context.
    /// </summary>
    public static void FlushCommit<T>() where T : IDataHolder
    {
      LightDBMapper mapper = GetMapper(typeof(T));
      ServerApp<RealmServer>.IOQueue.ExecuteInContext(() => mapper.Flush());
    }

    /// <summary>
    /// Flush all commited changes to the underlying Database.
    /// FlushCommit() needs to be called to persist the operation.
    /// Will be executed in the global IO context.
    /// </summary>
    public static void FlushCommit(Type t)
    {
      LightDBMapper mapper = GetMapper(t);
      ServerApp<RealmServer>.IOQueue.ExecuteInContext(() => mapper.Flush());
    }

    public static void SaveCache(this LightDBMapper mapper)
    {
      string cacheFilename = mapper.GetCacheFilename();
      try
      {
        mapper.SaveCache(cacheFilename);
      }
      catch(Exception ex)
      {
        File.Delete(cacheFilename);
        LogUtil.ErrorException(ex, "Failed to save cache to file: " + cacheFilename);
      }
    }

    public static bool IsCached(this LightDBMapper mapper)
    {
      return File.Exists(mapper.GetCacheFilename());
    }

    public static void FlushCache(this LightDBMapper mapper)
    {
      string cacheFilename = mapper.GetCacheFilename();
      if(!File.Exists(cacheFilename))
        return;
      File.Delete(cacheFilename);
    }

    private static bool LoadCache(this LightDBMapper mapper)
    {
      string cacheFilename = mapper.GetCacheFilename();
      if(mapper.IsCached())
      {
        try
        {
          if(mapper.LoadCache(cacheFilename))
            return true;
          log.Warn("Cache signature in file \"{0}\" is out of date.", cacheFilename);
          log.Warn("Reloading content from Database...");
        }
        catch(Exception ex)
        {
          if(ex is EndOfStreamException)
            log.Warn("Cache signature in file \"{0}\" is out of date.", cacheFilename);
          else
            LogUtil.ErrorException(ex, "Unable to load cache from \"" + cacheFilename + "\".");
          log.Warn("Reloading content from Database...");
        }
      }

      return false;
    }

    public static string GetCacheFilename(this LightDBMapper mapper)
    {
      StringBuilder stringBuilder = new StringBuilder(mapper.Mapping.DataHolderDefinitions.Length * 12);
      foreach(DataHolderDefinition holderDefinition in mapper.Mapping.DataHolderDefinitions)
        stringBuilder.Append(holderDefinition.Name);
      stringBuilder.Append(".cache");
      return RealmServerConfiguration.Instance.GetCacheFile(stringBuilder.ToString());
    }

    /// <summary>Deletes all cache-files</summary>
    public static void PurgeCache()
    {
      foreach(string file in Directory.GetFiles(RealmServerConfiguration.Instance.CacheDir))
      {
        if(file.EndsWith(".cache"))
          File.Delete(file);
      }
    }

    public static void SaveDefaultStubs()
    {
      SaveStubs(typeof(ContentMgr).Assembly);
    }

    public static void SaveStubs(Assembly asm)
    {
      EnsureInitialized();
      LightDBMgr.SaveAllStubs(Path.Combine(s_implementationRoot, ".stubs"),
        DataHolderMgr.CreateDataHolderDefinitionArray(asm));
    }
  }
}