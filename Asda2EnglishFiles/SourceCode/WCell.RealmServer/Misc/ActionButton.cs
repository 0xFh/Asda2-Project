using System;

namespace WCell.RealmServer.Misc
{
    public struct ActionButton
    {
        public static readonly byte[] EmptyButton = new byte[4];
        public const uint Size = 4;
        public const uint MaxAmount = 144;
        public uint Index;
        public ushort Action;
        public byte Type;
        public byte Info;

        public static byte[] CreateEmptyActionButtonBar()
        {
            return new byte[576];
        }

        public void Set(byte[] actions)
        {
            uint num = this.Index * 4U;
            actions[num] = (byte) ((uint) this.Action & (uint) byte.MaxValue);
            actions[num + 1U] = (byte) (((int) this.Action & 65280) >> 8);
            actions[num + 2U] = this.Type;
            actions[num + 3U] = this.Info;
        }

        public static void Set(byte[] actions, uint index, ushort action, byte type, byte info)
        {
            index *= 4U;
            actions[index] = (byte) ((uint) action & (uint) byte.MaxValue);
            actions[index + 1U] = (byte) (((int) action & 65280) >> 8);
            actions[index + 2U] = type;
            actions[index + 3U] = info;
        }
    }
}