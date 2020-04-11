using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Guilds;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class GuildCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Guild");
    }

    public class CreateGuildCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Create", "C");
        EnglishParamInfo = "[-[n <leadername>]] <name>";
        EnglishDescription = "Create a guild with given name. -n allows you to select the leader by name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string str = trigger.Text.NextModifiers();
        string leaderName = "";
        if(str.Contains("n"))
          leaderName = trigger.Text.NextWord();
        string name = trigger.Text.NextWord().Trim();
        if(!GuildMgr.CanUseName(name))
          trigger.Reply("Invalid name: " + name);
        else if(leaderName.Length > 0)
        {
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            CharacterRecord recordByName = CharacterRecord.GetRecordByName(leaderName);
            if(recordByName == null)
              trigger.Reply("Character with name \"{0}\" does not exist.", (object) leaderName);
            else
              CreateGuild(trigger, name, recordByName);
          });
        }
        else
        {
          Character target = trigger.Args.Target as Character;
          if(target != null)
          {
            CharacterRecord record = target.Record;
            CreateGuild(trigger, name, record).Leader.Character = target;
          }
          else
            trigger.Reply(
              "Could not create Guild. You did not select a Character to be the Guild leader. Use the -n switch to specify the leader by name.");
        }
      }

      private Guild CreateGuild(CmdTrigger<RealmServerCmdArgs> trigger, string name, CharacterRecord record)
      {
        Guild guild = new Guild(record, name);
        trigger.Reply("Guild created");
        return guild;
      }
    }

    public class SetLevelCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("SetLevel", "SL");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = trigger.Text.NextInt();
        Character target = trigger.Args.Target as Character;
        if(target == null || !target.IsInGuild)
        {
          trigger.Reply("Target is not in guild.");
        }
        else
        {
          switch(num)
          {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
              target.Guild.Level = (byte) num;
              trigger.Reply("Done.");
              break;
            default:
              trigger.Reply("Incorrect guild level.");
              break;
          }
        }
      }
    }

    public class PointsCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("points", "p");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = trigger.Text.NextInt();
        trigger.Args.Character.GuildPoints += num;
        trigger.Reply("Done.");
      }
    }

    public class JoinGuildCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Add", "A");
        EnglishParamInfo = "[-[n <membername>]] <name>";
        EnglishDescription =
          "Let somebody join the guild with the given name. -n allows you to select the new member by name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string str = trigger.Text.NextModifiers();
        string memberName = "";
        if(str.Contains("n"))
          memberName = trigger.Text.NextWord();
        string name = trigger.Text.NextWord().Trim();
        Guild guild = GuildMgr.GetGuild(name);
        if(guild == null)
          trigger.Reply("Guild does not exist: " + name);
        else if(memberName.Length > 0)
        {
          ServerApp<RealmServer>.IOQueue.AddMessage(() =>
          {
            CharacterRecord recordByName = CharacterRecord.GetRecordByName(memberName);
            if(recordByName == null)
              trigger.Reply("Character with name \"{0}\" does not exist.", (object) memberName);
            else
              guild.AddMember(recordByName);
          });
        }
        else
        {
          Character target = trigger.Args.Target as Character;
          if(target == null)
            trigger.Reply(
              "You did not select a valid member. Use the -n switch to specify the new member by name.");
          else
            guild.AddMember(target);
        }
      }
    }

    public class GuildPromoteCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Promote", "P");
        EnglishParamInfo = "<guild name>";
        EnglishDescription = "Promotes a member of a guild with the given name to the next rank.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string name = trigger.Text.NextWord().Trim();
        if(GuildMgr.GetGuild(name) == null)
        {
          trigger.Reply("Guild does not exist: " + name);
        }
        else
        {
          Character target = trigger.Args.Target as Character;
          if(target == null)
          {
            trigger.Reply("You did not select a valid member.");
          }
          else
          {
            if(target.GuildMember.RankId <= 0)
              return;
            --target.GuildMember.RankId;
          }
        }
      }
    }

    public class LeaveGuildCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Leave", "L");
        EnglishParamInfo = "[-[n <membername>]] <name>";
        EnglishDescription =
          "Let somebody leave the guild with the given name. -n allows you to select the member by name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string str = trigger.Text.NextModifiers();
        string name1 = "";
        if(str.Contains("n"))
          name1 = trigger.Text.NextWord();
        string name2 = trigger.Text.NextWord().Trim();
        Guild guild = GuildMgr.GetGuild(name2);
        if(guild == null)
        {
          trigger.Reply("Guild does not exist: " + name2);
        }
        else
        {
          if(name1.Length > 0)
          {
            if(!guild.RemoveMember(name1, false))
            {
              trigger.Reply("{0} is not a member of \"{1}\".", (object) name1, (object) guild.Name);
              return;
            }
          }
          else
          {
            Character target = trigger.Args.Target as Character;
            if(target == null)
            {
              trigger.Reply(
                "You did not select a valid member. Use the -n switch to specify the new member by name.");
              return;
            }

            if(target.GuildMember == null || !guild.RemoveMember(target.GuildMember))
            {
              trigger.Reply("{0} is not a member of \"{1}\".", (object) target.Name, (object) guild.Name);
              return;
            }
          }

          trigger.Reply("Done.");
        }
      }
    }

    public class GuildListCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("List", "L");
        EnglishParamInfo = "[<searchterm>]";
        EnglishDescription = "Lists all Guild or only those with a matching name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        IEnumerable<Guild> source;
        if(trigger.Text.HasNext)
        {
          string searchTerm = trigger.Text.Remainder;
          source = GuildMgr.GuildsById.Values.Where(gld =>
            gld.Name.ContainsIgnoreCase(searchTerm));
        }
        else
          source = GuildMgr.GuildsById.Values;

        int num = source.Count();
        if(num == 0)
        {
          trigger.Reply("No Guilds found.");
        }
        else
        {
          trigger.Reply("{0} Guilds found:", (object) num);
          foreach(Guild guild in source)
            trigger.Reply(guild.ToString());
        }
      }
    }

    public class DisbandGuildCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Disband", "D");
        EnglishParamInfo = "<name> <name confirm>";
        EnglishDescription = "Disbands the guild with the given name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        trigger.Text.NextModifiers();
        string name = trigger.Text.NextWord().Trim();
        if(trigger.Text.NextWord().Trim() != name)
        {
          trigger.Reply("The confirmation name did not match the name. Please type the name twice.");
        }
        else
        {
          Guild guild = GuildMgr.GetGuild(name);
          if(guild == null)
          {
            trigger.Reply("Guild does not exist: " + name);
          }
          else
          {
            guild.Disband();
            trigger.Reply("{0} has been disbanded.", (object) guild.Name);
          }
        }
      }
    }

    public class GuildChatCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Say", "Msg");
        EnglishParamInfo = "[-n <name>] <text>";
        EnglishDescription =
          "Sends the given text to your, your target's or the specified Guild. -n can be ommited if not used by/on a Character.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string str = trigger.Text.NextModifiers();
        Unit target = trigger.Args.Target;
        Guild guild;
        if(!(target is Character) || str == "n")
        {
          string name = trigger.Text.NextWord().Trim();
          guild = GuildMgr.GetGuild(name);
          if(guild == null)
          {
            trigger.Reply("Invalid Guild: " + name);
            return;
          }
        }
        else
        {
          guild = ((Character) target).Guild;
          if(guild == null)
          {
            trigger.Reply(target + " is not a member of any Guild.");
            return;
          }
        }

        guild.SendMessage(trigger.Args.User, trigger.Text.Remainder);
      }
    }
  }
}