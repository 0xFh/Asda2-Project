using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
  public static class Targeting
  {
    /// <summary>
    /// Iterates over all objects within the given radius around this object.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="predicate">Returns whether to continue iteration.</param>
    /// <returns>True, if iteration should continue (usually indicating that we did not find what we were looking for).</returns>
    public static bool IterateEnvironment(this IWorldLocation location, float radius,
      Func<WorldObject, bool> predicate)
    {
      return location.Map.IterateObjects(location.Position, radius, location.Phase, predicate);
    }

    /// <summary>
    /// Iterates over all objects of the given Type within the given radius around this object.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="predicate">Returns whether to continue iteration.</param>
    /// <returns>True, if iteration should continue (usually indicating that we did not find what we were looking for).</returns>
    public static bool IterateEnvironment<O>(this IWorldLocation location, float radius, Func<O, bool> predicate)
      where O : WorldObject
    {
      return location.Map.IterateObjects(location.Position, radius, location.Phase,
        obj =>
        {
          if(obj is O)
            return predicate((O) obj);
          return true;
        });
    }

    /// <summary>Returns all objects in radius</summary>
    public static IList<WorldObject> GetObjectsInRadius<O>(this O wObj, float radius, ObjectTypes filter,
      bool checkVisible, int limit = 2147483647) where O : WorldObject
    {
      if(wObj.Map == null)
        return WorldObject.EmptyArray;
      IList<WorldObject> objectsInRadius;
      if(checkVisible)
      {
        Func<WorldObject, bool> filter1 = obj =>
        {
          if(obj.CheckObjType(filter))
            return wObj.CanSee(obj);
          return false;
        };
        objectsInRadius = wObj.Map.GetObjectsInRadius(wObj.Position, radius, filter1, wObj.Phase, limit);
      }
      else
        objectsInRadius = wObj.Map.GetObjectsInRadius(wObj.Position, radius, filter, wObj.Phase, limit);

      return objectsInRadius;
    }

    public static IList<WorldObject> GetVisibleObjectsInRadius<O>(this O obj, float radius, ObjectTypes filter,
      int limit = 2147483647) where O : WorldObject
    {
      return obj.GetObjectsInRadius(radius, filter, true, limit);
    }

    public static IList<WorldObject> GetVisibleObjectsInRadius<O>(this O obj, float radius,
      Func<WorldObject, bool> filter, int limit) where O : WorldObject
    {
      if(obj.Map == null)
        return WorldObject.EmptyArray;
      Func<WorldObject, bool> filter1 = otherObj =>
      {
        if(filter(otherObj))
          return obj.CanSee(otherObj);
        return false;
      };
      return obj.Map.GetObjectsInRadius(obj.Position, radius, filter1, obj.Phase, limit);
    }

    public static IList<WorldObject> GetVisibleObjectsInUpdateRadius<O>(this O obj, ObjectTypes filter)
      where O : WorldObject
    {
      return obj.GetVisibleObjectsInRadius(WorldObject.BroadcastRange, filter, 0);
    }

    public static IList<WorldObject> GetVisibleObjectsInUpdateRadius<O>(this O obj, Func<WorldObject, bool> filter)
      where O : WorldObject
    {
      return obj.GetVisibleObjectsInRadius(WorldObject.BroadcastRange, filter, 0);
    }

    /// <summary>
    /// Gets all clients in update-radius that can see this object
    /// </summary>
    public static ICollection<IRealmClient> GetNearbyClients<O>(this O obj, bool includeSelf) where O : WorldObject
    {
      return obj.GetNearbyClients(WorldObject.BroadcastRange, includeSelf);
    }

    /// <summary>Gets all clients that can see this object</summary>
    public static ICollection<IRealmClient> GetNearbyClients<O>(this O obj, float radius, bool includeSelf)
      where O : WorldObject
    {
      if(obj.Map == null || !obj.IsAreaActive)
        return RealmClient.EmptyArray;
      Func<Character, bool> filter = otherObj =>
      {
        if(!otherObj.CanSee(obj))
          return false;
        if(!includeSelf)
          return (object) obj != otherObj;
        return true;
      };
      return obj.Map
        .GetObjectsInRadius(obj.Position, radius, filter, obj.Phase, int.MaxValue)
        .TransformList(chr => chr.Client);
    }

    /// <summary>Gets all characters that can see this object</summary>
    public static ICollection<Character> GetNearbyCharacters<O>(this O obj) where O : WorldObject
    {
      return obj.GetNearbyCharacters(WorldObject.BroadcastRange, true);
    }

    /// <summary>Gets all characters that can see this object</summary>
    public static ICollection<Character> GetNearbyCharacters<O>(this O obj, bool includeSelf) where O : WorldObject
    {
      return obj.GetNearbyCharacters(WorldObject.BroadcastRange, includeSelf);
    }

    /// <summary>Gets all characters that can see this object</summary>
    public static ICollection<Character> GetNearbyCharacters<O>(this O obj, float radius, bool includeSelf = true)
      where O : WorldObject
    {
      if(obj.Map == null || obj.AreaCharCount <= 0)
        return Character.EmptyArray;
      Func<Character, bool> filter = otherObj =>
      {
        if(!otherObj.CanSee(obj))
          return false;
        if(obj == otherObj)
          return includeSelf;
        return true;
      };
      return obj.Map.GetObjectsInRadius(obj.Position, radius, filter, obj.Phase, int.MaxValue);
    }

    /// <summary>Gets all Horde players in the given radius.</summary>
    public static ICollection<Character> GetNearbyHordeCharacters<O>(this O obj, float radius) where O : WorldObject
    {
      if(obj.Map != null)
        return obj.Map.GetObjectsInRadius(obj.Position, radius,
          (Func<Character, bool>) (otherObj => otherObj.FactionGroup == FactionGroup.Horde), obj.Phase,
          int.MaxValue);
      return Character.EmptyArray;
    }

    /// <summary>Gets all alliance players in the given radius.</summary>
    public static ICollection<Character> GetNearbyAllianceCharacters<O>(this O obj, float radius)
      where O : WorldObject
    {
      if(obj.Map != null)
        return obj.Map.GetObjectsInRadius(obj.Position, radius,
          (Func<Character, bool>) (otherObj => otherObj.FactionGroup == FactionGroup.Alliance), obj.Phase,
          int.MaxValue);
      return Character.EmptyArray;
    }

    /// <summary>
    /// Gets all units that are at least neutral with it who can see this object
    /// </summary>
    public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj) where O : WorldObject
    {
      return obj.GetNearbyAtLeastNeutralUnits(WorldObject.BroadcastRange, true);
    }

    /// <summary>
    /// Gets all units that are at least neutral with it who can see this object
    /// </summary>
    public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj, bool includeSelf)
      where O : WorldObject
    {
      return obj.GetNearbyAtLeastNeutralUnits(WorldObject.BroadcastRange, includeSelf);
    }

    /// <summary>
    /// Gets all units that are at least neutral with it who can see this object
    /// </summary>
    public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj, float radius,
      bool includeSelf = true) where O : WorldObject
    {
      if(obj.Map == null || obj.AreaCharCount <= 0)
        return new Unit[0];
      Func<Unit, bool> filter = otherObj =>
      {
        if(otherObj.CanSee(obj) && (obj != otherObj || includeSelf))
          return obj.IsAtLeastNeutralWith(otherObj);
        return false;
      };
      return obj.Map.GetObjectsInRadius(obj.Position, radius, filter, obj.Phase, int.MaxValue);
    }

    public static GameObject GetNearbyGO<O>(this O wObj, GOEntryId id) where O : WorldObject
    {
      return wObj.GetNearbyGO(id, WorldObject.BroadcastRange);
    }

    public static GameObject GetNearbyGO<O>(this O wObj, GOEntryId id, float radius) where O : WorldObject
    {
      GameObject go = null;
      wObj.IterateEnvironment(radius, obj =>
      {
        if(!wObj.CanSee(obj) || !(obj is GameObject) || ((GameObject) obj).Entry.GOId != id)
          return true;
        go = (GameObject) obj;
        return false;
      });
      return go;
    }

    public static NPC GetNearbyNPC<O>(this O wObj, NPCId id) where O : WorldObject
    {
      return wObj.GetNearbyNPC(id, WorldObject.BroadcastRange);
    }

    public static NPC GetNearbyNPC<O>(this O wObj, NPCId id, float radius) where O : WorldObject
    {
      NPC npc = null;
      wObj.IterateEnvironment(radius, obj =>
      {
        if(!wObj.CanSee(obj) || !(obj is NPC) || ((NPC) obj).Entry.NPCId != id)
          return true;
        npc = (NPC) obj;
        return false;
      });
      return npc;
    }

    /// <summary>
    /// Gets a random nearby Character in WorldObject.BroadcastRange who is alive and visible.
    /// </summary>
    public static Character GetNearbyRandomHostileCharacter<O>(this O wObj) where O : WorldObject
    {
      return wObj.GetNearbyRandomHostileCharacter(WorldObject.BroadcastRange);
    }

    /// <summary>
    /// Gets a random nearby Character in WorldObject.BroadcastRange who is alive and visible.
    /// </summary>
    public static Character GetNearbyRandomHostileCharacter<O>(this O wObj, float radius) where O : WorldObject
    {
      if(wObj.AreaCharCount == 0)
        return null;
      if(radius > (double) WorldObject.BroadcastRange)
      {
        LogManager.GetCurrentClassLogger()
          .Warn("Called GetNearbyRandomHostileCharacter with radius = {0} > BroadcastRange = {1}",
            radius, WorldObject.BroadcastRange);
        return null;
      }

      Character chr = null;
      int r = Utility.Random(0, wObj.AreaCharCount);
      int i = 0;
      float radiusSq = radius * radius;
      wObj.IterateEnvironment(WorldObject.BroadcastRange, obj =>
      {
        if(obj is Character)
        {
          if(!wObj.CanSee(obj) || !((Unit) obj).IsAlive ||
             (!wObj.IsHostileWith(obj) || !wObj.IsInRadiusSq(obj, radiusSq)))
          {
            --r;
            return true;
          }

          chr = (Character) obj;
          ++i;
        }

        return i != r;
      });
      return chr;
    }

    /// <summary>
    /// Returns the Unit that is closest within the given Radius around this Object
    /// TODO: Should add visibility test?
    /// </summary>
    public static Unit GetNearestUnit(this IWorldLocation wObj, float radius)
    {
      Unit unit = null;
      float sqDist = float.MaxValue;
      wObj.IterateEnvironment(radius, (Func<Unit, bool>) (obj =>
      {
        float distanceSq = obj.GetDistanceSq(wObj);
        if(distanceSq < (double) sqDist)
        {
          sqDist = distanceSq;
          unit = obj;
        }

        return true;
      }));
      return unit;
    }

    /// <summary>
    /// Returns the Unit that is closest within the given Radius around this Object and passes the filter
    /// </summary>
    public static Unit GetNearestUnit(this IWorldLocation wObj, Func<Unit, bool> filter)
    {
      return wObj.GetNearestUnit(WorldObject.BroadcastRange, filter);
    }

    /// <summary>
    /// Returns the Unit that is closest within the given Radius around this Object and passes the filter
    /// </summary>
    public static Unit GetNearestUnit(this IWorldLocation wObj, float radius, Func<Unit, bool> filter)
    {
      Unit target = null;
      float sqDist = float.MaxValue;
      wObj.IterateEnvironment(radius, (Func<Unit, bool>) (unit =>
      {
        if(filter(unit))
        {
          float distanceSq = unit.GetDistanceSq(wObj);
          if(distanceSq < (double) sqDist)
          {
            sqDist = distanceSq;
            target = unit;
          }
        }

        return true;
      }));
      return target;
    }

    public static Unit GetRandomUnit<O>(this O wObj, float radius, bool checkVisible = true) where O : WorldObject
    {
      return (Unit) wObj.GetObjectsInRadius(radius, ObjectTypes.Unit, checkVisible, 0)
        .GetRandom();
    }

    public static Unit GetRandomVisibleUnit(this WorldObject wObj, float radius, Func<Unit, bool> filter)
    {
      return (Unit) wObj.GetVisibleObjectsInRadius(radius, obj =>
      {
        if(obj is Unit)
          return filter((Unit) obj);
        return false;
      }, 0).GetRandom();
    }

    public static Unit GetNearbyRandomAlliedUnit<O>(this O wObj) where O : WorldObject
    {
      return wObj.GetNearbyRandomAlliedUnit(WorldObject.BroadcastRange);
    }

    public static Unit GetNearbyRandomAlliedUnit<O>(this O wObj, float radius) where O : WorldObject
    {
      return wObj.GetRandomVisibleUnit(radius,
        unit => unit.IsAlliedWith(wObj));
    }

    public static Unit GetNearbyRandomHostileUnit<O>(this O wObj, float radius) where O : WorldObject
    {
      return wObj.GetRandomVisibleUnit(radius, wObj.MayAttack);
    }
  }
}