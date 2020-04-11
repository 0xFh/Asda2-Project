using System.Collections.Generic;
using WCell.Core.Addons;
using WCell.RealmServer.Addons;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class AddonCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Addon");
      EnglishDescription = "Provides commands for managing Addons";
    }

    public class ListAddonsCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("List", "L");
        EnglishParamInfo = "[-l]";
        EnglishDescription = "Lists all active Addons. -l to also list libraries.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = 0;
        bool flag = trigger.Text.NextModifiers().Contains("l");
        foreach(WCellAddonContext context in WCellAddonMgr.Contexts)
        {
          ++num;
          if(context.Addon != null)
            trigger.Reply(num + ". " + trigger.Translate(RealmLangKey.Addon) +
                          " " + context.Addon);
          else if(flag)
            trigger.Reply(num + ". " + trigger.Translate(RealmLangKey.Library) +
                          " " + context.Assembly);
        }
      }
    }

    public class LoadAddonCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Load");
        EnglishParamInfo = "<Path>";
        EnglishDescription = "Loads a new Addon from the given file.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string libName = trigger.Text.NextWord();
        if(libName.Length == 0)
        {
          trigger.Reply("No Path given.");
        }
        else
        {
          trigger.Reply("Loading addon from " + libName + "...");
          WCellAddonContext wcellAddonContext = WCellAddonMgr<RealmAddonMgr>.Instance.TryLoadAddon(libName);
          if(wcellAddonContext == null)
            trigger.Reply("File does not exist or has invalid format: " + libName);
          else
            trigger.Reply("Done: " + wcellAddonContext);
        }
      }
    }
  }
}