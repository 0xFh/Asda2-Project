using System;

namespace WCell.Util.Variables
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NotVariableAttribute : Attribute
    {
    }
}