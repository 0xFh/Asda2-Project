using System;
using System.Collections.Generic;
using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor
{
  /// <summary>
  /// Every map can have one MapEditor.
  /// It contains all the information related to editing that map (mostly spawn editing).
  /// </summary>
  public class MapEditor
  {
    /// <summary>
    /// Set of Characters who are currently working with this editor.
    /// Only use in the Map's context.
    /// </summary>
    public readonly Dictionary<uint, EditorArchitectInfo> Team = new Dictionary<uint, EditorArchitectInfo>();

    private readonly List<EditorFigurine> Figurines = new List<EditorFigurine>(100);
    public const int CharacterUpdateDelayMillis = 1000;
    private bool m_IsVisible;

    public MapEditor(Map map)
    {
      Map = map;
      Menu = new MapEditorMenu(this);
      m_IsVisible = false;
    }

    public GossipMenu Menu { get; private set; }

    public Map Map { get; private set; }

    public bool IsVisible
    {
      get { return m_IsVisible; }
      set
      {
        if(m_IsVisible == value)
          return;
        Map.EnsureContext();
        m_IsVisible = value;
        if(value)
          PlaceFigurines();
        else
          RemoveFigurines();
      }
    }

    private void PlaceFigurines()
    {
      Map.ForeachSpawnPool(pool =>
      {
        foreach(NPCSpawnPoint spawnPoint in pool.SpawnPoints)
        {
          SpawnPointFigurine spawnPointFigurine = new SpawnPointFigurine(this, spawnPoint);
          Figurines.Add(spawnPointFigurine);
          spawnPointFigurine.TeleportTo(spawnPoint);
        }
      });
    }

    private void RemoveFigurines()
    {
      foreach(WorldObject figurine in Figurines)
        figurine.Delete();
      Figurines.Clear();
    }

    private void InvalidateVisibilityForTeamMembers()
    {
      foreach(EditorArchitectInfo editorArchitectInfo in Team.Values)
      {
        if(editorArchitectInfo.Character.IsInContext)
          editorArchitectInfo.Character.ResetOwnWorld();
      }
    }

    /// <summary>
    /// Adds the given Character to the team of this editor (if not already part of the team)
    /// </summary>
    public void Join(Character chr)
    {
      Map.AddMessage(() =>
      {
        if(chr.Map != Map)
          return;
        OnJoin(chr);
      });
    }

    public void Leave(Character chr)
    {
      Map.AddMessage(() => OnLeave(chr));
    }

    private void OnJoin(Character chr)
    {
      if(Team.ContainsKey(chr.EntityId.Low))
        return;
      EditorArchitectInfo architectInfo = MapEditorMgr.GetOrCreateArchitectInfo(chr);
      Team.Add(chr.EntityId.Low, architectInfo);
      architectInfo.Editor = this;
      chr.CallPeriodically(1000, UpdateCallback);
    }

    private void OnLeave(Character chr)
    {
      Team.Remove(chr.EntityId.Low);
      chr.RemoveUpdateAction(UpdateCallback);
      chr.ResetOwnWorld();
    }

    /// <summary>Called periodically on editing Characters</summary>
    private void UpdateCallback(WorldObject obj)
    {
      Character chr = (Character) obj;
      if(obj.Map != Map || !obj.IsInWorld)
      {
        Leave(chr);
      }
      else
      {
        EditorArchitectInfo architectInfo = MapEditorMgr.GetOrCreateArchitectInfo(chr);
        EditorFigurine target = chr.Target as EditorFigurine;
        if(architectInfo.CurrentTarget == target)
          return;
        EditorFigurine editorFigurine = target;
        architectInfo.CurrentTarget = editorFigurine;
      }
    }
  }
}