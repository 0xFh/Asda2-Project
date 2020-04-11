using System;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Misc;
using WCell.Util.Commands;
using WCell.Util.Strings;

namespace WCell.RealmServer.Commands
{
  /// <summary>Default trigger for console</summary>
  public class DefaultCmdTrigger : RealmServerCmdTrigger
  {
    public DefaultCmdTrigger()
    {
    }

    public DefaultCmdTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand, RealmServerCmdArgs args)
      : base(new StringStream(text), selectedCommand, args)
    {
    }

    public DefaultCmdTrigger(string text)
      : base(new StringStream(text), null,
        new RealmServerCmdArgs(null, false, null))
    {
    }

    public DefaultCmdTrigger(string text, BaseCommand<RealmServerCmdArgs> selectedCommand)
      : base(new StringStream(text), selectedCommand,
        new RealmServerCmdArgs(null, false, null))
    {
    }

    public DefaultCmdTrigger(StringStream args)
      : base(args, null,
        new RealmServerCmdArgs(null, false, null))
    {
    }

    public DefaultCmdTrigger(StringStream args, BaseCommand<RealmServerCmdArgs> selectedCommand)
      : base(args, selectedCommand, new RealmServerCmdArgs(null, false, null))
    {
    }

    public override void Reply(string txt)
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(txt);
      Console.ResetColor();
    }

    public override void ReplyFormat(string txt)
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine(ChatUtility.Strip(txt));
      Console.ResetColor();
    }
  }
}