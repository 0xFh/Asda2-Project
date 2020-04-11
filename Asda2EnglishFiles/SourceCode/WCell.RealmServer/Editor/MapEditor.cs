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
            this.Map = map;
            this.Menu = (GossipMenu) new MapEditorMenu(this);
            this.m_IsVisible = false;
        }

        public GossipMenu Menu { get; private set; }

        public Map Map { get; private set; }

        public bool IsVisible
        {
            get { return this.m_IsVisible; }
            set
            {
                if (this.m_IsVisible == value)
                    return;
                this.Map.EnsureContext();
                this.m_IsVisible = value;
                if (value)
                    this.PlaceFigurines();
                else
                    this.RemoveFigurines();
            }
        }

        private void PlaceFigurines()
        {
            this.Map.ForeachSpawnPool((Action<NPCSpawnPool>) (pool =>
            {
                foreach (NPCSpawnPoint spawnPoint in pool.SpawnPoints)
                {
                    SpawnPointFigurine spawnPointFigurine = new SpawnPointFigurine(this, spawnPoint);
                    this.Figurines.Add((EditorFigurine) spawnPointFigurine);
                    spawnPointFigurine.TeleportTo((IWorldLocation) spawnPoint);
                }
            }));
        }

        private void RemoveFigurines()
        {
            foreach (WorldObject figurine in this.Figurines)
                figurine.Delete();
            this.Figurines.Clear();
        }

        private void InvalidateVisibilityForTeamMembers()
        {
            foreach (EditorArchitectInfo editorArchitectInfo in this.Team.Values)
            {
                if (editorArchitectInfo.Character.IsInContext)
                    editorArchitectInfo.Character.ResetOwnWorld();
            }
        }

        /// <summary>
        /// Adds the given Character to the team of this editor (if not already part of the team)
        /// </summary>
        public void Join(Character chr)
        {
            this.Map.AddMessage((Action) (() =>
            {
                if (chr.Map != this.Map)
                    return;
                this.OnJoin(chr);
            }));
        }

        public void Leave(Character chr)
        {
            this.Map.AddMessage((Action) (() => this.OnLeave(chr)));
        }

        private void OnJoin(Character chr)
        {
            if (this.Team.ContainsKey(chr.EntityId.Low))
                return;
            EditorArchitectInfo architectInfo = MapEditorMgr.GetOrCreateArchitectInfo(chr);
            this.Team.Add(chr.EntityId.Low, architectInfo);
            architectInfo.Editor = this;
            chr.CallPeriodically(1000, new Action<WorldObject>(this.UpdateCallback));
        }

        private void OnLeave(Character chr)
        {
            this.Team.Remove(chr.EntityId.Low);
            chr.RemoveUpdateAction(new Action<WorldObject>(this.UpdateCallback));
            chr.ResetOwnWorld();
        }

        /// <summary>Called periodically on editing Characters</summary>
        private void UpdateCallback(WorldObject obj)
        {
            Character chr = (Character) obj;
            if (obj.Map != this.Map || !obj.IsInWorld)
            {
                this.Leave(chr);
            }
            else
            {
                EditorArchitectInfo architectInfo = MapEditorMgr.GetOrCreateArchitectInfo(chr);
                EditorFigurine target = chr.Target as EditorFigurine;
                if (architectInfo.CurrentTarget == target)
                    return;
                EditorFigurine editorFigurine = target;
                architectInfo.CurrentTarget = editorFigurine;
            }
        }
    }
}