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
        RealmLocalizer.Instance.Translate(convo.Character.Locale, RealmLangKey.EditorMapMenuText) +
        GossipTextHelper.Newline;
      if(!GOMgr.Loaded || !NPCMgr.Loaded)
        str =
          convo.Speaker.HasUpdateAction(
            action => action is PeriodicLoadMapTimer)
            ? str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                RealmLangKey.EditorMapMenuStatusDataLoading)
            : str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
                RealmLangKey.EditorMapMenuStatusNoData);
      else if(!Editor.Map.IsSpawned)
        str = !Editor.Map.IsSpawning
          ? str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
              RealmLangKey.EditorMapMenuStatusNotSpawned)
          : str + RealmLocalizer.Instance.Translate(convo.Character.Locale,
              RealmLangKey.EditorMapMenuStatusSpawning);
      return str;
    }

    public MapEditorMenu(MapEditor editor)
    {
      Editor = editor;
      KeepOpen = true;
      AddItem(new LocalizedGossipMenuItem(
        OnLoadClicked, convo =>
        {
          if(!GOMgr.Loaded || !NPCMgr.Loaded)
            return !convo.Speaker.HasUpdateAction(
              action => action is PeriodicLoadMapTimer);
          return false;
        }, RealmLangKey.EditorMapMenuLoadData));
      AddItem(new LocalizedGossipMenuItem(convo =>
      {
        Editor.Map.SpawnMapLater();
        convo.Character.AddMessage(convo.Invalidate);
      }, convo =>
      {
        if(GOMgr.Loaded && NPCMgr.Loaded && !Editor.Map.IsSpawned)
          return !Editor.Map.IsSpawning;
        return false;
      }, RealmLangKey.EditorMapMenuSpawnMap));
      AddItem(new LocalizedGossipMenuItem(convo =>
        {
          Editor.Map.ClearLater();
          convo.Character.AddMessage(convo.Invalidate);
        }, convo => Editor.Map.IsSpawned, RealmLangKey.AreYouSure,
        RealmLangKey.EditorMapMenuClearMap));
      AddItem(new LocalizedGossipMenuItem(
        convo => Editor.IsVisible = true, convo =>
        {
          if(Editor.Map.IsSpawned)
            return !Editor.IsVisible;
          return false;
        }, RealmLangKey.EditorMapMenuShow));
      AddItem(new LocalizedGossipMenuItem(
        convo => Editor.IsVisible = false, convo =>
        {
          if(Editor.Map.IsSpawned)
            return Editor.IsVisible;
          return false;
        }, RealmLangKey.EditorMapMenuHide));
      AddItem(new LocalizedGossipMenuItem(
        convo => Editor.Map.SpawnPointsEnabled = true,
        convo => !Editor.Map.SpawnPointsEnabled,
        RealmLangKey.EditorMapMenuEnableAllSpawnPoints));
      AddItem(new LocalizedGossipMenuItem(
        convo => Editor.Map.SpawnPointsEnabled = false,
        convo => Editor.Map.SpawnPointsEnabled, RealmLangKey.AreYouSure,
        RealmLangKey.EditorMapMenuDisableAllSpawnPoints));
      AddQuitMenuItem(convo => Editor.Leave(convo.Character), RealmLangKey.Done);
    }

    public MapEditor Editor { get; private set; }

    private static void OnLoadClicked(GossipConversation convo)
    {
      GOMgr.LoadAllLater();
      NPCMgr.LoadAllLater();
      convo.Character.SendSystemMessage(RealmLangKey.PleaseWait);
      convo.Character.AddUpdateAction(new PeriodicLoadMapTimer(convo));
    }

    private class PeriodicLoadMapTimer : ObjectUpdateTimer
    {
      private readonly GossipConversation m_Convo;

      public PeriodicLoadMapTimer(GossipConversation convo)
      {
        m_Convo = convo;
        Delay = 1000;
        Callback = OnTick;
      }

      private void OnTick(WorldObject obj)
      {
        Character character = (Character) obj;
        if(!NPCMgr.Loaded || !GOMgr.Loaded)
          return;
        if(character.GossipConversation == m_Convo)
          m_Convo.Invalidate();
        character.RemoveUpdateAction(this);
      }
    }
  }
}