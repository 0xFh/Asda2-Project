using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WCell.Util.Data
{
    /// <summary>
    /// Static container and utility class for DataHolder-information
    /// </summary>
    public static class DataHolderMgr
    {
        public static readonly Dictionary<string, DataHolderDefinition> DataHolderDefinitions =
            new Dictionary<string, DataHolderDefinition>(
                (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        public static void CreateAndStoreDataHolderDefinitions(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                if (((IEnumerable<DataHolderAttribute>) type.GetCustomAttributes<DataHolderAttribute>())
                    .Count<DataHolderAttribute>() > 0)
                {
                    DataHolderDefinition holderDefinition = DataHolderMgr.CreateDataHolderDefinition(type);
                    DataHolderMgr.DataHolderDefinitions.Add(holderDefinition.Name, holderDefinition);
                }
            }
        }

        public static Dictionary<string, DataHolderDefinition> CreateDataHolderDefinitionMap(Assembly asm)
        {
            Dictionary<string, DataHolderDefinition> dictionary = new Dictionary<string, DataHolderDefinition>();
            foreach (Type type in asm.GetTypes())
            {
                if (((IEnumerable<DataHolderAttribute>) type.GetCustomAttributes<DataHolderAttribute>())
                    .Count<DataHolderAttribute>() > 0)
                {
                    DataHolderDefinition holderDefinition = DataHolderMgr.CreateDataHolderDefinition(type);
                    dictionary.Add(holderDefinition.Name, holderDefinition);
                }
            }

            return dictionary;
        }

        public static DataHolderDefinition[] CreateDataHolderDefinitionArray(Assembly asm)
        {
            return DataHolderMgr.CreateDataHolderDefinitionList(asm).ToArray();
        }

        public static List<DataHolderDefinition> CreateDataHolderDefinitionList(Assembly asm)
        {
            List<DataHolderDefinition> holderDefinitionList = new List<DataHolderDefinition>();
            foreach (Type type in asm.GetTypes())
            {
                if (type.Name == "PetLevelStatInfo")
                    type.ToString();
                if (((IEnumerable<DataHolderAttribute>) type.GetCustomAttributes<DataHolderAttribute>())
                    .Count<DataHolderAttribute>() > 0)
                {
                    DataHolderDefinition holderDefinition = DataHolderMgr.CreateDataHolderDefinition(type);
                    holderDefinitionList.Add(holderDefinition);
                }
            }

            return holderDefinitionList;
        }

        public static DataHolderDefinition CreateDataHolderDefinition<T>() where T : IDataHolder
        {
            return DataHolderMgr.CreateDataHolderDefinition(typeof(T));
        }

        public static DataHolderDefinition CreateDataHolderDefinition(Type type)
        {
            if (type.GetInterface("IDataHolder") == (Type) null)
                throw new ArgumentException("DataHolder-Type must implement IDataHolder: " + type.FullName);
            DataHolderAttribute attribute =
                ((IEnumerable<DataHolderAttribute>) type.GetCustomAttributes(typeof(DataHolderAttribute), false))
                .FirstOrDefault<DataHolderAttribute>();
            string dependingField;
            string name;
            if (attribute == null)
            {
                dependingField = (string) null;
                name = type.Name;
            }
            else
            {
                name = string.IsNullOrEmpty(attribute.Name) ? type.Name : attribute.Name;
                dependingField = attribute.DependsOnField;
            }

            return new DataHolderDefinition(name, type, dependingField, attribute);
        }
    }
}