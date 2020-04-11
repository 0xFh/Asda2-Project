using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace WCell.Core.Database
{
  public static class DatabaseConfiguration
  {
    private static Dictionary<string, string> s_configurationMappings = new Dictionary<string, string>();

    static DatabaseConfiguration()
    {
      s_configurationMappings.Add("mssql",
        "Configurations.SQLServerConfiguration.arconfig");
      s_configurationMappings.Add("mssql2005",
        "Configurations.SQLServer2005Configuration.arconfig");
      s_configurationMappings.Add("mysql", "Configurations.MySQLConfiguration.arconfig");
      s_configurationMappings.Add("mysql5", "Configurations.MySQL5Configuration.arconfig");
      s_configurationMappings.Add("oracle", "Configurations.OracleConfiguration.arconfig");
      s_configurationMappings.Add("pgsql",
        "Configurations.PostgreSQLServerConfiguration.arconfig");
      s_configurationMappings.Add("db2", "Configurations.DB2Configuration.arconfig");
      s_configurationMappings.Add("firebird",
        "Configurations.FireBirdConfiguration.arconfig");
      s_configurationMappings.Add("sqlLite",
        "Configurations.SQLLiteConfiguration.arconfig");
    }

    public static TextReader GetARConfiguration(string dbType, string connString)
    {
      string lower = dbType.ToLower();
      if(!s_configurationMappings.ContainsKey(lower))
        return null;
      Stream manifestResourceStream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream(typeof(DatabaseConfiguration),
          s_configurationMappings[lower]);
      if(manifestResourceStream == null)
        return null;
      string end = new StreamReader(manifestResourceStream).ReadToEnd();
      if((lower == "mysql" || lower == "mysql5") && !connString.ToLower().Contains("convert zero datetime"))
        connString += "Convert Zero DateTime=true;";
      return new StringReader(end.Replace("{0}", connString));
    }
  }
}