using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SpawnZoneCommand : RealmServerCommand
  {
    protected SpawnZoneCommand()
    {
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    protected override void Initialize()
    {
      Init("SpawnZone");
      EnglishParamInfo = "[<name>]";
      EnglishDescription =
        "Spawns GOs and NPCs in the current (or specified) Zone. - Only used for development purposes where the maps arent spawned automatically.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      trigger.Reply("Not yet implemented - Use \"Map Spawn\" instead");
    }
  }
}