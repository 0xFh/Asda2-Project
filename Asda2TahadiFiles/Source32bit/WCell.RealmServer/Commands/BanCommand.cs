using System;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
  public class BanCommand : RealmServerCommand
  {
    protected BanCommand()
    {
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("Ban");
      EnglishParamInfo = "[-[smhdw] [<seconds>] [<minutes>] [<hours>] [<days>] [<weeks>]]";
      EnglishDescription =
        "Deactivates the given Account. Reactivation time can optionally also be specified.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Character chr = trigger.Args.Target as Character;
      IUser banner = trigger.Args.User;
      if(chr != null && ReferenceEquals(chr, banner))
        chr = chr.Target as Character;
      if(chr == null || ReferenceEquals(chr, banner))
        trigger.Reply("Invalid Target.");
      else if(banner != null && chr.Role >= banner.Role)
      {
        trigger.Reply("Cannot ban Users of higher or equal Rank.");
      }
      else
      {
        TimeSpan? nullable1 = trigger.Text.NextTimeSpan();
        DateTime? until;
        if(nullable1.HasValue)
        {
          DateTime now = DateTime.Now;
          TimeSpan? nullable2 = nullable1;
          until = nullable2.HasValue ? now + nullable2.GetValueOrDefault() : new DateTime?();
        }
        else
          until = new DateTime?();

        string timeStr = until.HasValue ? "until " + until : "(indefinitely)";
        trigger.Reply("Banning Account {0} ({1}) {2}...", (object) chr.Account.Name, (object) chr.Name,
          (object) timeStr);
        ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() =>
        {
          IContextHandler contextHandler = chr.ContextHandler;
          RealmAccount account = chr.Account;
          if(account == null || contextHandler == null)
            trigger.Reply("Character logged off.");
          else if(account.SetAccountActive(false, until))
            contextHandler.AddMessage(() =>
            {
              if(chr.IsInWorld)
                chr.Kick(banner, "Banned " + timeStr, 5);
              trigger.Reply("Done.");
            });
          else
            trigger.Reply("Could not ban Account.");
        }));
      }
    }
  }
}