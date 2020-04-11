using System;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
  public class SaveCommand : RealmServerCommand
  {
    protected SaveCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Save");
      EnglishParamInfo = "";
      EnglishDescription = "Updates all changes on the given Character";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      trigger.Reply("Saving...");
      Character chr = (Character) trigger.Args.Target;
      ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() =>
      {
        if(chr == null)
          return;
        if(chr.SaveNow())
          trigger.Reply("Done.");
        else
          trigger.Reply("Could not save \"" + chr + "\" to DB.");
      }));
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }
  }
}