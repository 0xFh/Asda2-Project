using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Variables;

namespace WCell.RealmServer.GameObjects
{
    /// <summary>
    /// The Selection Manager keeps track of all GOs that have been selected by Staff members
    /// </summary>
    public class GOSelectMgr
    {
        [NotVariable] public static SpellId MarkerId = SpellId.ABOUTTOSPAWN;
        [NotVariable] public static float MarkerRadius = 8f;
        [NotVariable] public static float MaxSearchRadius = 20f;
        [NotVariable] public static float MinSearchAngle = 3.141593f;
        public static readonly GOSelectMgr Instance = new GOSelectMgr();

        private GOSelectMgr()
        {
        }

        /// <summary>
        /// Tries to select the nearest GO that is in front of the character
        /// </summary>
        /// <returns>The newly selected GO.</returns>
        public GameObject SelectClosest(Character chr)
        {
            IList<WorldObject> objectsInRadius =
                chr.GetObjectsInRadius<Character>(GOSelectMgr.MaxSearchRadius, ObjectTypes.GameObject, true, 0);
            float num = float.MaxValue;
            GameObject gameObject1 = (GameObject) null;
            foreach (GameObject gameObject2 in (IEnumerable<WorldObject>) objectsInRadius)
            {
                float distanceSq = chr.GetDistanceSq((WorldObject) gameObject2);
                if (gameObject1 == null ||
                    gameObject2.IsInFrontOf((WorldObject) chr) && (double) distanceSq < (double) num)
                {
                    gameObject1 = gameObject2;
                    num = distanceSq;
                }
            }

            this[chr] = gameObject1;
            return gameObject1;
        }

        /// <summary>Sets the Character's selected GameObject</summary>
        internal GameObject this[Character chr]
        {
            get { return chr.ExtraInfo.SelectedGO; }
            set
            {
                ExtraInfo extraInfo = chr.ExtraInfo;
                this.Deselect(extraInfo);
                if (value == null)
                    return;
                GOSelection selection = new GOSelection(value);
                if (GOSelectMgr.MarkerId != SpellId.None)
                {
                    DynamicObject marker = new DynamicObject((Unit) chr, GOSelectMgr.MarkerId, GOSelectMgr.MarkerRadius,
                        value.Map, value.Position);
                    selection.Marker = marker;
                    marker.CallPeriodically(2000, (Action<WorldObject>) (obj =>
                    {
                        if (chr.IsInWorld && chr.Map == marker.Map && (selection.GO != null && selection.GO.IsInWorld))
                            return;
                        marker.Delete();
                    }));
                }

                extraInfo.m_goSelection = selection;
            }
        }

        /// <summary>Deselects the given Character's current GO</summary>
        internal void Deselect(ExtraInfo info)
        {
            GOSelection goSelection = info.m_goSelection;
            if (goSelection == null)
                return;
            goSelection.Dispose();
            info.m_goSelection = (GOSelection) null;
        }
    }
}