using System.Collections.Generic;
using WCell.Core.DBC;

namespace WCell.RealmServer.Misc
{
    public static class CfgCategories
    {
        public static Dictionary<int, string> ReadCategories()
        {
            return new MappedDBCReader<string, DBCCtfCategoriesConverter>(
                RealmServerConfiguration.GetDBCFile("Cfg_Categories.dbc")).Entries;
        }
    }
}