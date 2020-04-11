using System;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Armorer
{
    public static class ArmorerMgr
    {
        private static DurabilityCost[] itemClassRepairModifiers;
        private static DurabilityQuality[] itemQualityRepairModifiers;

        private static bool ArmorerCheatChecks(Character curChar, NPC armorer, Item item)
        {
            if (item != null)
                return ArmorerMgr.ArmorerCheatChecks(curChar, armorer);
            return false;
        }

        private static bool ArmorerCheatChecks(Character curChar, NPC armorer)
        {
            if (curChar == null || armorer == null || !armorer.CheckVendorInteraction(curChar))
                return false;
            curChar.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
            return true;
        }

        private static uint GetCostToRepair(Item item)
        {
            if (item == null || item.Template == null || item.MaxDurability <= 0)
                return 0;
            uint num1 = (uint) (item.MaxDurability - item.Durability);
            if (num1 <= 0U)
                return 0;
            DurabilityCost classRepairModifier = ArmorerMgr.itemClassRepairModifiers[item.Template.Level];
            uint num2 = classRepairModifier == null
                ? 1U
                : classRepairModifier.GetModifierBySubClassId(item.Template.Class, item.Template.SubClass);
            DurabilityQuality qualityRepairModifier =
                ArmorerMgr.itemQualityRepairModifiers[(int) (item.Template.Quality + 1U) * 2];
            uint num3 = qualityRepairModifier == null ? 100U : qualityRepairModifier.CostModifierPct;
            return Math.Max(num1 * num2 * num3 / 100U, 1U);
        }

        public static void Initialize()
        {
            ListDBCReader<DurabilityCost, DBCDurabilityCostsConverter> listDbcReader1 =
                new ListDBCReader<DurabilityCost, DBCDurabilityCostsConverter>(
                    RealmServerConfiguration.GetDBCFile("DurabilityCosts.dbc"));
            ListDBCReader<DurabilityQuality, DBCDurabilityQualityConverter> listDbcReader2 =
                new ListDBCReader<DurabilityQuality, DBCDurabilityQualityConverter>(
                    RealmServerConfiguration.GetDBCFile("DurabilityQuality.dbc"));
            ArmorerMgr.itemClassRepairModifiers = listDbcReader1.EntryList.ToArray();
            ArmorerMgr.itemQualityRepairModifiers = listDbcReader2.EntryList.ToArray();
        }
    }
}