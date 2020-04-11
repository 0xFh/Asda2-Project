using System;

namespace WCell.Constants
{
    [Flags]
    public enum CorpseReleaseFlags : byte
    {
        None = 0,
        TrackStealthed = 2,
        ShowCorpseAutoReleaseTimer = 8,
        HideReleaseWindow = 16, // 0x10
    }
}