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
      uint num = Index * 4U;
      actions[num] = (byte) (Action & (uint) byte.MaxValue);
      actions[num + 1U] = (byte) ((Action & 65280) >> 8);
      actions[num + 2U] = Type;
      actions[num + 3U] = Info;
    }

    public static void Set(byte[] actions, uint index, ushort action, byte type, byte info)
    {
      index *= 4U;
      actions[index] = (byte) (action & (uint) byte.MaxValue);
      actions[index + 1U] = (byte) ((action & 65280) >> 8);
      actions[index + 2U] = type;
      actions[index + 3U] = info;
    }
  }
}