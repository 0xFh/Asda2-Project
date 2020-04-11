using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ReloadCommand : RealmServerCommand
  {
    protected ReloadCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Reload");
      EnglishDescription = "Flushes the cache and reloads static data from DB.";
    }

    public class ReloadNPCsCommand : LoadCommand.LoadNPCsCommand
    {
      protected override void Initialize()
      {
        Init("NPCs");
        EnglishParamInfo = "[esw]";
        EnglishDescription =
          "Reloads all NPC definitions from files and/or DB. e: Load entries; s: Load Spawns; w: Load Waypoints (together with s)";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Process(trigger, true);
      }
    }
  }
}