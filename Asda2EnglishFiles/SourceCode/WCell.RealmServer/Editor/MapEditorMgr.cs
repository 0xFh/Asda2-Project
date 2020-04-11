using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Collections;

namespace WCell.RealmServer.Editor
{
    public static class MapEditorMgr
    {
        /// <summary>All active Editors</summary>
        public static readonly IDictionary<Map, MapEditor> EditorsByMap =
            (IDictionary<Map, MapEditor>) new SynchronizedDictionary<Map, MapEditor>();

        /// <summary>Everyone who is or was recently using the editor</summary>
        public static readonly IDictionary<Character, EditorArchitectInfo> Architects =
            (IDictionary<Character, EditorArchitectInfo>) new SynchronizedDictionary<Character, EditorArchitectInfo>();

        public static MapEditor GetEditor(Map map)
        {
            MapEditor mapEditor;
            MapEditorMgr.EditorsByMap.TryGetValue(map, out mapEditor);
            return mapEditor;
        }

        public static EditorArchitectInfo GetArchitectInfo(Character chr)
        {
            EditorArchitectInfo editorArchitectInfo;
            MapEditorMgr.Architects.TryGetValue(chr, out editorArchitectInfo);
            return editorArchitectInfo;
        }

        public static MapEditor GetOrCreateEditor(Map map)
        {
            MapEditor mapEditor;
            if (!MapEditorMgr.EditorsByMap.TryGetValue(map, out mapEditor))
                MapEditorMgr.EditorsByMap.Add(map, mapEditor = new MapEditor(map));
            return mapEditor;
        }

        public static EditorArchitectInfo GetOrCreateArchitectInfo(Character chr)
        {
            EditorArchitectInfo editorArchitectInfo;
            if (!MapEditorMgr.Architects.TryGetValue(chr, out editorArchitectInfo))
                MapEditorMgr.Architects.Add(chr, editorArchitectInfo = new EditorArchitectInfo(chr));
            return editorArchitectInfo;
        }

        public static MapEditor StartEditing(Map map, Character chr = null)
        {
            MapEditor editor = MapEditorMgr.GetOrCreateEditor(map);
            if (chr != null)
                editor.Join(chr);
            return editor;
        }

        public static void StopEditing(Character chr)
        {
            EditorArchitectInfo architectInfo = MapEditorMgr.GetArchitectInfo(chr);
            if (architectInfo == null)
                return;
            architectInfo.Editor.Leave(chr);
        }
    }
}