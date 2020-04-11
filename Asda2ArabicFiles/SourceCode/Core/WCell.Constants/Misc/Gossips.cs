using System;

namespace WCell.Constants
{
    [System.Serializable]
	public enum GossipMenuIcon : byte
	{
		Talk = 0,
		Trade = 1,
		Taxi = 2,
		Train = 2,
		Resurrect = 3,
		Bind = 4,
		Bank = 5,
		Guild = 6,
		Tabard = 7,
		Battlefield = 8,
		End
	}

	[Flags]
    [System.Serializable]
	public enum GossipPOIFlags : uint
	{
		None = 0,
        Six = 6
	}
}