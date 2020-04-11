using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
    public static class UnitMgr
    {
        [NotVariable] public static UnitModelInfo[] ModelInfos = new UnitModelInfo[1000];

        public static UnitModelInfo DefaultModel = new UnitModelInfo()
        {
            BoundingRadius = 0.1f,
            CombatReach = 0.4f,
            DisplayId = 1,
            Gender = GenderType.Neutral
        };

        private static bool loaded;

        public static void InitModels()
        {
            if (UnitMgr.loaded)
                return;
            UnitMgr.loaded = true;
            ContentMgr.Load<UnitModelInfo>();
        }

        public static UnitModelInfo GetModelInfo(uint monstrId)
        {
            return UnitMgr.DefaultModel;
        }
    }
}