using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
	//[DataHolder]
    [System.Serializable]
	public class UnitModelInfo : IDataHolder
	{
		public uint DisplayId;

		public float BoundingRadius, CombatReach;

		public GenderType Gender;


		public void FinalizeDataHolder()
		{
		    BoundingRadius = BoundingRadius/3;
		    CombatReach = CombatReach/3;
			if (DisplayId > 100000)
			{
				ContentMgr.OnInvalidDBData("ModelInfo has invalid Id: " + this);
				return;
			}
			if (CombatReach < 0.5f)
			{
				CombatReach = 0.5f;
			}
			ArrayUtil.Set(ref UnitMgr.ModelInfos, DisplayId, this);
		}

		public override string ToString()
		{
			return string.Format("{0} (Id: {1})", GetType(), DisplayId);
		}
	}
}