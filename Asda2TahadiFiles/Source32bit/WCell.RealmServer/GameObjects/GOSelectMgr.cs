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
    [NotVariable]public static SpellId MarkerId = SpellId.ABOUTTOSPAWN;
    [NotVariable]public static float MarkerRadius = 8f;
    [NotVariable]public static float MaxSearchRadius = 20f;
    [NotVariable]public static float MinSearchAngle = 3.141593f;
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
        chr.GetObjectsInRadius(MaxSearchRadius, ObjectTypes.GameObject, true, 0);
      float num = float.MaxValue;
      GameObject gameObject1 = null;
      foreach(GameObject gameObject2 in objectsInRadius)
      {
        float distanceSq = chr.GetDistanceSq(gameObject2);
        if(gameObject1 == null ||
           gameObject2.IsInFrontOf(chr) && distanceSq < (double) num)
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
        Deselect(extraInfo);
        if(value == null)
          return;
        GOSelection selection = new GOSelection(value);
        if(MarkerId != SpellId.None)
        {
          DynamicObject marker = new DynamicObject(chr, MarkerId, MarkerRadius,
            value.Map, value.Position);
          selection.Marker = marker;
          marker.CallPeriodically(2000, obj =>
          {
            if(chr.IsInWorld && chr.Map == marker.Map && (selection.GO != null && selection.GO.IsInWorld))
              return;
            marker.Delete();
          });
        }

        extraInfo.m_goSelection = selection;
      }
    }

    /// <summary>Deselects the given Character's current GO</summary>
    internal void Deselect(ExtraInfo info)
    {
      GOSelection goSelection = info.m_goSelection;
      if(goSelection == null)
        return;
      goSelection.Dispose();
      info.m_goSelection = null;
    }
  }
}