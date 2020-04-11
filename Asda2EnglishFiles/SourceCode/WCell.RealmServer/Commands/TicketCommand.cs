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
            this.Init("Ticket", "Tickets");
            this.EnglishDescription = "Provides all commands necessary to work with Tickets.";
        }

        public override bool MayTrigger(CmdTrigger<RealmServerCmdArgs> trigger, BaseCommand<RealmServerCmdArgs> command,
            bool silent)
        {
            if (command is TicketCommand.TicketSubCmd)
            {
                TicketCommand.TicketSubCmd ticketSubCmd = (TicketCommand.TicketSubCmd) command;
                if ((ticketSubCmd.RequiresTicketHandler || ticketSubCmd.RequiresActiveTicket) &&
                    trigger.Args.TicketHandler == null)
                {
                    if (!silent)
                    {
                        trigger.Reply("Cannot use Command in this Context (TicketHandler required)");
                        RealmCommandHandler.Instance.DisplayCmd(trigger, (BaseCommand<RealmServerCmdArgs>) this, false,
                            false);
                    }

                    return false;
                }

                if (ticketSubCmd.RequiresActiveTicket && trigger.Args.TicketHandler.HandlingTicket == null)
                {
                    if (!silent)
                    {
                        trigger.Reply("You do not have a Ticket selected. Use the \"Next\" command first:");
                        TicketCommand.SelectNextTicketCommand nextTicketCommand =
                            RealmCommandHandler.Instance.Get<TicketCommand.SelectNextTicketCommand>();
                        RealmCommandHandler.Instance.DisplayCmd(trigger,
                            (BaseCommand<RealmServerCmdArgs>) nextTicketCommand, false, true);
                    }

                    return false;
                }
            }

            return true;
        }

        public abstract class TicketSubCmd : RealmServerCommand.SubCommand
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

        public class SelectPlayerTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Select", "Sel");
                this.EnglishParamInfo = "[<playername>]";
                this.EnglishDescription =
                    "Selects the Ticket of the targeted Player or the Player with the given name.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string playerName = trigger.Text.NextWord();
                Character argumentOrTarget = trigger.Args.GetCharArgumentOrTarget(trigger, playerName);
                ITicketHandler ticketHandler = trigger.Args.TicketHandler;
                if (argumentOrTarget != null && argumentOrTarget.IsInWorld)
                {
                    Ticket ticket = argumentOrTarget.Ticket;
                    ITicketHandler handler = ticket.Handler;
                    if (handler != null && handler.Role > ticketHandler.Role)
                    {
                        trigger.Reply("Ticket is already being handled by: " + handler.Name);
                    }
                    else
                    {
                        if (handler != null)
                        {
                            trigger.Reply("Taking over Ticket from: " + handler.Name);
                            handler.SendMessage("The Ticket you were handling by " + (object) ticket.Owner +
                                                " is now handled by: " + (object) ticketHandler);
                        }

                        ticket.Handler = ticketHandler;
                    }
                }
                else
                    trigger.Reply("Selected player is offline or does not exist: " + playerName);
            }
        }

        public class SelectNextTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("SelectNext", "SelNext", "Next", "N");
                this.EnglishDescription = "Selects the next unhandled ticket.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Ticket ticket = TicketMgr.Instance.HandleNextUnhandledTicket(trigger.Args.TicketHandler);
                if (ticket == null)
                    trigger.Reply("There are currently no unhandled Tickets.");
                else
                    ticket.DisplayFormat((ITriggerer) trigger, "Now Selected: ");
            }
        }

        public class UnselectTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Unselect", "U");
                this.EnglishDescription = "Unselects the current Ticket.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Args.TicketHandler.HandlingTicket.Handler = (ITicketHandler) null;
                trigger.Reply("Done.");
            }

            public override bool RequiresActiveTicket
            {
                get { return true; }
            }
        }

        public class GotoTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Goto", "Go", "Tele");
                this.EnglishDescription =
                    "Teleports directly to the ticket issuer or his/her ticket posting location if offline.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Ticket handlingTicket = trigger.Args.TicketHandler.HandlingTicket;
                Character owner = handlingTicket.Owner;
                if (owner == null)
                {
                    trigger.Reply("The owner of this Ticket is offline.");
                    trigger.Args.Target.TeleportTo(handlingTicket.Map, handlingTicket.Position);
                }
                else
                    trigger.Args.Target.TeleportTo((IWorldLocation) owner);
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

        public class NotifyTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Notify", "Msg", "M");
                this.EnglishDescription = "Sends a notification to the person who issued the current ticket.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character owner = trigger.Args.TicketHandler.HandlingTicket.Owner;
                if (owner == null)
                {
                    trigger.Reply("The owner of this Ticket is offline.");
                }
                else
                {
                    string text = trigger.Text.Remainder;
                    owner.ExecuteInContext((Action) (() => owner.Notify(text)));
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

        public class ListTicketsCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("List", "L");
                this.EnglishDescription = "Shows all currently active tickets.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Ticket[] allTickets = TicketMgr.Instance.GetAllTickets();
                if (allTickets.Length == 0)
                {
                    trigger.Reply("There are no active tickets.");
                }
                else
                {
                    foreach (Ticket ticket in allTickets)
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

        public class CurrentTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Current", "Cur");
                this.EnglishDescription = "Shows the Ticket you are currently handling.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Args.TicketHandler.HandlingTicket.DisplayFormat((ITriggerer) trigger, "Now Selected: ");
            }

            public override bool RequiresActiveTicket
            {
                get { return true; }
            }
        }

        public class ShowTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Show", "S");
                this.EnglishDescription = "Shows the Target's currently active Ticket.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character target = trigger.Args.Target as Character;
                if (target == null)
                {
                    trigger.Reply("Invalid selection.");
                }
                else
                {
                    Ticket ticket = target.Ticket;
                    if (ticket != null)
                        ticket.DisplayFormat((ITriggerer) trigger, "");
                    else
                        trigger.Reply("{0} has no active Ticket.", (object) target.Name);
                }
            }

            public override bool RequiresIngameTarget
            {
                get { return true; }
            }
        }

        public class DeleteTicketCommand : TicketCommand.TicketSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Delete", "Del", "D", "Remove", "R");
                this.EnglishDescription = "Deletes the current Ticket.";
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