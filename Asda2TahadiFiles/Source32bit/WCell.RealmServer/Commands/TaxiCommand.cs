using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Taxi;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class TaxiCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Taxi");
      EnglishDescription = "Provides commands to manage Taxi nodes.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      if(!trigger.Text.HasNext)
        return;
      base.Process(trigger);
    }

    /// <summary>
    /// Returns all Nodes matching the arguments in the given trigger.
    /// </summary>
    /// <param name="trigger">[part of name|id]</param>
    /// <returns></returns>
    public static List<PathNode> GetNodes(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      string s = trigger.Text.NextWord();
      List<PathNode> pathNodeList = new List<PathNode>();
      uint result;
      if(uint.TryParse(s, out result))
      {
        PathNode node = TaxiMgr.GetNode(result);
        if(node == null)
        {
          trigger.Reply("Invalid Id: " + result);
          return pathNodeList;
        }

        pathNodeList.Add(node);
      }
      else if(s.Length > 0)
      {
        foreach(PathNode pathNode in TaxiMgr.PathNodesById)
        {
          if(pathNode != null && pathNode.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
            pathNodeList.Add(pathNode);
        }
      }
      else
      {
        foreach(PathNode pathNode in TaxiMgr.PathNodesById)
        {
          if(pathNode != null)
            pathNodeList.Add(pathNode);
        }
      }

      if(pathNodeList.Count == 0)
      {
        if(s.Length > 0)
          trigger.Reply("Invalid Node-name: " + s);
        else
          trigger.Reply("No Node.");
      }

      return pathNodeList;
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.None; }
    }

    public class StopTaxiCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Stop", "Cancel", "C");
        EnglishParamInfo = "";
        EnglishDescription = "Stops the current taxi flight (if flying).";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == null)
          trigger.Reply("No one selected.");
        else if(target.IsOnTaxi)
        {
          target.CancelTaxiFlight();
          trigger.Reply("Cancelled.");
        }
        else
          trigger.Reply("Not on Taxi.");
      }
    }

    public class ShowTaxiMapCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Show");
        EnglishParamInfo = "[-a]";
        EnglishDescription =
          "Shows the Taxi-Map. The -a switch automatically activates all Nodes beforehand.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Character target = trigger.Args.Target as Character;
        if(target == null)
        {
          trigger.Reply("No one selected.");
        }
        else
        {
          string str = trigger.Text.NextModifiers();
          if(str.Contains("a"))
            target.ActivateAllTaxiNodes();
          Map map;
          PathNode node;
          if(str.Contains("r"))
          {
            map = World.GetNonInstancedMap(
              trigger.Text.NextEnum(MapId.End));
            if(map == null)
            {
              trigger.Reply("Invalid Map.");
              return;
            }

            node = map.FirstTaxiNode;
          }
          else
          {
            map = target.Map;
            node = TaxiMgr.GetNearestTaxiNode(target.Position);
          }

          if(node != null)
            TaxiHandler.ShowTaxiList(target, node);
          else
            trigger.Reply("There are no Taxis available on this Map ({0})", (object) map.Name);
        }
      }
    }

    public class GotoNodeCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Go", "Goto");
        EnglishParamInfo = "[<name>|<id>]";
        EnglishDescription = "Goes to the first node matching the given Name or Id.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        List<PathNode> nodes = GetNodes(trigger);
        Unit target = trigger.Args.Target;
        if(target == null)
        {
          trigger.Reply("No one selected.");
        }
        else
        {
          PathNode pathNode = nodes.FirstOrDefault();
          if(pathNode == null)
            return;
          target.TeleportTo(pathNode.Map, pathNode.Position);
        }
      }
    }

    public class NextNodeCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("GotoNext", "TeleNext", "Next");
        EnglishDescription = "Teleports to the closest Taxi Node in the current Map.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == null)
        {
          trigger.Reply("No one selected.");
        }
        else
        {
          PathNode pathNode = null;
          float num = float.MaxValue;
          foreach(PathNode node in GetNodes(trigger))
          {
            if(node.MapId == target.Map.Id)
            {
              float distanceSq = target.GetDistanceSq(node.Position);
              if(distanceSq < (double) num)
              {
                num = distanceSq;
                pathNode = node;
              }
            }
          }

          if(pathNode == null)
            trigger.Reply("No Node found in Map.");
          else
            target.TeleportTo(pathNode.Map, pathNode.Position);
        }
      }
    }

    public class ListNodesCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("List");
        EnglishParamInfo = "[-rc <Map>][<name>|<id>]";
        EnglishDescription =
          "Lists all Taxi nodes or only those matching the given Name or Id. -r swtich filters Nodes of the given Map. -rc switch filters Nodes of the current Map.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        MapId mapId = MapId.End;
        string str = trigger.Text.NextModifiers();
        if(str.Contains("r"))
        {
          if(str.Contains("c") && trigger.Args.Target != null)
          {
            mapId = trigger.Args.Target.Map.Id;
          }
          else
          {
            mapId = trigger.Text.NextEnum(MapId.End);
            if(mapId == MapId.End)
            {
              trigger.Reply("Invalid Map: " + mapId);
              return;
            }
          }
        }

        foreach(PathNode node in GetNodes(trigger))
        {
          if(mapId == MapId.End || node.MapId == mapId)
            trigger.Reply(node.ToString());
        }
      }
    }

    public class TaxiInfoCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Info");
        EnglishParamInfo = "";
        EnglishDescription = "Shows information about the current Taxi-path.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if(trigger.Args.Target == null)
          trigger.Reply("Nothing selected.");
        else if(!trigger.Args.Target.IsOnTaxi)
        {
          trigger.Reply("{0} is not on a Taxi.", (object) trigger.Args.Target.Name);
        }
        else
        {
          TaxiPath taxiPath = trigger.Args.Target.TaxiPaths.Peek();
          trigger.Reply("Flying on: " + taxiPath);
          trigger.Reply("Flying for {0}m {1}s / {2}m {3}s ({4}%)",
            (object) (trigger.Args.Target.TaxiTime / 60000),
            (object) (trigger.Args.Target.TaxiTime / 1000 % 60), (object) (taxiPath.PathTime / 60000U),
            (object) (taxiPath.PathTime / 1000U % 60U),
            (object) ((long) (100 * trigger.Args.Target.TaxiTime) / (long) taxiPath.PathTime));
        }
      }
    }

    public class ActivateNodesCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Activate");
        EnglishParamInfo = "[<name>|<id>]";
        EnglishDescription = "Activates all Taxi nodes or only those matching the given Name or Id.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Character target = trigger.Args.Target as Character;
        if(target == null)
          trigger.Reply("No Character selected.");
        else
          target.ActivateAllTaxiNodes();
      }
    }
  }
}