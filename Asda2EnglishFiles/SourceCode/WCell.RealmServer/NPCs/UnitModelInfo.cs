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
            this.BoundingRadius /= 3f;
            this.CombatReach /= 3f;
            if (this.DisplayId > 100000U)
            {
                ContentMgr.OnInvalidDBData("ModelInfo has invalid Id: " + (object) this);
            }
            else
            {
                if ((double) this.CombatReach < 0.5)
                    this.CombatReach = 0.5f;
                ArrayUtil.Set<UnitModelInfo>(ref UnitMgr.ModelInfos, this.DisplayId, this);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", (object) this.GetType(), (object) this.DisplayId);
        }
    }
}