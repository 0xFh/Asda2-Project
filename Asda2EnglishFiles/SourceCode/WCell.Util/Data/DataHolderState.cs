using System;

namespace WCell.Util.Data
{
    [Flags]
    public enum DataHolderState : uint
    {
        Steady = 0,
        JustCreated = 1,
        Dirty = 2,
    }
}