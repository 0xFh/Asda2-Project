using WCell.Core;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Entities
{
  /// <summary>
  /// Wraps a WorldObject.
  /// This is primarily used for Auras and other things that are allowed to persist after
  /// a Character or object might be gone, but still require basic information about the original
  /// object.
  /// </summary>
  public class ObjectReference : IEntity
  {
    private WorldObject m_Object;

    public static ObjectReference GetOrCreate(Map rgn, EntityId id)
    {
      WorldObject worldObject = rgn.GetObject(id);
      if(worldObject != null)
        return worldObject.SharedReference;
      return new ObjectReference(id, 1);
    }

    public EntityId EntityId { get; private set; }

    public ObjectReference(WorldObject obj)
    {
      EntityId = obj.EntityId;
      Level = obj.CasterLevel;
      m_Object = obj;
    }

    public ObjectReference(EntityId entityId, int level)
    {
      EntityId = entityId;
      Level = level;
    }

    public ObjectReference(int level)
    {
      Level = level;
    }

    public ObjectReference()
    {
    }

    public int Level { get; internal set; }

    public WorldObject Object
    {
      get
      {
        if(m_Object == null || !m_Object.IsInWorld)
          return null;
        return m_Object;
      }
      internal set { m_Object = value; }
    }

    /// <summary>Returns the Unit behind this object (if exists)</summary>
    public Unit UnitMaster
    {
      get
      {
        if(m_Object == null)
          return null;
        return m_Object.UnitMaster;
      }
    }

    public override string ToString()
    {
      WorldObject worldObject = Object;
      if(worldObject != null)
        return worldObject.ToString();
      return string.Format("Object with Id: {0}", EntityId);
    }
  }
}