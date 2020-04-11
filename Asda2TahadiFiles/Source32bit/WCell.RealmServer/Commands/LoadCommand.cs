using System;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class LoadCommand : RealmServerCommand
  {
    protected LoadCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Load");
      EnglishDescription = "Loads static data from DB.";
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    public class LoadItemsCommand : SubCommand
    {
      protected LoadItemsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Items");
        EnglishDescription = "Loads all ItemTemplates and -Spawns.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(Asda2ItemMgr.Loaded)
          trigger.Reply("Item definitions have already been loaded.");
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            trigger.Reply("Loading Items...");
            Asda2ItemMgr.LoadAll();
            trigger.Reply("Done.");
          });
      }
    }

    public class LoadGOsCommand : SubCommand
    {
      protected LoadGOsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("GOs");
        EnglishDescription = "Loads all GOTemplates and spawns.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(GOMgr.Loaded)
          trigger.Reply("GO definitions have already been loaded.");
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            trigger.Reply("Loading GOs...");
            GOMgr.LoadAll();
            if(Map.AutoSpawnMaps)
              MapCommand.MapSpawnCommand.SpawnAllMaps(trigger);
            trigger.Reply("Done.");
          });
      }
    }

    public class LoadNPCsCommand : SubCommand
    {
      protected LoadNPCsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("NPCs");
        EnglishParamInfo = "[esw]";
        EnglishDescription =
          "Loads all NPC definitions from files and/or DB. e: Load entries; s: Load Spawns; w: Load Waypoints (together with s)";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Process(trigger, false);
      }

      public virtual void Process(CmdTrigger<RealmServerCmdArgs> trigger, bool force)
      {
        if(!force && NPCMgr.Loaded)
          trigger.Reply("NPC definitions have already been loaded.");
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            trigger.Reply("Loading NPCs{0}...", force ? (object) " (FORCED) " : (object) "");
            if(!trigger.Text.HasNext)
            {
              NPCMgr.LoadNPCDefs(force);
            }
            else
            {
              string str = trigger.Text.NextWord();
              if(str.Contains("e"))
                NPCMgr.LoadEntries(force);
              if(str.Contains("s"))
                NPCMgr.OnlyLoadSpawns(force);
              if(str.Contains("w"))
                NPCMgr.LoadWaypoints(force);
            }

            if(Map.AutoSpawnMaps)
              MapCommand.MapSpawnCommand.SpawnAllMaps(trigger);
            trigger.Reply("Done.");
          });
      }
    }

    public class LoadQuestsCommand : SubCommand
    {
      protected LoadQuestsCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Quests");
        EnglishDescription = "Loads all Quest definitions from files and/or DB.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(QuestMgr.Loaded)
          trigger.Reply("Quest definitions have already been loaded.");
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            trigger.Reply("Loading Quests...");
            QuestMgr.LoadAll();
            trigger.Reply("Done.");
          });
      }
    }

    public class LoadLootCommand : SubCommand
    {
      protected LoadLootCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Loot");
        EnglishDescription = "Loads all Loot from files and/or DB.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(Asda2LootMgr.Loaded)
        {
          trigger.Reply("Loot has already been loaded. Clearing and reloading");
          Asda2LootMgr.ClearLootData();
        }

        ServerApp<RealmServer>.IOQueue.AddMessage(() =>
        {
          trigger.Reply("Loading Loot...");
          Asda2LootMgr.LoadAll();
          trigger.Reply("Done.");
        });
      }
    }

    public class LoadAllCommand : SubCommand
    {
      protected LoadAllCommand()
      {
      }

      protected override void Initialize()
      {
        Init("All");
        EnglishParamInfo = "[-w]";
        EnglishDescription =
          "Loads all static content definitions from DB. The -w switch will ensure that execution (of the current Map) won't continue until Loading finished.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(trigger.Text.NextModifiers() == "w")
          LoadAll(trigger, false);
        else
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
            LoadAll(trigger, false));
      }

      public virtual void LoadAll(CmdTrigger<RealmServerCmdArgs> trigger, bool force)
      {
        DateTime now = DateTime.Now;
        try
        {
          if(ItemMgr.Loaded)
          {
            trigger.Reply("Item definitions have already been loaded.");
          }
          else
          {
            trigger.Reply("Loading Items...");
            ItemMgr.LoadAll();
            trigger.Reply("Done.");
          }
        }
        catch(Exception ex)
        {
          FailNotify(trigger, ex);
        }

        try
        {
          if(NPCMgr.Loaded)
          {
            trigger.Reply("NPC definitions have already been loaded.");
          }
          else
          {
            trigger.Reply("Loading NPCs...");
            NPCMgr.LoadNPCDefs(force);
            trigger.Reply("Done.");
          }
        }
        catch(Exception ex)
        {
          FailNotify(trigger, ex);
        }

        try
        {
          if(GOMgr.Loaded)
          {
            trigger.Reply("GO definitions have already been loaded.");
          }
          else
          {
            trigger.Reply("Loading GOs...");
            GOMgr.LoadAll();
            trigger.Reply("Done.");
          }
        }
        catch(Exception ex)
        {
          FailNotify(trigger, ex);
        }

        try
        {
          if(QuestMgr.Loaded)
          {
            trigger.Reply("Quest definitions have already been loaded.");
          }
          else
          {
            trigger.Reply("Loading Quests...");
            QuestMgr.LoadAll();
            trigger.Reply("Done.");
          }
        }
        catch(Exception ex)
        {
          FailNotify(trigger, ex);
        }

        try
        {
          if(Asda2LootMgr.Loaded)
          {
            trigger.Reply("Loot has already been loaded.");
          }
          else
          {
            trigger.Reply("Loading Loot...");
            Asda2LootMgr.LoadAll();
            trigger.Reply("Done.");
          }
        }
        catch(Exception ex)
        {
          FailNotify(trigger, ex);
        }

        trigger.Reply("All done - Loading took: " + (DateTime.Now - now));
        GC.Collect(2, GCCollectionMode.Optimized);
        if(!Map.AutoSpawnMaps)
          return;
        MapCommand.MapSpawnCommand.SpawnAllMaps(trigger);
      }
    }
  }
}