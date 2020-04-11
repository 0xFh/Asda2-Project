using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
  public class GOCommand : RealmServerCommand
  {
    protected GOCommand()
    {
    }

    protected override void Initialize()
    {
      Init("GO", "GOs", "GameObject");
      EnglishDescription = "Used for interaction with and creation of GameObjects";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.None; }
    }

    public override bool RequiresCharacter
    {
      get { return true; }
    }

    public class SpawnCommand : SubCommand
    {
      protected SpawnCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Spawn", "Create", "Add");
        EnglishParamInfo = "[-c] [<GOId>]";
        EnglishDescription =
          "Creates a new GameObject with the given id at the current position. -c spawns the closest GO in the Area and teleports you there.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string str = trigger.Text.NextModifiers();
        GOEntry entry = GOMgr.GetEntry(trigger.Text.NextEnum(GOEntryId.End), false);
        Unit target = trigger.Args.Target;
        Map map = target != null ? target.Map : World.Kalimdor;
        if(str == "c")
        {
          GOSpawnEntry goSpawnEntry;
          if(entry != null)
          {
            goSpawnEntry = entry.SpawnEntries.GetClosestEntry(target);
          }
          else
          {
            List<GOSpawnPoolTemplate> poolTemplatesByMap = GOMgr.GetSpawnPoolTemplatesByMap(map.Id);
            goSpawnEntry = poolTemplatesByMap == null
              ? null
              : poolTemplatesByMap.GetClosestEntry(target);
          }

          if(goSpawnEntry == null)
          {
            trigger.Reply("No valid SpawnEntries found (Entry: {0})", (object) entry);
          }
          else
          {
            goSpawnEntry.Spawn(map);
            trigger.Reply("Spawned: " + goSpawnEntry.Entry);
            if(target == null)
              return;
            target.TeleportTo(map, goSpawnEntry.Position);
          }
        }
        else if(entry != null)
        {
          GameObject gameObject = entry.Spawn(trigger.Args.Target, trigger.Args.Target);
          trigger.Reply("Successfully spawned new GO: {0}.", (object) gameObject.Name);
        }
        else
          trigger.Reply("Invalid GO.");
      }
    }

    public class SelectCommand : SubCommand
    {
      protected SelectCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Select", "Sel");
        EnglishParamInfo = "";
        EnglishDescription = "Selects the next GameObject in front of you";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Select(trigger);
      }

      public static void Select(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GameObject gameObject = GOSelectMgr.Instance.SelectClosest(trigger.Args.Character);
        if(gameObject != null)
        {
          if(!trigger.Args.HasCharacter)
            trigger.Args.Context = gameObject;
          trigger.Reply("Selected: " + gameObject);
        }
        else
          trigger.Reply("No Object in front of you within {0} yards.", (object) GOSelectMgr.MaxSearchRadius);
      }
    }

    public class DeselectCommand : SubCommand
    {
      protected DeselectCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Deselect", "Des");
        EnglishParamInfo = "";
        EnglishDescription = "Deselects your currently selected GameObject";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GOSelectMgr.Instance.Deselect(trigger.Args.Character.ExtraInfo);
        trigger.Args.Context = null;
        trigger.Reply("Done.");
      }
    }

    public class GoSetCommand : SubCommand
    {
      protected GoSetCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Set", "S");
        EnglishParamInfo = "<some.prop> <someValue>";
        EnglishDescription = "Sets properties on the currently selected GO";
      }

      public override RoleStatus DefaultRequiredStatus
      {
        get { return RoleStatus.Admin; }
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        SetCommand.Set(trigger, trigger.Args.Character.ExtraInfo.SelectedGO);
      }
    }

    public class GoGetCommand : SubCommand
    {
      protected GoGetCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Get", "G");
        EnglishParamInfo = "<some.prop>";
        EnglishDescription = "Gets properties on the currently selected GO";
      }

      public override RoleStatus DefaultRequiredStatus
      {
        get { return RoleStatus.Admin; }
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GetCommand.GetAndReply(trigger, trigger.Args.Character.ExtraInfo.SelectedGO);
      }
    }

    public class GoCallCommand : SubCommand
    {
      protected GoCallCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Call");
        EnglishParamInfo = "<some.method> [arg1 [, arg2, ...]]";
        EnglishDescription = "Calls the given method with parameters on the currently selected GO";
      }

      public override RoleStatus DefaultRequiredStatus
      {
        get { return RoleStatus.Admin; }
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        CallCommand.Call(trigger, trigger.Args.Character.ExtraInfo.SelectedGO, true);
      }
    }

    public class AnimCommand : SubCommand
    {
      protected AnimCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Anim", "Animation");
        EnglishParamInfo = "<animValue>";
        EnglishDescription = "Animates the selected GO with the given parameter";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GameObject selectedGo = trigger.Args.Character.ExtraInfo.SelectedGO;
        if(selectedGo == null)
        {
          trigger.Reply("No object selected.");
        }
        else
        {
          uint anim = trigger.Text.NextUInt(1U);
          selectedGo.SendCustomAnim(anim);
        }
      }
    }

    public class GoToggleCommand : SubCommand
    {
      protected GoToggleCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Toggle", "T");
        EnglishParamInfo = "[<value>]";
        EnglishDescription = "Toggles the state on the selected GO or the one in front of you";
      }

      public override RoleStatus DefaultRequiredStatus
      {
        get { return RoleStatus.Staff; }
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        GameObject selectedGo = trigger.Args.Character.ExtraInfo.SelectedGO;
        if(selectedGo == null)
          SelectCommand.Select(trigger);
        if(selectedGo == null)
          return;
        bool flag = trigger.Text.HasNext ? trigger.Text.NextBool() : !selectedGo.IsEnabled;
        selectedGo.IsEnabled = flag;
        trigger.Reply("{0} is now {1}", (object) selectedGo, flag ? (object) "enabled" : (object) "disabled");
      }
    }
  }
}