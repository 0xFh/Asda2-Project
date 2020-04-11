using NLog;
using System;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Taxi
{
  public class TaxiNodeMask
  {
    private static Logger sLog = LogManager.GetCurrentClassLogger();
    private uint[] fields;

    public uint[] Mask
    {
      get { return fields; }
      internal set { fields = value; }
    }

    public TaxiNodeMask()
    {
      fields = new uint[32];
    }

    public TaxiNodeMask(uint[] mask)
    {
      if(mask.Length < 32)
        Array.Resize(ref mask, 32);
      fields = mask;
    }

    public void Activate(PathNode node)
    {
      Activate(node.Id);
    }

    public void Activate(uint nodeId)
    {
      uint num = fields[nodeId / 32U] | 1U << (int) (nodeId % 32U);
      fields[nodeId / 32U] = num;
    }

    public bool IsActive(PathNode node)
    {
      if(node != null)
        return IsActive(node.Id);
      return false;
    }

    public bool IsActive(uint nodeId)
    {
      uint num1 = Mask[nodeId / 32U];
      uint num2 = 1U << (int) (nodeId % 32U);
      return ((int) num2 & (int) num1) == (int) num2;
    }
  }
}