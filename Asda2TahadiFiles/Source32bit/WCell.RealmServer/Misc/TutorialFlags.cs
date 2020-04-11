using System;

namespace WCell.RealmServer.Misc
{
  public class TutorialFlags
  {
    /// <summary>Shares with the Character's record</summary>
    private byte[] m_flagData;

    internal TutorialFlags(byte[] flagData)
    {
      if(flagData.Length != 32)
        throw new ArgumentOutOfRangeException(nameof(flagData), "byte array must be 32 bytes");
      m_flagData = flagData;
    }

    internal byte[] FlagData
    {
      get { return m_flagData; }
    }

    public void SetFlag(uint flagIndex)
    {
      m_flagData[flagIndex / 8U] |= (byte) (1 << (int) flagIndex % 8);
    }

    public void ClearFlags()
    {
      for(int index = 0; index < 32; ++index)
        m_flagData[index] = byte.MaxValue;
    }

    public void ResetFlags()
    {
      for(int index = 0; index < 32; ++index)
        m_flagData[index] = 0;
    }
  }
}