using System;

namespace WCell.Util.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PersistentAttribute : DBAttribute
    {
        public int Length;

        /// <summary>A custom variable accessor.</summary>
        public Type AccessorType;

        /// <summary>Used to read data from the db as this type</summary>
        public Type ReadType;

        /// <summary>
        /// Used if this object is actually of a different type then it's field/property declares
        /// </summary>
        public Type ActualType;

        public PersistentAttribute()
        {
        }

        /// <summary>Used to convert the type in the Db to this type</summary>
        public PersistentAttribute(Type readType)
        {
            this.ReadType = readType;
        }

        public PersistentAttribute(string name)
        {
            this.Name = name;
        }

        public PersistentAttribute(int arrLength)
        {
            this.Length = arrLength;
        }

        public PersistentAttribute(string name, int arrLength)
        {
            this.Length = arrLength;
            this.Name = name;
        }
    }
}