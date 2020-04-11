using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ExploreCommand : RealmServerCommand
  {
    protected ExploreCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Explore");
      EnglishParamInfo = "[<zone>]";
      EnglishDescription =
        "Explores the map. If zone is given it will toggle exploration of that zone, else it will explore all zones.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Character target = (Character) trigger.Args.Target;
      ZoneId id = trigger.Text.NextEnum(ZoneId.None);
      if(id == ZoneId.None)
      {
        for(PlayerFields playerFields = PlayerFields.EXPLORED_ZONES_1;
          playerFields < (PlayerFields) (1041L + (long) UpdateFieldMgr.ExplorationZoneFieldSize);
          ++playerFields)
          target.SetUInt32(playerFields, uint.MaxValue);
      }
      else
        target.SetZoneExplored(id, !target.IsZoneExplored(id));
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }
  }
}