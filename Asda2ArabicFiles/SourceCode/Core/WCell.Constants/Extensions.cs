using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Util;

namespace WCell.Constants
{
	public static class Extensions
	{
		public static string ToString(this ObjectTypeId type, uint id)
		{
			string entryStr;
			if (type == ObjectTypeId.Item)
			{
				entryStr = (Asda2ItemId)id + " (" + id + ")";
			}
			else if (type == ObjectTypeId.Unit)
			{
				entryStr = (NPCId)id + " (" + id + ")";
			}
			else if (type == ObjectTypeId.GameObject)
			{
				entryStr = (GOEntryId)id + " (" + id + ")";
			}
			else
			{
				entryStr = id + " (" + id + ")";
			}
			return entryStr;
		}

		public static string ToString(this ObjectTypeId type, IEnumerable<uint> ids, string conj)
		{
			return ids.Aggregate("", (current, id) => current + (type.ToString(id) + conj));
		}

		public static string ToString(this GOEntryId id)
		{
			return id + "(Id: " + (int)id + ")";
		}

		public static ClassMask ToMask(this ClassId clss)
		{
			return (ClassMask)(1 << ((int)clss - 1));
		}

		public static ClassId[] GetIds(this ClassMask mask)
		{
			var ids = Utility.GetSetIndices((uint) mask);
			var classIds = new ClassId[ids.Length];
			for (var i = 0; i < ids.Length; i++)
			{
				var id = ids[i];
				classIds[i] = (ClassId) (id+1);
			}
			return classIds;
		}

		#region HasAnyFlag (thanks Microsoft, for giving us HasFlag, but not HasAnyFlag)
		public static bool HasAnyFlag(this UnitFlags flags, UnitFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this NPCFlags flags, NPCFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this GroupMemberFlags flags, GroupMemberFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this HitFlags flags, HitFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this MovementFlags flags, MovementFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this MonsterMoveFlags flags, MonsterMoveFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this SplineFlags flags, SplineFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this ClassMask flags, ClassMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this ClassMask flags, ClassId clss)
		{
			return (flags & (ClassMask)(1 << (int) (clss-1))) != 0;
		}

		public static bool HasAnyFlag(this  RaceMask flags, RaceMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this ClassMask2 flags, ClassMask2 otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this  RaceMask2 flags, RaceMask2 otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this  ShapeshiftMask flags, ShapeshiftMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

        public static bool HasAnyFlag(this AchievementFlags flags, AchievementFlags otherFlags)
        {
            return (flags & otherFlags) != 0;
        }

		public static ShapeshiftMask ToMask(this ShapeshiftForm form)
		{
			return (ShapeshiftMask) (1 << ((int) form - 1));
		}
		#endregion
	}
}