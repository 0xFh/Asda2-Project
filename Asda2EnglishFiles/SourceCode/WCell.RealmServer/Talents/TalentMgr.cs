using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Talents;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Talents
{
    public static class TalentMgr
    {
        /// <summary>Player talent reset price in copper</summary>
        public static readonly uint[] PlayerTalentResetPricesPerTier = new uint[11]
        {
            1000U,
            5000U,
            10000U,
            15000U,
            20000U,
            25000U,
            30000U,
            35000U,
            40000U,
            45000U,
            50000U
        };

        /// <summary>Pet talent reset price in copper</summary>
        public static readonly uint[] PetTalentResetPricesPerTier = new uint[12]
        {
            1000U,
            5000U,
            10000U,
            20000U,
            30000U,
            40000U,
            50000U,
            60000U,
            70000U,
            80000U,
            90000U,
            100000U
        };

        [NotVariable] public static TalentTree[] TalentTrees = new TalentTree[612];
        [NotVariable] public static TalentTree[][] TreesByClass = new TalentTree[12][];
        [NotVariable] public static TalentEntry[] Entries = new TalentEntry[2000];
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Maximum amount of TalentTrees per class (hardcoded in client)
        /// for some reason pet "Cunning" is on tab 4
        /// </summary>
        public const int MaxTabCount = 5;

        internal const int MaxTalentRowCount = 20;
        internal const int MaxTalentColCount = 8;

        /// <summary>Returns all Trees of the given class</summary>
        public static TalentTree[] GetTrees(ClassId clss)
        {
            return TalentMgr.TreesByClass[(uint) clss];
        }

        /// <summary>Returns the requested TalentTree</summary>
        public static TalentTree GetTree(TalentTreeId treeId)
        {
            return TalentMgr.TalentTrees[(uint) treeId];
        }

        /// <summary>Returns the requested TalentEntry</summary>
        public static TalentEntry GetEntry(TalentId talentId)
        {
            return TalentMgr.Entries[(uint) talentId];
        }

        public class TalentTreeConverter : AdvancedDBCRecordConverter<TalentTree>
        {
            public override TalentTree ConvertTo(byte[] rawData, ref int id)
            {
                TalentTree talentTree = new TalentTree();
                id = (int) (talentTree.Id = (TalentTreeId) DBCRecordConverter.GetUInt32(rawData, 0));
                talentTree.Name = this.GetString(rawData, 1);
                ClassMask uint32 = (ClassMask) DBCRecordConverter.GetUInt32(rawData, 20);
                talentTree.Class = WCellConstants.ClassTypesByMask[uint32];
                talentTree.PetTabIndex = DBCRecordConverter.GetUInt32(rawData, 21);
                talentTree.TabIndex = DBCRecordConverter.GetUInt32(rawData, 22);
                return talentTree;
            }
        }

        public class TalentConverter : AdvancedDBCRecordConverter<TalentEntry>
        {
            public override TalentEntry ConvertTo(byte[] rawData, ref int id)
            {
                TalentEntry talentEntry = new TalentEntry();
                id = (int) (talentEntry.Id = (TalentId) DBCRecordConverter.GetUInt32(rawData, 0));
                TalentTreeId uint32_1 = (TalentTreeId) DBCRecordConverter.GetUInt32(rawData, 1);
                talentEntry.Tree = TalentMgr.TalentTrees.Get<TalentTree>((uint) uint32_1);
                if (talentEntry.Tree == null)
                    return (TalentEntry) null;
                talentEntry.Row = DBCRecordConverter.GetUInt32(rawData, 2);
                talentEntry.Col = DBCRecordConverter.GetUInt32(rawData, 3);
                List<Spell> spellList = new List<Spell>(5);
                for (int index = 0; index < 9; ++index)
                {
                    uint uint32_2 = DBCRecordConverter.GetUInt32(rawData, index + 4);
                    Spell triggerSpell;
                    if (uint32_2 != 0U && (triggerSpell = SpellHandler.Get(uint32_2)) != null)
                    {
                        if (triggerSpell.IsTeachSpell)
                            triggerSpell =
                                triggerSpell.GetEffectsWhere(
                                    (Predicate<SpellEffect>) (effect => effect.TriggerSpell != null))[0].TriggerSpell;
                        if (triggerSpell != null)
                            spellList.Add(triggerSpell);
                        else
                            TalentMgr.log.Warn("Talent has invalid Spell: {0} ({1})", (object) talentEntry.Id,
                                (object) uint32_2);
                    }
                    else
                        break;
                }

                talentEntry.Spells = spellList.ToArray();
                talentEntry.RequiredId = (TalentId) DBCRecordConverter.GetUInt32(rawData, 13);
                talentEntry.RequiredRank = DBCRecordConverter.GetUInt32(rawData, 16);
                return talentEntry;
            }
        }
    }
}