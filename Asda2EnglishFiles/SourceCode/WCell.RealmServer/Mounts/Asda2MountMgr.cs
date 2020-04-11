using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util.Variables;

namespace WCell.RealmServer.Mounts
{
    public static class Asda2MountMgr
    {
        [NotVariable]
        public static Dictionary<int, MountTemplate> TemplatesByItemIDs = new Dictionary<int, MountTemplate>();

        [NotVariable] public static Dictionary<int, MountTemplate> TemplatesById = new Dictionary<int, MountTemplate>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Last, "Mount system")]
        public static void Init()
        {
            ContentMgr.Load<MountTemplate>();
        }
    }
}