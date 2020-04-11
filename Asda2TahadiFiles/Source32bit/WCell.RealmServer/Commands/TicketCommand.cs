using System;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
  public class TicketCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Ticket", "Tickets");
      EnglishDescription = "Provides all commands necessary to work with Tickets.";
    }

    public override bool MayTrigger(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> command,
      bool silent)
    {
      if(command is TicketSubCmd)
      {
        TicketSubCmd ticketSubCmd = (TicketSubCmd) command;
        if((ticketSubCmd.RequiresTicketHandler || ticketSubCmd.RequiresActiveTicket) &&
           trigger.Args.TicketHandler == null)
        {
          if(!silent)
          {
            trigger.Reply("Cannot use Command in this Context (TicketHandler required)");
            RealmCommandHandler.Instance.DisplayCmd(trigger, this, false,
              false);
          }

          return false;
        }

        if(ticketSubCmd.RequiresActiveTicket && trigger.Args.TicketHandler.HandlingTicket == null)
        {
          if(!silent)
          {
            trigger.Reply("You do not have a Ticket selected. Use the \"Next\" command first:");
            SelectNextTicketCommand nextTicketCommand =
              RealmCommandHandler.Instance.Get<SelectNextTicketCommand>();
            RealmCommandHandler.Instance.DisplayCmd(trigger,
              nextTicketCommand, false, true);
          }

          return false;
        }
      }

      return true;
    }

    public abstract class TicketSubCmd : SubCommand
    {
      public virtual bool RequiresTicketHandler
      {
        get { return true; }
      }

      public virtual bool RequiresActiveTicket
      {
        get { return false; }
      }

      public virtual bool RequiresIngameTarget
      {
        get { return false; }
      }
    }

    public class SelectPlayerTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Select", "Sel");
        EnglishParamInfo = "[<playername>]";
        EnglishDescription =
          "Selects the Ticket of the targeted Player or the Player with the given name.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        string playerName = trigger.Text.NextWord();
        Character argumentOrTarget = trigger.Args.GetCharArgumentOrTarget(trigger, playerName);
        ITicketHandler ticketHandler = trigger.Args.TicketHandler;
        if(argumentOrTarget != null && argumentOrTarget.IsInWorld)
        {
          Ticket ticket = argumentOrTarget.Ticket;
          ITicketHandler handler = ticket.Handler;
          if(handler != null && handler.Role > ticketHandler.Role)
          {
            trigger.Reply("Ticket is already being handled by: " + handler.Name);
          }
          else
          {
            if(handler != null)
            {
              trigger.Reply("Taking over Ticket from: " + handler.Name);
              handler.SendMessage("The Ticket you were handling by " + ticket.Owner +
                                  " is now handled by: " + ticketHandler);
            }

            ticket.Handler = ticketHandler;
          }
        }
        else
          trigger.Reply("Selected player is offline or does not exist: " + playerName);
      }
    }

    public class SelectNextTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("SelectNext", "SelNext", "Next", "N");
        EnglishDescription = "Selects the next unhandled ticket.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Ticket ticket = TicketMgr.Instance.HandleNextUnhandledTicket(trigger.Args.TicketHandler);
        if(ticket == null)
          trigger.Reply("There are currently no unhandled Tickets.");
        else
          ticket.DisplayFormat(trigger, "Now Selected: ");
      }
    }

    public class UnselectTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Unselect", "U");
        EnglishDescription = "Unselects the current Ticket.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        trigger.Args.TicketHandler.HandlingTicket.Handler = null;
        trigger.Reply("Done.");
      }

      public override bool RequiresActiveTicket
      {
        get { return true; }
      }
    }

    public class GotoTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Goto", "Go", "Tele");
        EnglishDescription =
          "Teleports directly to the ticket issuer or his/her ticket posting location if offline.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Ticket handlingTicket = trigger.Args.TicketHandler.HandlingTicket;
        Character owner = handlingTicket.Owner;
        if(owner == null)
        {
          trigger.Reply("The owner of this Ticket is offline.");
          trigger.Args.Target.TeleportTo(handlingTicket.Map, handlingTicket.Position);
        }
        else
          trigger.Args.Target.TeleportTo(owner);
      }

      public override bool RequiresActiveTicket
      {
        get { return true; }
      }

      public override bool RequiresIngameTarget
      {
        get { return true; }
      }
    }

    public class NotifyTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Notify", "Msg", "M");
        EnglishDescription = "Sends a notification to the person who issued the current ticket.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Character owner = trigger.Args.TicketHandler.HandlingTicket.Owner;
        if(owner == null)
        {
          trigger.Reply("The owner of this Ticket is offline.");
        }
        else
        {
          string text = trigger.Text.Remainder;
          owner.ExecuteInContext(() => owner.Notify(text));
          trigger.Reply(RealmLangKey.Done);
        }
      }

      public override bool RequiresActiveTicket
      {
        get { return true; }
      }

      public override bool RequiresIngameTarget
      {
        get { return true; }
      }
    }

    public class ListTicketsCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("List", "L");
        EnglishDescription = "Shows all currently active tickets.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Ticket[] allTickets = TicketMgr.Instance.GetAllTickets();
        if(allTickets.Length == 0)
        {
          trigger.Reply("There are no active tickets.");
        }
        else
        {
          foreach(Ticket ticket in allTickets)
            trigger.Reply("{0} by {1}{2} (age: {3})", (object) ticket.Type, (object) ticket.OwnerName,
              ticket.Owner != null
                ? (object) ""
                : (object) ChatUtility.Colorize(" (Offline)", Color.Red, true), (object) ticket.Age);
        }
      }

      public override bool RequiresTicketHandler
      {
        get { return false; }
      }
    }

    public class CurrentTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Current", "Cur");
        EnglishDescription = "Shows the Ticket you are currently handling.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        trigger.Args.TicketHandler.HandlingTicket.DisplayFormat(trigger, "Now Selected: ");
      }

      public override bool RequiresActiveTicket
      {
        get { return true; }
      }
    }

    public class ShowTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Show", "S");
        EnglishDescription = "Shows the Target's currently active Ticket.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Character target = trigger.Args.Target as Character;
        if(target == null)
        {
          trigger.Reply("Invalid selection.");
        }
        else
        {
          Ticket ticket = target.Ticket;
          if(ticket != null)
            ticket.DisplayFormat(trigger, "");
          else
            trigger.Reply("{0} has no active Ticket.", (object) target.Name);
        }
      }

      public override bool RequiresIngameTarget
      {
        get { return true; }
      }
    }

    public class DeleteTicketCommand : TicketSubCmd
    {
      protected override void Initialize()
      {
        Init("Delete", "Del", "D", "Remove", "R");
        EnglishDescription = "Deletes the current Ticket.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Ticket handlingTicket = trigger.Args.TicketHandler.HandlingTicket;
        trigger.Reply(handlingTicket.OwnerName + "'s Ticket has been deleted.");
        handlingTicket.Delete();
      }

      public override bool RequiresActiveTicket
      {
        get { return true; }
      }
    }
  }
}