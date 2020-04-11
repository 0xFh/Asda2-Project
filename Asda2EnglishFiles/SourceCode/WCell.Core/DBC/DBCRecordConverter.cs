using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.Core.DBC
{
    public abstract class DBCRecordConverter : IDisposable
    {
        private byte[] m_stringTable;

        public void Init(byte[] stringTable)
        {
            this.m_stringTable = stringTable;
        }

        public virtual void Convert(byte[] rawData)
        {
        }

        protected static int CopyTo(byte[] bytes, object obj, int index)
        {
            foreach (MemberInfo member in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Cast<MemberInfo>()
                .Concat<MemberInfo>(
                    (IEnumerable<MemberInfo>) obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)))
            {
                if (!member.IsReadonly() &&
                    member.GetCustomAttributes(typeof(NotPersistentAttribute), true).Length <= 0)
                {
                    object obj1 = Utility.ChangeType((object) DBCRecordConverter.GetInt32(bytes, index++),
                        member.GetVariableType());
                    member.SetUnindexedValue(obj, obj1);
                }
            }

            return index;
        }

        /// <summary>
        /// Copies the next count fields into obj, starting from offset.
        /// Keep in mind, that one field has a length of 4 bytes.
        /// </summary>
        protected static void CopyTo(byte[] bytes, int fromOffset, int length, object target, int toOffset)
        {
            if (length % 4 != 0)
                throw new Exception("Cannot copy to object " + target + " because it's size is not a multiple of 4.");
            try
            {
                GCHandle gcHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(bytes, fromOffset * 4,
                        new IntPtr(gcHandle.AddrOfPinnedObject().ToInt64() + (long) toOffset), length);
                }
                finally
                {
                    gcHandle.Free();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Unable to copy bytes to object {0} of type {1}", target, (object) target.GetType()),
                    ex);
            }
        }

        protected static uint GetUInt32(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();
            return BitConverter.ToUInt32(data, startIndex);
        }

        protected static int GetInt32(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();
            return BitConverter.ToInt32(data, startIndex);
        }

        protected static float GetFloat(byte[] data, int field)
        {
            int startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                throw new IndexOutOfRangeException();
            return BitConverter.ToSingle(data, startIndex);
        }

        protected static ulong GetUInt64(byte[] data, int startingField)
        {
            int startIndex = startingField * 4;
            if (startIndex + 8 > data.Length)
                throw new IndexOutOfRangeException();
            return BitConverter.ToUInt64(data, startIndex);
        }

        public string GetString(byte[] data, int stringOffset)
        {
            return this.GetString(data, WCellConstants.DefaultLocale, stringOffset);
        }

        public string[] GetStrings(byte[] data, int stringOffset)
        {
            string[] strArray = new string[8];
            for (int index = 0; index < 8; ++index)
                strArray[index] = this.GetString(data, (ClientLocale) index, stringOffset);
            return strArray;
        }

        public string GetString(byte[] data, ref int offset)
        {
            string str = this.GetString(data, offset);
            offset += 17;
            return str;
        }

        public string GetString(byte[] data, ClientLocale locale, int stringOffset)
        {
            int int32 = DBCRecordConverter.GetInt32(data, (int) (stringOffset + locale));
            int num = 0;
            do
                ;
            while (this.m_stringTable[int32 + num++] != (byte) 0);
            return WCellConstants.DefaultEncoding.GetString(this.m_stringTable, int32, num - 1) ?? "";
        }

        public void Dispose()
        {
            this.m_stringTable = (byte[]) null;
        }
    }
}