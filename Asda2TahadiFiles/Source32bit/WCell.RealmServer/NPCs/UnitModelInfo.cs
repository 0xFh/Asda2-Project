using System;
using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
  [Serializable]
  public class UnitModelInfo : IDataHolder
  {
    public uint DisplayId;
    public float BoundingRadius;
    public float CombatReach;
    public GenderType Gender;

    public void FinalizeDataHolder()
    {
      BoundingRadius /= 3f;
      CombatReach /= 3f;
      if(DisplayId > 100000U)
      {
        ContentMgr.OnInvalidDBData("ModelInfo has invalid Id: " + this);
      }
      else
      {
        if(CombatReach < 0.5)
          CombatReach = 0.5f;
        ArrayUtil.Set(ref UnitMgr.ModelInfos, DisplayId, this);
      }
    }

    public override string ToString()
    {
      return string.Format("{0} (Id: {1})", GetType(), DisplayId);
    }
  }
}