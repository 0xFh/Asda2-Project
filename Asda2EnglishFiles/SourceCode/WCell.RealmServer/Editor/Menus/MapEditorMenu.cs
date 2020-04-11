using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Editor.Menus
{
    public class MapEditorMenu : DynamicTextGossipMenu
    {
        /// <summary>Generate the menu's text dynamically</summary>
        public override string GetText(GossipConversation convo)
        {
            string str =
                RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuText,
                    new object[0]) + GossipTextHelper.Newline;
            if (!GOMgr.Loaded || !NPCMgr.Loaded)
                str =
                    convo.Speaker.HasUpdateAction(
                        (Func<ObjectUpdateTimer, bool>) (action => action is MapEditorMenu.PeriodicLoadMapTimer))
                        ? str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                              RealmLangKey.EditorMapMenuStatusDataLoading, new object[0])
                        : str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                              RealmLangKey.EditorMapMenuStatusNoData, new object[0]);
            else if (!this.Editor.Map.IsSpawned)
                str = !this.Editor.Map.IsSpawning
                    ? str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                          RealmLangKey.EditorMapMenuStatusNotSpawned, new object[0])
                    : str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                          RealmLangKey.EditorMapMenuStatusSpawning, new object[0]);
            return str;
        }

        public MapEditorMenu(MapEditor editor)
        {
            this.Editor = editor;
            this.KeepOpen = true;
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                new GossipActionHandler(MapEditorMenu.OnLoadClicked), (GossipActionDecider) (convo =>
                {
                    if (!GOMgr.Loaded || !NPCMgr.Loaded)
                        return !convo.Speaker.HasUpdateAction(
                            (Func<ObjectUpdateTimer, bool>) (action => action is MapEditorMenu.PeriodicLoadMapTimer));
                    return false;
                }), RealmLangKey.EditorMapMenuLoadData, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem((GossipActionHandler) (convo =>
            {
                this.Editor.Map.SpawnMapLater();
                convo.Character.AddMessage(new Action(convo.Invalidate));
            }), (GossipActionDecider) (convo =>
            {
                if (GOMgr.Loaded && NPCMgr.Loaded && !this.Editor.Map.IsSpawned)
                    return !this.Editor.Map.IsSpawning;
                return false;
            }), RealmLangKey.EditorMapMenuSpawnMap, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem((GossipActionHandler) (convo =>
                {
                    this.Editor.Map.ClearLater();
                    convo.Character.AddMessage(new Action(convo.Invalidate));
                }), (GossipActionDecider) (convo => this.Editor.Map.IsSpawned), RealmLangKey.AreYouSure,
                RealmLangKey.EditorMapMenuClearMap, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                (GossipActionHandler) (convo => this.Editor.IsVisible = true), (GossipActionDecider) (convo =>
                {
                    if (this.Editor.Map.IsSpawned)
                        return !this.Editor.IsVisible;
                    return false;
                }), RealmLangKey.EditorMapMenuShow, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                (GossipActionHandler) (convo => this.Editor.IsVisible = false), (GossipActionDecider) (convo =>
                {
                    if (this.Editor.Map.IsSpawned)
                        return this.Editor.IsVisible;
                    return false;
                }), RealmLangKey.EditorMapMenuHide, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                (GossipActionHandler) (convo => this.Editor.Map.SpawnPointsEnabled = true),
                (GossipActionDecider) (convo => !this.Editor.Map.SpawnPointsEnabled),
                RealmLangKey.EditorMapMenuEnableAllSpawnPoints, new object[0]));
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                (GossipActionHandler) (convo => this.Editor.Map.SpawnPointsEnabled = false),
                (GossipActionDecider) (convo => this.Editor.Map.SpawnPointsEnabled), RealmLangKey.AreYouSure,
                RealmLangKey.EditorMapMenuDisableAllSpawnPoints, new object[0]));
            this.AddQuitMenuItem((GossipActionHandler) (convo => this.Editor.Leave(convo.Character)), RealmLangKey.Done,
                new object[0]);
        }

        public MapEditor Editor { get; private set; }

        private static void OnLoadClicked(GossipConversation convo)
        {
            GOMgr.LoadAllLater();
            NPCMgr.LoadAllLater();
            convo.Character.SendSystemMessage(RealmLangKey.PleaseWait, new object[0]);
            convo.Character.AddUpdateAction((ObjectUpdateTimer) new MapEditorMenu.PeriodicLoadMapTimer(convo));
        }

        private class PeriodicLoadMapTimer : ObjectUpdateTimer
        {
            private readonly GossipConversation m_Convo;

            public PeriodicLoadMapTimer(GossipConversation convo)
            {
                this.m_Convo = convo;
                this.Delay = 1000;
                this.Callback = new Action<WorldObject>(this.OnTick);
            }

            private void OnTick(WorldObject obj)
            {
                Character character = (Character) obj;
                if (!NPCMgr.Loaded || !GOMgr.Loaded)
                    return;
                if (character.GossipConversation == this.m_Convo)
                    this.m_Convo.Invalidate();
                character.RemoveUpdateAction((ObjectUpdateTimer) this);
            }
        }
    }
}