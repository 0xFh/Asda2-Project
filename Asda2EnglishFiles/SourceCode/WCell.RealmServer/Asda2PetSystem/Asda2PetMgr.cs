using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util.Variables;

namespace WCell.RealmServer.Asda2PetSystem
{
    public static class Asda2PetMgr
    {
        [NotVariable] public static PetTemplate[] PetTemplates = new PetTemplate[10000];

        [NotVariable]
        public static Dictionary<int, Dictionary<int, int[]>> ExpTable = new Dictionary<int, Dictionary<int, int[]>>();

        [NotVariable]
        public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>> PetOptionValues =
            new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>();

        [NotVariable]
        public static Dictionary<int, Dictionary<int, Dictionary<string, PetTemplate>>> PetTemplatesByRankAndRarity =
            new Dictionary<int, Dictionary<int, Dictionary<string, PetTemplate>>>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Third, Name = "Pet system")]
        public static void InitEntries()
        {
            ContentMgr.Load<PetTemplate>();
            ContentMgr.Load<PetExpTableRecord>();
            ContentMgr.Load<PetOptionValueRecord>();
        }
    }
}