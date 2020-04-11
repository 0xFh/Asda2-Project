using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class PortalCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Portal");
      EnglishParamInfo = "<target loc>";
      EnglishDescription = "Creates a new Portal to the given Target location.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      string remainder = trigger.Text.Remainder;
      Unit target = trigger.Args.Target;
      if(remainder.Length < 2)
        trigger.Reply("Invalid search term: " + remainder);
      else if(trigger.Args.Character != null)
      {
        List<INamedWorldZoneLocation> matches = WorldLocationMgr.GetMatches(remainder);
        if(matches.Count == 0)
          trigger.Reply("No matches found for: " + remainder);
        else if(matches.Count > 20)
          trigger.Reply("Found too many matches ({0}), please narrow down the location.",
            (object) matches.Count);
        else if(matches.Count == 1)
          CreatePortal(target, matches[0]);
        else
          trigger.Args.Character.StartGossip(WorldLocationMgr.CreateTeleMenu(matches,
            (convo, loc) =>
              CreatePortal(target, loc)));
      }
      else
      {
        INamedWorldZoneLocation firstMatch = WorldLocationMgr.GetFirstMatch(remainder);
        if(firstMatch != null)
          CreatePortal(target, firstMatch);
        else
          trigger.Reply("No matches found for: " + remainder);
      }
    }

    private void CreatePortal(WorldObject at, IWorldLocation target)
    {
      Portal portal = Portal.Create(at, target);
      at.PlaceInFront(portal);
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Unit; }
    }
  }
}