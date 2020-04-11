using System.Collections.Generic;

namespace WCell.Util.DB
{
  public class UpdateKeyValueList : KeyValueListBase
  {
    public readonly List<KeyValuePair<string, object>> Where;

    public UpdateKeyValueList(TableDefinition table)
      : base(table)
    {
      Where = new List<KeyValuePair<string, object>>();
    }

    public UpdateKeyValueList(TableDefinition table, List<KeyValuePair<string, object>> where)
      : this(table)
    {
      Where = where;
    }
  }
}