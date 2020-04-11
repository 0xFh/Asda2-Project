using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core.DBC;
using WCell.RealmServer.Res;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
    public class GameTables
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        [NotVariable] public static float[] Table = new float[100];

        /// <summary>
        /// The base mana regeneration modifier per level
        /// TODO: Get it from the DBCs (GtOCTRegenMP.dbc?)
        /// </summary>
        [NotVariable] private static readonly float[] BaseRegen = new float[81]
        {
            0.0f,
            0.034965f,
            0.034191f,
            0.033465f,
            0.032526f,
            0.031661f,
            0.031076f,
            0.030523f,
            0.029994f,
            0.029307f,
            0.028661f,
            0.027584f,
            0.026215f,
            0.025381f,
            0.0243f,
            0.023345f,
            0.022748f,
            0.021958f,
            0.021386f,
            0.02079f,
            0.020121f,
            0.019733f,
            0.019155f,
            0.018819f,
            0.018316f,
            0.017936f,
            0.017576f,
            0.017201f,
            0.016919f,
            0.016581f,
            0.016233f,
            0.015994f,
            0.015707f,
            0.015464f,
            0.015204f,
            0.014956f,
            0.014744f,
            0.014495f,
            0.014302f,
            0.014094f,
            0.013895f,
            36f / (965f * (float) Math.E),
            0.013522f,
            0.013363f,
            0.013175f,
            0.012996f,
            0.012853f,
            0.012687f,
            0.012539f,
            0.012384f,
            0.012233f,
            0.012113f,
            0.011973f,
            0.011859f,
            0.011714f,
            0.011575f,
            0.011473f,
            0.011342f,
            0.011245f,
            0.01111f,
            0.010999f,
            0.0107f,
            0.010522f,
            0.01029f,
            0.010119f,
            0.009968f,
            0.009808f,
            0.009651f,
            0.009553f,
            0.009445f,
            0.009327f,
            0.008859f,
            0.008415f,
            0.007993f,
            0.007592f,
            0.007211f,
            0.006849f,
            0.006506f,
            0.006179f,
            0.005869f,
            0.005575f
        };

        public static readonly float[] BaseDodge = new float[12]
        {
            0.0f,
            0.0075f,
            0.00652f,
            -0.0545f,
            -0.0059f,
            0.03183f,
            0.0114f,
            0.0167f,
            0.034575f,
            0.02011f,
            0.0f,
            -0.0187f
        };

        /// <summary>
        /// Agi/1% crit (ClassMeleeCritChance) to agility/1%dodge coefficient multipliers
        /// Divide the value from GetClassMeleeCritChanceValue(level, Id) by this.
        /// Latest intel says this is wrong.
        /// </summary>
        public static readonly float[] CritAgiMod = new float[12]
        {
            0.0f,
            0.85f,
            1f,
            1.1f,
            2f,
            1f,
            0.85f,
            1.6f,
            1f,
            1f,
            0.0f,
            2f
        };

        /// <summary>
        /// Constant used for diminishing returns indexed per class.
        /// </summary>
        public static readonly float[] DiminisherConstant = new float[12]
        {
            0.0f,
            0.956f,
            0.956f,
            0.988f,
            0.988f,
            0.983f,
            0.956f,
            0.988f,
            0.983f,
            0.983f,
            0.0f,
            0.972f
        };

        /// <summary>Stat cap constant per class</summary>
        public static readonly float[] StatCap = new float[12]
        {
            0.0f,
            88.12902f,
            88.12902f,
            145.5604f,
            145.5604f,
            150.3759f,
            88.12902f,
            145.5604f,
            150.3759f,
            150.3759f,
            0.0f,
            116.8907f
        };

        private static float[] s_baseMeleeCritChance;
        private static float[] s_baseSpellCritChance;
        private static float[] s_classMeleeCritChance;
        private static float[] s_classSpellCritChance;
        private static float[] s_barberShopCosts;
        private static float[] s_octManaRegen;
        private static float[] s_octHealthRegen;
        private static float[] s_octManaRegenPerSpirit;
        private static float[] s_octHealthRegenPerSpirit;
        private static Dictionary<CombatRating, float[]> s_combatRatings;
        public static bool Loaded;

        /// <summary>
        /// All combat ratings from WCell.Constants.Misc.CombatRating
        /// NOTE: Fields are indexed by level starting from level 1 = index0 (use level-1)
        /// </summary>
        public static Dictionary<CombatRating, float[]> CombatRatings
        {
            get { return GameTables.s_combatRatings; }
        }

        /// <summary>
        /// The base spell crit chance modifier indexed by class - 1
        /// </summary>
        public static float[] BaseSpellCritChance
        {
            get { return GameTables.s_baseSpellCritChance; }
        }

        /// <summary>
        /// The base melee crit chance modifier indexed by class - 1
        /// </summary>
        public static float[] BaseMeleeCritChance
        {
            get { return GameTables.s_baseMeleeCritChance; }
        }

        /// <summary>
        /// Spell crit modifier by level and class
        /// Used for crit per intellect and crit per crit rating
        /// </summary>
        private static float[] ClassSpellCritChance
        {
            get { return GameTables.s_classSpellCritChance; }
        }

        /// <summary>
        /// Melee crit modifier by level and class
        /// Used for crit per agility and crit per crit rating
        /// </summary>
        private static float[] ClassMeleeCritChance
        {
            get { return GameTables.s_classMeleeCritChance; }
        }

        /// <summary>Barber shop cost per level (?)</summary>
        public static float[] BarberShopCosts
        {
            get { return GameTables.s_barberShopCosts; }
        }

        /// <summary>Mana regeneration per class per level (in combat?)</summary>
        public static float[] OCTRegenMP
        {
            get { return GameTables.s_octManaRegen; }
        }

        /// <summary>Health regeneration per class per level (in combat?)</summary>
        public static float[] OCTRegenHP
        {
            get { return GameTables.s_octHealthRegen; }
        }

        /// <summary>
        /// Mana regeneration per class per level (how much spirit it takes for MP5)
        /// </summary>
        public static float[] RegenMPPerSpirit
        {
            get { return GameTables.s_octManaRegenPerSpirit; }
        }

        /// <summary>
        /// Health regeneration per class per level (how much spirit it takes for what?)
        /// </summary>
        public static float[] RegenHPPerSpirit
        {
            get { return GameTables.s_octHealthRegenPerSpirit; }
        }

        /// <summary>Get's the table from the CombatRating property.</summary>
        /// <param name="rating"></param>
        /// <returns>The combat rating table with 100 values indexed by level - 1</returns>
        public static float[] GetCRTable(CombatRating rating)
        {
            return GameTables.Table;
        }

        /// <summary>
        /// Gets the modified value from the table ClassSpellCritChance from the correct index.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns>The modified value matching the format "XX stat for 1% chance"</returns>
        public static float GetClassSpellCritChanceValue(int level, ClassId id)
        {
            return GameTables.ModifyValue(GameTables.GetValuePerRating(GameTables.ClassSpellCritChance, level, id));
        }

        /// <summary>
        /// Gets the modified value from the table ClassMeleeCritChance from the correct index.
        /// Returns the modified value matching the format "XX stat for 1% chance"
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns>The modified value matching the format "XX rating for 1% chance"</returns>
        public static float GetClassMeleeCritChanceValue(int level, ClassId id)
        {
            return GameTables.ModifyValue(GameTables.GetValuePerRating(GameTables.ClassMeleeCritChance, level, id));
        }

        public static float GetUnmodifiedClassSpellCritChanceValue(int level, ClassId id)
        {
            return GameTables.GetValuePerRating(GameTables.ClassSpellCritChance, level, id);
        }

        public static float GetUnModifiedClassMeleeCritChanceValue(int level, ClassId id)
        {
            return GameTables.GetValuePerRating(GameTables.ClassMeleeCritChance, level, id);
        }

        /// <summary>
        /// Modifies the values from ClassSpellCritChance, ClassMeleeCritChance
        /// to match the format of "XX stat for 1% chance"
        /// TODO: Phase out use for optimization (use the unmodified value)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float ModifyValue(float value)
        {
            return (float) (1.0 / ((double) value * 100.0));
        }

        /// <summary>
        /// Gets the real value from the table (unmodified)
        /// NOTE: Only for ClassSpellCritChance, ClassMeleeCritChance
        /// Will return the wrong values if used incorrectly.
        /// </summary>
        /// <param name="table">The table (ClassSpellCritChance, ClassMeleeCritChance)</param>
        /// <param name="level">Level of the character</param>
        /// <param name="classId">ClassId of the character</param>
        /// <returns></returns>
        private static float GetValuePerRating(float[] table, int level, ClassId classId)
        {
            if (level > 100)
                level = 100;
            if (level < 1)
                level = 1;
            return table[100 * (int) classId + level - 101];
        }

        private static bool LoadRatingChanceDBC(string file, out float[] vals)
        {
            vals = new float[0];
            string dbcFile = RealmServerConfiguration.GetDBCFile(file);
            if (!File.Exists(dbcFile))
            {
                GameTables.s_log.Error(string.Format(WCell_RealmServer.DBCFileDoesntExist, (object) file));
                return false;
            }

            ListDBCReader<float, GameTableConverter> listDbcReader =
                new ListDBCReader<float, GameTableConverter>(dbcFile);
            vals = new float[listDbcReader.EntryList.Count];
            for (int index = 0; index < vals.Length; ++index)
                vals[index] = listDbcReader.EntryList[index];
            return true;
        }

        private static bool LoadGtBaseSpellCritChanceDBC()
        {
            return GameTables.LoadRatingChanceDBC("gtChanceToSpellCritBase.dbc", out GameTables.s_baseSpellCritChance);
        }

        private static bool LoadGtClassSpellCritChanceDBC()
        {
            return GameTables.LoadRatingChanceDBC("gtChanceToSpellCrit.dbc", out GameTables.s_classSpellCritChance);
        }

        private static bool LoadGtBaseMeleeCritChanceDBC()
        {
            return GameTables.LoadRatingChanceDBC("gtChanceToMeleeCritBase.dbc", out GameTables.s_baseMeleeCritChance);
        }

        private static bool LoadGtClassMeleeCritChanceDBC()
        {
            return GameTables.LoadRatingChanceDBC("gtChanceToMeleeCrit.dbc", out GameTables.s_classMeleeCritChance);
        }

        private static bool LoadGtClassHealthRegenPerSpiritDBC()
        {
            return GameTables.LoadRatingChanceDBC("GtRegenHPPerSpt.dbc", out GameTables.s_octHealthRegenPerSpirit);
        }

        private static bool LoadGtClassManaRegenPerSpiritDBC()
        {
            return GameTables.LoadRatingChanceDBC("GtRegenMPPerSpt.dbc", out GameTables.s_octManaRegenPerSpirit);
        }

        private static bool LoadGtClassOCTHealthRegenDBC()
        {
            return GameTables.LoadRatingChanceDBC("GtOCTRegenHP.dbc", out GameTables.s_octHealthRegen);
        }

        private static bool LoadGtClassOCTManaRegenDBC()
        {
            return GameTables.LoadRatingChanceDBC("GtOCTRegenMP.dbc", out GameTables.s_octManaRegen);
        }

        private static bool LoadGtBarberShopCostDBC(out float[] vals)
        {
            ListDBCReader<float, GameTableConverter> listDbcReader =
                new ListDBCReader<float, GameTableConverter>(
                    RealmServerConfiguration.GetDBCFile("gtBarberShopCostBase.dbc"));
            vals = new float[listDbcReader.EntryList.Count];
            for (int index = 0; index < vals.Length; ++index)
                vals[index] = listDbcReader.EntryList[index];
            return true;
        }

        private static bool LoadGtCombatRatingsDBC(out Dictionary<CombatRating, float[]> combatRatings)
        {
            combatRatings = new Dictionary<CombatRating, float[]>();
            string dbcFile = RealmServerConfiguration.GetDBCFile("gtCombatRatings.dbc");
            if (!File.Exists(dbcFile))
            {
                GameTables.s_log.Error(string.Format(WCell_RealmServer.DBCFileDoesntExist, (object) dbcFile));
                return false;
            }

            ListDBCReader<float, GameTableConverter> listDbcReader =
                new ListDBCReader<float, GameTableConverter>(dbcFile);
            for (int index1 = 1; index1 < 25; ++index1)
            {
                combatRatings[(CombatRating) index1] = new float[100];
                for (int index2 = (index1 - 1) * 100; index2 < index1 * 100; ++index2)
                    combatRatings[(CombatRating) index1][index2 - (index1 - 1) * 100] = listDbcReader.EntryList[index2];
            }

            return true;
        }

        /// <summary>Loads the DBC file starting with gtXXXX.dbc</summary>
        /// <returns>Wether all gametables were loaded</returns>
        public static bool LoadGtDBCs()
        {
            if (!GameTables.LoadGtBaseSpellCritChanceDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed,
                    (object) "gtChanceToSpellCritBase.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassSpellCritChanceDBC())
            {
                GameTables.s_log.Info(
                    string.Format(WCell_RealmServer.DBCLoadFailed, (object) "gtChanceToSpellCrit.dbc"));
                return false;
            }

            if (!GameTables.LoadGtBaseMeleeCritChanceDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed,
                    (object) "gtChanceToMeleeCritBase.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassMeleeCritChanceDBC())
            {
                GameTables.s_log.Info(
                    string.Format(WCell_RealmServer.DBCLoadFailed, (object) "gtChanceToMeleeCrit.dbc"));
                return false;
            }

            if (!GameTables.LoadGtBarberShopCostDBC(out GameTables.s_barberShopCosts))
            {
                GameTables.s_log.Info(
                    string.Format(WCell_RealmServer.DBCLoadFailed, (object) "gtChanceToMeleeCrit.dbc"));
                return false;
            }

            if (!GameTables.LoadGtCombatRatingsDBC(out GameTables.s_combatRatings))
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed, (object) "gtCombatRatings.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassHealthRegenPerSpiritDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed, (object) "GtRegenHPPerSpt.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassManaRegenPerSpiritDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed, (object) "GtRegenMPPerSpt.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassOCTHealthRegenDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed, (object) "GtOCTRegenHP.dbc"));
                return false;
            }

            if (!GameTables.LoadGtClassOCTManaRegenDBC())
            {
                GameTables.s_log.Info(string.Format(WCell_RealmServer.DBCLoadFailed, (object) "GtOCTRegenMP.dbc"));
                return false;
            }

            GameTables.Loaded = true;
            return true;
        }

        public static float GetBaseRegenForLevel(int level)
        {
            if (level < GameTables.BaseRegen.Length)
                return GameTables.BaseRegen[level];
            return GameTables.BaseRegen[GameTables.BaseRegen.Length - 1];
        }
    }
}