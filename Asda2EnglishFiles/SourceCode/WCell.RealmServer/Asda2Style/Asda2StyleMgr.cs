using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Asda2Style
{
    public static class Asda2StyleMgr
    {
        public static Dictionary<short, FaceTableRecord> FaceTemplates = new Dictionary<short, FaceTableRecord>();
        public static Dictionary<short, HairTableRecord> HairTemplates = new Dictionary<short, HairTableRecord>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Style shop.")]
        public static void Init()
        {
            ContentMgr.Load<HairTableRecord>();
            ContentMgr.Load<FaceTableRecord>();
        }
    }
}