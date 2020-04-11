using System.Collections.Generic;

namespace WCell.Util.DB
{
    public class KeyValueListBase
    {
        public readonly TableDefinition Table;
        public readonly List<KeyValuePair<string, object>> Pairs;

        public KeyValueListBase(TableDefinition table)
        {
            this.Table = table;
            this.Pairs = new List<KeyValuePair<string, object>>();
        }

        public KeyValueListBase(TableDefinition table, List<KeyValuePair<string, object>> pairs)
            : this(table)
        {
            this.Pairs = pairs;
        }

        public string TableName
        {
            get { return this.Table.Name; }
        }

        public void AddPair(string key, object value)
        {
            this.Pairs.Add(new KeyValuePair<string, object>(key, value));
        }
    }
}