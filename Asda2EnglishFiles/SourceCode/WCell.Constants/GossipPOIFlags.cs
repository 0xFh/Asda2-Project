using System;

namespace WCell.Constants
{
    [Flags]
    [Serializable]
    public enum GossipPOIFlags : uint
    {
        None = 0,
        Six = 6,
    }
}