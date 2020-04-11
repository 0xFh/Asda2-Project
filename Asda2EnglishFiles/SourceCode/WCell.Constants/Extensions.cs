using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Util;

namespace WCell.Constants
{
    public static class Extensions
    {
        public static string ToString(this ObjectTypeId type, uint id)
        {
            string str;
            switch (type)
            {
                case ObjectTypeId.Item:
                    str = ((int) id).ToString() + " (" + (object) id + ")";
                    break;
                case ObjectTypeId.Unit:
                    str = ((int) id).ToString() + " (" + (object) id + ")";
                    break;
                case ObjectTypeId.GameObject:
                    str = ((int) id).ToString() + " (" + (object) id + ")";
                    break;
                default:
                    str = ((int) id).ToString() + " (" + (object) id + ")";
                    break;
            }

            return str;
        }

        public static string ToString(this ObjectTypeId type, IEnumerable<uint> ids, string conj)
        {
            return ids.Aggregate<uint, string>("",
                (Func<string, uint, string>) ((current, id) => current + type.ToString(id) + conj));
        }

        public static string ToString(this GOEntryId id)
        {
            return ((int) id).ToString() + "(Id: " + (object) (int) id + ")";
        }

        public static ClassMask ToMask(this ClassId clss)
        {
            return (ClassMask) (1 << (int) (clss - 1U & (ClassId) 31));
        }

        public static ClassId[] GetIds(this ClassMask mask)
        {
            uint[] setIndices = Utility.GetSetIndices((uint) mask);
            ClassId[] classIdArray = new ClassId[setIndices.Length];
            for (int index = 0; index < setIndices.Length; ++index)
            {
                uint num = setIndices[index];
                classIdArray[index] = (ClassId) ((int) num + 1);
            }

            return classIdArray;
        }

        public static bool HasAnyFlag(this UnitFlags flags, UnitFlags otherFlags)
        {
            return (flags & otherFlags) != UnitFlags.None;
        }

        public static bool HasAnyFlag(this NPCFlags flags, NPCFlags otherFlags)
        {
            return (flags & otherFlags) != NPCFlags.None;
        }

        public static bool HasAnyFlag(this GroupMemberFlags flags, GroupMemberFlags otherFlags)
        {
            return (flags & otherFlags) != GroupMemberFlags.Normal;
        }

        public static bool HasAnyFlag(this HitFlags flags, HitFlags otherFlags)
        {
            return (flags & otherFlags) != HitFlags.NormalSwing;
        }

        public static bool HasAnyFlag(this MovementFlags flags, MovementFlags otherFlags)
        {
            return (flags & otherFlags) != MovementFlags.None;
        }

        public static bool HasAnyFlag(this MonsterMoveFlags flags, MonsterMoveFlags otherFlags)
        {
            return (flags & otherFlags) != MonsterMoveFlags.None;
        }

        public static bool HasAnyFlag(this SplineFlags flags, SplineFlags otherFlags)
        {
            return (flags & otherFlags) != SplineFlags.None;
        }

        public static bool HasAnyFlag(this ClassMask flags, ClassMask otherFlags)
        {
            return (flags & otherFlags) != ClassMask.None;
        }

        public static bool HasAnyFlag(this ClassMask flags, ClassId clss)
        {
            return (flags & (ClassMask) (1 << (int) (clss - 1U & (ClassId) 31))) != ClassMask.None;
        }

        public static bool HasAnyFlag(this RaceMask flags, RaceMask otherFlags)
        {
            return (flags & otherFlags) != ~RaceMask.AllRaces1;
        }

        public static bool HasAnyFlag(this ClassMask2 flags, ClassMask2 otherFlags)
        {
            return (flags & otherFlags) != ClassMask2.None;
        }

        public static bool HasAnyFlag(this RaceMask2 flags, RaceMask2 otherFlags)
        {
            return (flags & otherFlags) != RaceMask2.None;
        }

        public static bool HasAnyFlag(this ShapeshiftMask flags, ShapeshiftMask otherFlags)
        {
            return (flags & otherFlags) != ShapeshiftMask.None;
        }

        public static bool HasAnyFlag(this AchievementFlags flags, AchievementFlags otherFlags)
        {
            return (flags & otherFlags) != (AchievementFlags) 0;
        }

        public static ShapeshiftMask ToMask(this ShapeshiftForm form)
        {
            return (ShapeshiftMask) (1 << (int) (form - 1 & ShapeshiftForm.Moonkin));
        }
    }
}