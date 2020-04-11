using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Formulas
{
    /// <summary>
    /// Takes Target-level and receiver-level and returns the amount of base-experience to be gained
    /// </summary>
    public delegate int BaseExperienceCalculator(int targetLvl, int receiverLvl);

    public delegate int ExperienceCalculator(int receiverLvl, NPC npc);

    /// <summary>
    /// </summary>
    /// <returns>The amount of tequired Experience for that level</returns>
    public delegate int XpCalculator(int level);

    //[DataHolder]
    public class LevelXp : IDataHolder
    {
        public int Level, Xp;

        public uint GetId()
        {
            return (uint)Level;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            // ArrayUtil.Set(ref XpGenerator.XpTableForNexlLvl, (uint) Level, Xp);
        }
    }

    /// <summary>
    /// Static utility class that holds and calculates Level- and Experience-information.
    /// Has exchangable Calculator delegates to allow custom xp-calculations.
    /// </summary>
    public static class XpGenerator
    {
        public static float XpRate = 1f;
        /// <summary>
        /// Change this method in addons to create custom XP calculation
        /// </summary>
        public static Action<Character, INamed, int> CombatXpDistributer = DistributeCombatXp;

        /// <summary>
        /// Distributes the given amount of XP over the group of the given Character (or adds it only to the Char, if not in Group).
        /// </summary>
        /// <remarks>Requires Map-Context.</remarks>
        /// <param name="chr"></param>
        public static void DistributeCombatXp(Character chr, INamed killed, int xp)
        {
            if (chr.SoulmateRecord != null)
            {
                chr.SoulmateRecord.OnExpGained(true);
            }
            var groupMember = chr.GroupMember;
            if (groupMember != null)
            {
                var members = new List<Character>();
                var highestLevel = 0;
                var totalLevels = 0;
                groupMember.IterateMembersInRange(WorldObject.BroadcastRange,
                                                  member =>
                                                  {
                                                      var memberChar = member.Character;
                                                      if (memberChar != null)
                                                      {
                                                          totalLevels += memberChar.Level;
                                                          if (memberChar.Level > highestLevel)
                                                          {
                                                              highestLevel = memberChar.Level;
                                                          }
                                                          members.Add(memberChar);
                                                      }
                                                  });

                foreach (var member in members)
                {
                    var share = MathUtil.Divide(xp * member.Level, totalLevels);
                    member.GainCombatXp(share, killed, true);
                }
            }
            else
            {
                chr.GainCombatXp(xp, killed, true);
            }
        }

        /// <summary>
        /// Gets the amount of xp, required to gain this level (from level-1)
        /// </summary>
        public static int GetXpForlevel(int level)
        {
            if (XpTableForNexlLvl.Length >= level)
            {
                return XpTableForNexlLvl[level - 1];
            }
            return 0;
        }

        public static int GetStartXpForLevel(int lvl) //98 Level MAX
        {
            return XpTableLvl[lvl - 1];
        }

        [NotVariable] /// <summary>
        /// Array of Xp to be gained per level for default levels.
        /// Can be set to a different Array.
        /// </summary>
        public static int[] XpTableLvl = new int[] { 0, 40, 150, 401, 865, 1615, 2725, 4270, 6326, 8970, 12293, 16387, 21349, 27870, 35564, 44546, 54932, 66842, 80398, 95726, 113034, 132539, 154464, 179039, 206503, 237101, 271086, 308718, 350264, 400572, 461713, 534854, 620995, 724136, 847277, 995418, 1173559, 1386700, 1639841, 1952982, 2336123, 2799264, 3352405, 4005546, 4808687, 5811828, 7064969, 8618110, 10494105, 13260405, 17106653, 21502900, 26449148, 31945395, 37991643, 44196133, 51637984, 59611296, 68135476, 77217132, 86864452, 97085680, 107889116, 119283112, 131276078, 143876478, 157092830, 170933707, 185407740, 200523610, 217151067, 235441269, 255560491, 277691636, 302035895, 328814580, 358271134, 390673343, 426315773, 465522446, 508649787, 556089862, 608273944, 665676435, 728819175, 798276189, 874678904, 958721891, 1051169177, 1152861191, 1269807007, 1404294696, 1558955538, 1736815507, 1941354471 ,2000000000}; //, 2186801228, 2481337337, 2834780667, 3258912663, 3767871059 };
        //public static int[] XpTableLvl = new int[] { 0, 40, 150, 401, 865, 1615, 2725, 4270, 6326, 8970, 12293, 16387, 21349, 27870, 35564, 44546, 54932, 66842, 80398, 95726, 113034, 132539, 154464, 179039, 206503, 237101, 271086, 308718, 350264, 400572, 461713, 534854, 620995, 724136, 847277, 995418, 1173559, 1386700, 1639841, 1952982, 2336123, 2799264, 3352405, 4005546, 4808687, 5811828, 7064969, 8618110, 10494105, 13260405, 17106653, 21502900, 26449148, 31945395, 37991643, 44196133, 51637984, 59611296, 68135476, 77217132, 86864452, 97085680, 107889116, 119283112, 131276078, 143876478, 157092830, 170933707, 185407740, 200523610, 216293609, 233049570, 251125163, 270855897, 292579121, 316634018, 343361612, 412119223, 489759078, 573627715, 664281276, 762280402, 868190238, 982580430, 1106025126, 1239102976, 1382397131, 1536495244, 1701989471, 1879476469, 2070133961 };
        [NotVariable]
        /// <summary>
        /// Array of Xp to be gained per level for default levels.
        /// Can be set to a different Array.
        /// </summary>
        public static int[] XpTableForNexlLvl = { 0, 40, 110, 251, 464, 750, 1110, 1545, 2056, 2644, 3323, 4094, 4962, 6521, 7694, 8982, 10386, 11910, 13556, 15328, 17308, 19505, 21925, 24575, 27464, 30598, 33985, 37632, 41546, 50308, 61141, 73141, 86141, 103141, 123141, 148141, 178141, 213141, 253141, 313141, 383141, 463141, 553141, 653141, 803141, 1003141, 1253141, 1553141, 1875995, 2766300, 3846248, 4396247, 4946248, 5496247, 6046248, 6204490, 7441851, 7973312, 8524180, 9081656, 9647320, 10221228, 10803436, 11393996, 11992966, 12600400, 13216352, 13840877, 14474033, 15115870, 16627457, 18290202, 20119222, 22131145, 24344259, 26778685, 29456554, 32402209, 35642430, 39206673, 43127341, 47440075, 52184082, 57402491, 63142740, 69457014, 76402715, 84042987, 92447286, 101692014, 116945816, 134487689, 154660842, 177859969, 204538964,int.MaxValue }; //, 245446757, 294536109, 353443330, 424131996, 508958396 };
        //public static int[] XpTableForNexlLvl = new int[] { 0, 40, 110, 251, 464, 750, 1110, 1545, 2056, 2644, 3323, 4094, 4962, 6521, 7694, 8982, 10386, 11910, 13556, 15328, 17308, 19505, 21925, 24575, 27464, 30598, 33985, 37632, 41546, 50308, 61141, 73141, 86141, 103141, 123141, 148141, 178141, 213141, 253141, 313141, 383141, 463141, 553141, 653141, 803141, 1003141, 1253141, 1553141, 1875995, 2766300, 3846248, 4396247, 4946248, 5496247, 6046248, 6204490, 7441851, 7973312, 8524180, 9081656, 9647320, 10221228, 10803436, 11393996, 11992966, 12600400, 13216352, 13840877, 14474033, 15115870, 15769999, 16755961, 18075593, 19730734, 21723224, 24054897, 26727594, 68757611, 77639855, 83868637, 90653561, 97999126, 105909836, 114390192, 123444696, 133077850, 143294155, 154098113, 165494227, 177486998, 190657492 };

        public static int[] BaseExpForLvl = { 0, 2, 5, 9, 15, 19, 23, 26, 30, 34, 37, 41, 46, 55, 60, 65, 70, 75, 80, 86, 92, 98, 105, 112, 120, 128, 136, 145, 154, 168, 186, 204, 221, 246, 274, 309, 350, 395, 445, 522, 609, 702, 802, 908, 1071, 1287, 1548, 1849, 2157, 3007, 3966, 4311, 4623, 4908, 5168, 5086, 5860, 6041, 6223, 6396, 6563, 6725, 6882, 7034, 7182, 7326, 7467, 7605, 7741, 7558, 7807, 8094, 8419, 8783, 9187, 9633, 10123, 10659, 11244, 11881, 12574, 13326, 14143, 15027, 15986, 17024, 18148, 19365, 20682, 21637, 23722, 26064, 28695, 31648, 34964 };
        public static int GetBaseExpForLevel(int level)
        {
            var baseXp = 40000;
            if (BaseExpForLvl.Length > level)
                baseXp = BaseExpForLvl[level];
            return (int)(baseXp * XpRate);
        }

        public static int CalcDefaultXp(int receiverlvl, NPC npc)
        {
            var baseXp = GetBaseExpForLevel(npc.Level);
            var boost = 1f;
            switch (npc.Entry.Rank)
            {
                case CreatureRank.Boss:
                    boost = 30f;
                    break;
                case CreatureRank.Elite:
                    boost = 4f;
                    break;
                case CreatureRank.WorldBoss:
                    boost = 150f;
                    break;
                case CreatureRank.Normal:
                    boost = 1f;
                    break;
            }
            var lvlDiff = receiverlvl - npc.Level;
            switch (lvlDiff)
            {
                case 1:
                    boost *= 0.99f;
                    break;
                case 2:
                    boost *= 0.95f;
                    break;
                case 3:
                    boost *= 0.9f;
                    break;
                case 4:
                    boost *= 0.85f;
                    break;
                case 5:
                    boost *= 0.8f;
                    break;
                case 6:
                    boost *= 0.01f;
                    break;
            }
            return (int)(baseXp * boost);
        }
    }
}