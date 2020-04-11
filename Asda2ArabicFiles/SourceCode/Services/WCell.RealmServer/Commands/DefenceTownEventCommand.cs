using System.Globalization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Events.Asda2.Managers;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class DefenceTownEventCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("defencetown", "dtown");
    }

    public class StartCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("start");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var map = trigger.Args.Character.Map;
        if (map.DefenceTownEvent != null)
        {
          trigger.Reply(string.Format("Defence town event in {0} is already started.", map.Name));
          return;
        }
        var minLevel = trigger.Text.NextInt(10);
        var maxLevel = trigger.Text.NextInt(30);
        var amountMod = trigger.Text.NextFloat(1);
        var healthMod = trigger.Text.NextFloat(1);
        var otherStatsMod = trigger.Text.NextFloat(1);
        var speedMod = trigger.Text.NextFloat(1);
        var difficulty = trigger.Text.NextFloat(1);

        if (difficulty > CharacterFormulas.MaxDeffenceDownEventDifficulty)
        {
          difficulty = CharacterFormulas.MaxDeffenceDownEventDifficulty;
        }
        if (difficulty < 1)
        {
          difficulty = 1;
        }
        DeffenceTownEventManager.Start(map, minLevel, maxLevel, amountMod, healthMod, otherStatsMod, speedMod,
          difficulty);
        trigger.Reply("Ok, defence town event started. Town is {0}, dificulty is {1}. [{2}-{3}]Level", map.Name,
          difficulty, minLevel, maxLevel);
      }
    }

    public class StopCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("stop");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var map = trigger.Args.Character.Map;
        if (map.DefenceTownEvent == null)
        {
          trigger.Reply("Defence town event in not running.");
          return;
        }
        DeffenceTownEventManager.Stop(map, trigger.Text.NextInt(0) != 0);
        trigger.Reply("Guess word event stoped.");
      }
    }
  }

  public class EventCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("evt");
    }

    public class TransformCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("transform", "trfm");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var id = trigger.Text.NextInt(1);
        var mod = trigger.Text.NextModifiers();
        if (mod == "t")
        {
          var stopId = trigger.Text.NextInt(800);
          var delay = trigger.Text.NextInt(2500);
          var n = 0;
          for (var i = id; i < stopId; i++)
          {
            n++;
            var i1 = i;
            trigger.Args.Character.CallDelayed(n*delay, o =>
            {
              TransformCharacters(trigger.Args.Character, mod, i1);
              trigger.Args.Character.SendInfoMsg(i1.ToString(CultureInfo.InvariantCulture));
            });
          }
        }
        else
        {
          TransformCharacters(trigger.Args.Character, mod, id);
        }
      }

      private static void TransformCharacters(Character c, string mod, int id)
      {
        foreach (var chr in c.GetNearbyCharacters())
        {
          if (mod == "r")
          {
            chr.TransformationId = 0;
            chr.TransformationId = (short) Utility.Random(1, 800);
          }
          else
          {
            chr.TransformationId = 0;
            chr.TransformationId = (short) id;
          }
        }
      }
    }

    public class DanceCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("dance", "dnc", "d");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var movId = (short) trigger.Text.NextInt(1);

        foreach (var chr in trigger.Args.Character.GetNearbyCharacters(true))
        {
          Asda2CharacterHandler.SendEmoteResponse(chr, movId);
        }
      }
    }

    public class LuckyDropCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("luckydrop", "ld");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Asda2LootMgr.EnableLuckyDropEvent();
        trigger.Reply("done");
      }
    }
  }
}