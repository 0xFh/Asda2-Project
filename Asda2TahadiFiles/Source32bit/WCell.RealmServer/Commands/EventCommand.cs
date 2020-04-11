using System;
using System.Collections.Generic;
using System.Globalization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class EventCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("evt");
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    public class TransformCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("transform", "trfm");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int id = trigger.Text.NextInt(1);
        string mod = trigger.Text.NextModifiers();
        if(mod == "t")
        {
          int num1 = trigger.Text.NextInt(800);
          int num2 = trigger.Text.NextInt(2500);
          int num3 = 0;
          for(int index = id; index < num1; ++index)
          {
            ++num3;
            int i1 = index;
            trigger.Args.Character.CallDelayed(num3 * num2, o =>
            {
              TransformCharacters(trigger.Args.Character, mod, i1);
              trigger.Args.Character.SendInfoMsg(
                i1.ToString(CultureInfo.InvariantCulture));
            });
          }
        }
        else
          TransformCharacters(trigger.Args.Character, mod, id);
      }

      private static void TransformCharacters(Character c, string mod, int id)
      {
        foreach(Character nearbyCharacter in c.GetNearbyCharacters())
        {
          if(mod == "r")
          {
            nearbyCharacter.TransformationId = 0;
            nearbyCharacter.TransformationId = (short) Utility.Random(1, 800);
          }
          else
          {
            nearbyCharacter.TransformationId = 0;
            nearbyCharacter.TransformationId = (short) id;
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
        short emote = (short) trigger.Text.NextInt(1);
        foreach(Character nearbyCharacter in trigger.Args.Character
          .GetNearbyCharacters(true))
          Asda2CharacterHandler.SendEmoteResponse(nearbyCharacter, emote, 1, 0.0596617f, -0.9982219f);
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