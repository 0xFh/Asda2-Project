using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class AchievementCriteriaRequirementSet
    {
        public readonly List<AchievementCriteriaRequirementCreator> Requirements =
            new List<AchievementCriteriaRequirementCreator>();

        public readonly uint CriteriaId;

        public AchievementCriteriaRequirementSet(uint id)
        {
            this.CriteriaId = id;
        }

        public void Add(AchievementCriteriaRequirement requirement)
        {
            this.Requirements.Add(AchievementMgr.GetCriteriaRequirementCreator(requirement.Type));
        }

        public bool Meets(Character chr, Unit involved, uint miscValue)
        {
            using (List<AchievementCriteriaRequirementCreator>.Enumerator enumerator = this.Requirements.GetEnumerator()
            )
            {
                if (enumerator.MoveNext())
                    return enumerator.Current().Meets(chr, involved, miscValue);
            }

            return true;
        }
    }
}