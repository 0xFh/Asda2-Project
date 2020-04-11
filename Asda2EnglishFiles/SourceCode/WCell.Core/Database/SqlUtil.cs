using System.Collections.Generic;
using System.Text;
using WCell.Util.DB;

namespace WCell.Core.Database
{
    public class SqlUtil
    {
        public static string BuildSelect(string[] columns, string from)
        {
            return SqlUtil.BuildSelect(columns, from, (string) null);
        }

        public static string BuildSelect(string[] columns, string from, string suffix)
        {
            NHibernate.Dialect.Dialect dialect = DatabaseUtil.Dialect;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT ");
            for (int index = 0; index < columns.Length; ++index)
            {
                string column = columns[index];
                stringBuilder.Append(dialect.QuoteForColumnName(column));
                if (index < columns.Length - 1)
                    stringBuilder.Append(",");
            }

            stringBuilder.Append(" FROM ");
            stringBuilder.Append(from);
            if (suffix != null)
                stringBuilder.Append(" " + suffix);
            return stringBuilder.ToString();
        }

        public static string BuildInsert(KeyValueListBase liste)
        {
            List<KeyValuePair<string, object>> pairs = liste.Pairs;
            int count = pairs.Count;
            StringBuilder stringBuilder = SqlUtil.PrepareInsertBuilder(liste);
            stringBuilder.Append("(");
            for (int index = 0; index < count; ++index)
            {
                KeyValuePair<string, object> keyValuePair = pairs[index];
                stringBuilder.Append(SqlUtil.GetValueString(keyValuePair.Value));
                if (index < count - 1)
                    stringBuilder.Append(",");
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        public static string BuildUpdate(UpdateKeyValueList list)
        {
            return SqlUtil.BuildUpdate((KeyValueListBase) list, SqlUtil.BuildWhere(list.Where));
        }

        public static string BuildUpdate(KeyValueListBase liste, string where)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE " + liste.TableName + " SET ");
            SqlUtil.AppendKeyValuePairs(sb, liste.Pairs, ", ");
            sb.Append(" WHERE ");
            sb.Append(where);
            return sb.ToString();
        }

        public static string BuildDelete(string table, string where)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("DELETE FROM ");
            stringBuilder.Append(table);
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(where);
            return stringBuilder.ToString();
        }

        public static string BuildWhere(List<KeyValuePair<string, object>> pairs)
        {
            StringBuilder sb = new StringBuilder();
            SqlUtil.AppendKeyValuePairs(sb, pairs, " AND ");
            return sb.ToString();
        }

        public static void AppendKeyValuePairs(StringBuilder sb, List<KeyValuePair<string, object>> pairs,
            string connector)
        {
            for (int index = 0; index < pairs.Count; ++index)
            {
                KeyValuePair<string, object> pair = pairs[index];
                sb.Append(
                    DatabaseUtil.Dialect.QuoteForColumnName(pair.Key) + " = " + SqlUtil.GetValueString(pair.Value));
                if (index < pairs.Count - 1)
                    sb.Append(connector);
            }
        }

        public static string GetValueString(object obj)
        {
            return obj.ToString();
        }

        private static StringBuilder PrepareInsertBuilder(KeyValueListBase liste)
        {
            List<KeyValuePair<string, object>> pairs = liste.Pairs;
            int count = pairs.Count;
            StringBuilder stringBuilder = new StringBuilder(150);
            stringBuilder.Append("INSERT INTO " + DatabaseUtil.Dialect.QuoteForTableName(liste.TableName) + " (");
            for (int index = 0; index < count; ++index)
            {
                KeyValuePair<string, object> keyValuePair = pairs[index];
                stringBuilder.Append(DatabaseUtil.Dialect.QuoteForColumnName(keyValuePair.Key));
                if (index < count - 1)
                    stringBuilder.Append(",");
            }

            stringBuilder.Append(") VALUES ");
            return stringBuilder;
        }
    }
}