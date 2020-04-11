using System.Runtime.InteropServices;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// Represents a unique Aura-identifier: 2 Auras are exactly the same, only
    /// if they have the same spell-id and are both either positive or negative.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct AuraIndexId
    {
        public const uint AuraIdMask = 16777215;
        public static readonly AuraIndexId None;
        [FieldOffset(0)] public uint AuraUID;
        [FieldOffset(3)] public bool IsPositive;

        public AuraIndexId(uint auraUID, bool isPositive)
        {
            this.AuraUID = auraUID;
            this.IsPositive = isPositive;
        }

        public override bool Equals(object obj)
        {
            if (obj is AuraIndexId)
                return (int) ((AuraIndexId) obj).AuraUID == (int) this.AuraUID;
            return false;
        }

        public override int GetHashCode()
        {
            return this.AuraUID.GetHashCode();
        }

        public override string ToString()
        {
            return ((int) this.AuraUID & 16777215).ToString() +
                   (this.IsPositive ? (object) " (Beneficial)" : (object) " (Harmful)");
        }
    }
}