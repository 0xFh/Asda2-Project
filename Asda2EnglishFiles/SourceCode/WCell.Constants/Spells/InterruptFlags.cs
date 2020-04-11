using System;

namespace WCell.Constants.Spells
{
    /// <summary>Events that can interrupt casting</summary>
    [Flags]
    public enum InterruptFlags
    {
        None = 0,
        OnSilence = 1,
        OnSleep = 2,
        OnStunned = 4,
        OnMovement = 8,
        OnTakeDamage = 16, // 0x00000010
    }
}