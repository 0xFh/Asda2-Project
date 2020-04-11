using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    public class TeleportCommand : RealmServerCommand
    {
        protected TeleportCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            this.Init("Tele", "Teleport");
            this.EnglishParamInfo = "[-c [<x> <y> [<MapName or Id>]]] | [<LocationName>]";
            this.EnglishDescription =
                "Teleports to the given location or shows a list of all places that match the given name. -c teleports to the given coordinates instead. ";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("Invalid position. Usage: " + this.EnglishParamInfo);
            }
            else
            {
                Unit target = trigger.Args.Target;
                if (trigger.Text.NextModifiers() == "c")
                {
                    float? orientation = new float?();
                    Map map = (Map) null;
                    float num1 = trigger.Text.NextFloat(-50001f);
                    float num2 = trigger.Text.NextFloat(-50001f);
                    if (trigger.Text.HasNext)
                    {
                        MapId mapId = trigger.Text.NextEnum<MapId>(MapId.End);
                        map = WCell.RealmServer.Global.World.GetNonInstancedMap(mapId);
                        if (map == null)
                        {
                            trigger.Reply("Invalid map: " + (object) mapId);
                            return;
                        }
                    }

                    if ((double) num1 < -50000.0 || (double) num2 < -50000.0)
                    {
                        trigger.Reply("Invalid position. Usage: " + this.EnglishParamInfo);
                    }
                    else
                    {
                        if (map == null)
                            map = trigger.Args.Character.Map;
                        Vector3 pos = new Vector3(num1 + map.Offset, num2 + map.Offset, 0.0f);
                        trigger.Args.Target.TeleportTo(map, ref pos, orientation);
                    }
                }
                else
                {
                    string targetName = trigger.Text.Remainder;
                    if (trigger.Args.Character != null)
                    {
                        List<INamedWorldZoneLocation> matches = WorldLocationMgr.GetMatches(targetName);
                        if (matches.Count == 0)
                            trigger.Reply("No matches found for: " + targetName);
                        else if (matches.Count == 1)
                        {
                            target.TeleportTo((IWorldLocation) matches[0]);
                        }
                        else
                        {
                            INamedWorldZoneLocation worldZoneLocation =
                                matches.FirstOrDefault<INamedWorldZoneLocation>(
                                    (Func<INamedWorldZoneLocation, bool>) (loc =>
                                        loc.DefaultName.Equals(targetName,
                                            StringComparison.InvariantCultureIgnoreCase)));
                            if (worldZoneLocation != null)
                                target.TeleportTo((IWorldLocation) worldZoneLocation);
                            else
                                trigger.Args.Character.StartGossip(WorldLocationMgr.CreateTeleMenu(matches));
                        }
                    }
                    else
                    {
                        INamedWorldZoneLocation firstMatch = WorldLocationMgr.GetFirstMatch(targetName);
                        if (firstMatch != null)
                            target.TeleportTo((IWorldLocation) firstMatch);
                        else
                            trigger.Reply("No matches found for: " + targetName);
                    }
                }
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}