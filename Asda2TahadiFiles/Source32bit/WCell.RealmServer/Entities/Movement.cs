using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
  public class Movement
  {
    private const float SPEED_FACTOR = 0.001f;

    /// <summary>Starting time of movement</summary>
    protected uint m_lastMoveTime;

    /// <summary>Total time of movement</summary>
    protected uint m_totalMovingTime;

    /// <summary>Time at which movement should end</summary>
    protected uint m_desiredEndMovingTime;

    /// <summary>The target of the current (or last) travel</summary>
    protected Vector3 m_destination;

    protected Path _currentPath;

    /// <summary>The movement type (walking, running or flying)</summary>
    protected AIMoveType m_moveType;

    protected internal Unit m_owner;
    protected bool m_moving;
    protected bool m_MayMove;
    protected PathQuery m_currentQuery;
    private Vector2 _lastDestPosition;

    public Movement(Unit owner)
      : this(owner, AIMoveType.Run)
    {
    }

    public Movement(Unit owner, AIMoveType moveType)
    {
      m_owner = owner;
      m_moveType = moveType;
      m_MayMove = true;
    }

    public Vector3 Destination
    {
      get { return m_destination; }
    }

    /// <summary>AI-controlled Movement setting</summary>
    public bool MayMove
    {
      get { return m_MayMove; }
      set { m_MayMove = value; }
    }

    public bool IsMoving
    {
      get { return m_moving; }
    }

    /// <summary>Whether the owner is within 1 yard of the Destination</summary>
    public bool IsAtDestination
    {
      get { return m_owner.Position.DistanceSquared(ref m_destination) < 1.0; }
    }

    /// <summary>Get movement flags for the packet</summary>
    /// <returns></returns>
    public virtual MonsterMoveFlags MovementFlags
    {
      get
      {
        MonsterMoveFlags monsterMoveFlags;
        switch(m_moveType)
        {
          case AIMoveType.Walk:
          case AIMoveType.Run:
            monsterMoveFlags = MonsterMoveFlags.Walk;
            break;
          case AIMoveType.Sprint:
            monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
            break;
          case AIMoveType.Fly:
            monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
            break;
          default:
            monsterMoveFlags = MonsterMoveFlags.Flag_0x2000_FullPoints_1;
            break;
        }

        return monsterMoveFlags;
      }
    }

    public AIMoveType MoveType
    {
      get { return m_moveType; }
      set { m_moveType = value; }
    }

    /// <summary>
    /// Remaining movement time to current Destination (in millis)
    /// </summary>
    public uint RemainingTime
    {
      get
      {
        float num1;
        float num2;
        if(m_owner.IsFlying)
        {
          num1 = m_owner.FlightSpeed;
          num2 = m_owner.GetDistance(ref m_destination);
        }
        else if(m_moveType == AIMoveType.Run)
        {
          num1 = m_owner.RunSpeed;
          num2 = m_owner.GetDistanceXY(ref m_destination);
        }
        else
        {
          num1 = m_owner.WalkSpeed;
          num2 = m_owner.GetDistanceXY(ref m_destination);
        }

        float num3 = num1 * (1f / 1000f);
        return (uint) (num2 / (double) num3);
      }
    }

    /// <summary>Starts the MovementAI</summary>
    /// <returns>Whether already arrived</returns>
    public bool MoveTo(Vector3 destination, bool findPath = true)
    {
      if(!m_owner.IsInWorld)
      {
        m_owner.DeleteNow();
        return false;
      }

      m_destination = destination;
      if(IsAtDestination)
        return true;
      if(findPath)
      {
        Vector3 position = m_owner.Position;
        position.Z += 5f;
        m_currentQuery = new PathQuery(position, ref destination, m_owner.ContextHandler,
          OnPathQueryReply);
        m_owner.Map.Terrain.FindPath(m_currentQuery);
      }
      else if(m_owner.CanMove)
        MoveToDestination();

      return false;
    }

    /// <summary>Starts the MovementAI</summary>
    /// <returns>Whether already arrived</returns>
    public bool MoveToPoints(List<Vector3> points)
    {
      if(!m_owner.IsInWorld)
      {
        m_owner.DeleteNow();
        return false;
      }

      m_destination = points[points.Count - 1];
      if(IsAtDestination)
        return true;
      Vector3 position = m_owner.Position;
      position.Z += 5f;
      m_currentQuery = new PathQuery(position, ref m_destination, m_owner.ContextHandler,
        OnPathQueryReply);
      m_currentQuery.Path.Reset(points.Count);
      foreach(Vector3 point in points)
        m_currentQuery.Path.Add(point);
      m_currentQuery.Reply();
      return false;
    }

    /// <summary>Interpolates the current Position</summary>
    /// <returns>Whether we arrived</returns>
    public bool Update()
    {
      if(!m_moving)
        return false;
      if(!MayMove)
      {
        Stop();
        return false;
      }

      return UpdatePosition() && !CheckCollision();
    }

    private bool CheckCollision()
    {
      IList<WorldObject> objectsInRadius =
        m_owner.GetObjectsInRadius(m_owner.BoundingCollisionRadius, ObjectTypes.Unit, false,
          int.MaxValue);
      objectsInRadius.Remove(m_owner);
      float x = 0.0f;
      float y = 0.0f;
      foreach(Unit unit in objectsInRadius)
      {
        float num = m_owner.IsCollisionWith(unit);
        if(num > 0.0)
        {
          Vector3 vector3 = new Vector3(m_owner.Position.X - unit.Position.X,
            m_owner.Position.Y - unit.Position.Y);
          vector3.Normalize();
          x += vector3.X * num;
          y += vector3.Y * num;
        }
      }

      Vector3 vector3_1 = new Vector3(x, y);
      if(!(vector3_1 != Vector3.Zero))
        return false;
      if(vector3_1.Length() > m_owner.BoundingRadius / 2.0)
      {
        m_destination = new Vector3(m_owner.Position.X + x, m_owner.Position.Y + y);
        MoveToDestination();
      }

      return true;
    }

    /// <summary>Stops at the current position</summary>
    public void Stop()
    {
      if(!m_moving)
        return;
      UpdatePosition();
      m_moving = false;
    }

    /// <summary>Starts moving to current Destination</summary>
    /// <remarks>Sends movement packet to client</remarks>
    protected void MoveToDestination()
    {
      m_moving = true;
      m_totalMovingTime = RemainingTime;
      m_owner.SetOrientationTowards(ref m_destination);
      NPC owner = m_owner as NPC;
      if(_lastDestPosition != m_destination.XY)
      {
        Asda2MovmentHandler.SendMonstMoveOrAtackResponse(-1, owner, -1,
          new Vector3(m_destination.X - m_owner.Map.Offset,
            m_destination.Y - m_owner.Map.Offset), false);
        _lastDestPosition = m_destination.XY;
      }

      m_lastMoveTime = Utility.GetSystemTime();
      m_desiredEndMovingTime = m_lastMoveTime + m_totalMovingTime;
    }

    protected void OnPathQueryReply(PathQuery query)
    {
      if(query != m_currentQuery)
        return;
      m_currentQuery = null;
      FollowPath(query.Path);
    }

    public void FollowPath(Path path)
    {
      _currentPath = path;
      m_destination = _currentPath.Next();
      MoveToDestination();
    }

    /// <summary>Updates position of unit</summary>
    /// <returns>true if target point is reached</returns>
    protected bool UpdatePosition()
    {
      uint systemTime = Utility.GetSystemTime();
      float num = (systemTime - m_lastMoveTime) / (float) m_totalMovingTime;
      if(systemTime >= m_desiredEndMovingTime || num >= 1.0)
      {
        m_owner.Map.MoveObject(m_owner, ref m_destination);
        if(_currentPath != null)
        {
          if(_currentPath.HasNext())
          {
            if(!CheckCollision())
            {
              m_destination = _currentPath.Next();
              MoveToDestination();
            }
          }
          else
            _currentPath = null;

          return false;
        }

        m_moving = false;
        return true;
      }

      Vector3 position = m_owner.Position;
      Vector3 newPos = position + (m_destination - position) * num;
      m_lastMoveTime = systemTime;
      m_owner.Map.MoveObject(m_owner, ref newPos);
      return false;
    }
  }
}