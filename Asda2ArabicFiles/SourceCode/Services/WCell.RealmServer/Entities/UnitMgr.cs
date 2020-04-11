using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
	public static class UnitMgr
	{
		[NotVariable]
		public static UnitModelInfo[] ModelInfos = new UnitModelInfo[1000];

		private static bool loaded;
		
		public static void InitModels()
		{
			if (loaded)
			{
				return;
			}
			loaded = true;
			ContentMgr.Load<UnitModelInfo>();
		}

        public static UnitModelInfo DefaultModel = new UnitModelInfo(){ BoundingRadius =  0.1f,CombatReach = 0.4f,DisplayId=1,Gender = GenderType.Neutral}; 
		public static UnitModelInfo GetModelInfo(uint monstrId)
		{
			//InitModels();
            return  DefaultModel;
		}
	}
}