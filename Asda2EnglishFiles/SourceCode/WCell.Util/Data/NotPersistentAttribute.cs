using System;

namespace WCell.Util.Data
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NotPersistentAttribute : Attribute
    {
    }
}