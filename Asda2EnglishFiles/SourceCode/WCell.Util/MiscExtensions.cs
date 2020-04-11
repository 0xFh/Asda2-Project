using System;

namespace WCell.Util
{
    public static class MiscExtensions
    {
        public static bool IsNullOrEmpty(this Array array)
        {
            if (array == null)
                return true;
            return array.Length == 0;
        }
    }
}