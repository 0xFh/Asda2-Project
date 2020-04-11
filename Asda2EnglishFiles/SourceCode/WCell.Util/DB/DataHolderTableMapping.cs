using System;
using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.Util.DB
{
    /// <summary>
    /// Defines the relation between a set of DataHolder and the tables that belong to them.
    /// This is a many-to-many relationship:
    /// One DataHolder can use multiple tables and multiple tables can be used by the same DataHolder.
    /// It is ensured that all default-tables have a lower index than non-default ones in the <see cref="F:WCell.Util.DB.DataHolderTableMapping.TableDefinitions" />-array.
    /// </summary>
    public class DataHolderTableMapping
    {
        private static readonly DataHolderTableMapping.TableComparer DefaultTableComparer =
            new DataHolderTableMapping.TableComparer();

        public DataHolderDefinition[] DataHolderDefinitions;

        /// <summary>
        /// All Tables that are used by all <see cref="F:WCell.Util.DB.DataHolderTableMapping.DataHolderDefinitions" />.
        /// It is ensured that all default-tables have a lower index than non-default ones.
        /// </summary>
        public TableDefinition[] TableDefinitions;

        public DataHolderTableMapping(DataHolderDefinition[] dataHolderDefs, TableDefinition[] tableDefinitions)
        {
            this.DataHolderDefinitions = dataHolderDefs;
            this.TableDefinitions = tableDefinitions;
            Array.Sort<TableDefinition>(this.TableDefinitions,
                (IComparer<TableDefinition>) DataHolderTableMapping.DefaultTableComparer);
        }

        public DataHolderDefinition GetDataHolderDefinition(Type t)
        {
            foreach (DataHolderDefinition holderDefinition in this.DataHolderDefinitions)
            {
                if (holderDefinition.Type == t)
                    return holderDefinition;
            }

            return (DataHolderDefinition) null;
        }

        public override string ToString()
        {
            return string.Format("Mapping of DataHolders ({0}) to Tables ({1})",
                (object) ((IEnumerable<DataHolderDefinition>) this.DataHolderDefinitions)
                .ToString<DataHolderDefinition>(", "),
                (object) ((IEnumerable<TableDefinition>) this.TableDefinitions).ToString<TableDefinition>(", "));
        }

        public class TableComparer : IComparer<TableDefinition>
        {
            public int Compare(TableDefinition x, TableDefinition y)
            {
                return x.IsDefaultTable == y.IsDefaultTable ? 0 : (x.IsDefaultTable ? -1 : 1);
            }
        }
    }
}