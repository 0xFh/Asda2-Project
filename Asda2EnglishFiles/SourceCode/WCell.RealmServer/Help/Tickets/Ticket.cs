using System;
using WCell.Constants.Tickets;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Help.Tickets
{
    /// <summary>
    /// Represents a ticket issued by a player.
    /// TODO: Save to DB
    /// </summary>
    public class Ticket : IWorldLocation, IHasPosition
    {
        private uint m_charId;
        private Character m_owner;
        private string m_ownerName;
        private TicketType m_Type;
        private Map m_Map;
        private string m_Message;
        private DateTime m_Timestamp;
        internal Ticket m_previous;
        internal Ticket m_next;
        internal ITicketHandler m_handler;

        public event Ticket.TicketHandlerChangedHandler TicketHandlerChanged;

        public Ticket(Character chr, string message, TicketType type)
        {
            this.m_owner = chr;
            this.m_ownerName = chr.Name;
            this.m_charId = chr.EntityId.Low;
            this.m_Message = message;
            this.m_Map = chr.Map;
            this.Position = chr.Position;
            this.Phase = chr.Phase;
            this.m_Timestamp = DateTime.Now;
            this.m_Type = type;
        }

        public Ticket Previous
        {
            get { return this.m_previous; }
        }

        public Ticket Next
        {
            get { return this.m_next; }
        }

        /// <summary>
        /// The low part of the EntityId of the Character who issued this Ticket
        /// </summary>
        public uint CharId
        {
            get { return this.m_charId; }
            set { this.m_charId = value; }
        }

        /// <summary>The owner of this Ticket or null if not online</summary>
        public Character Owner
        {
            get { return this.m_owner; }
        }

        /// <summary>The owner of this Ticket or null if not online</summary>
        public string OwnerName
        {
            get { return this.m_ownerName; }
        }

        public TicketType Type
        {
            get { return this.m_Type; }
            set { this.m_Type = value; }
        }

        public MapId MapId
        {
            get { return this.m_Map.Id; }
        }

        /// <summary>
        /// The Map where this Ticket was submitted or the Owner last logged out.
        /// </summary>
        public Map Map
        {
            get { return this.m_Map; }
            set { this.m_Map = value; }
        }

        /// <summary>
        /// The Position where this Ticket was submitted or the Owner last logged out.
        /// </summary>
        public Vector3 Position { get; set; }

        public uint Phase { get; set; }

        public string Message
        {
            get { return this.m_Message; }
            set { this.m_Message = value; }
        }

        /// <summary>The time when the ticket was submitted</summary>
        public DateTime Timestamp
        {
            get { return this.m_Timestamp; }
            set { this.m_Timestamp = value; }
        }

        public string TimestampStr
        {
            get { return this.m_Timestamp.ToString(); }
        }

        /// <summary>
        /// The time between now and when the ticket was submitted
        /// </summary>
        public TimeSpan Age
        {
            get { return DateTime.Now - this.m_Timestamp; }
        }

        /// <summary>
        /// The current Handler of this Ticket.
        /// Setting the handler is synchronized and also automatically sets the Handler's current HandlingTicket to this.
        /// </summary>
        public ITicketHandler Handler
        {
            get { return this.m_handler; }
            set
            {
                if (this.m_handler == value)
                    return;
                TicketMgr.Instance.lck.EnterWriteLock();
                try
                {
                    this.SetHandlerUnlocked(value);
                }
                finally
                {
                    TicketMgr.Instance.lck.ExitWriteLock();
                }
            }
        }

        internal void SetHandlerUnlocked(ITicketHandler handler)
        {
            if (this.m_handler == handler)
                return;
            ITicketHandler handler1 = this.m_handler;
            this.m_handler = handler;
            if (handler != null)
                handler.HandlingTicket = this;
            Ticket.TicketHandlerChangedHandler ticketHandlerChanged = this.TicketHandlerChanged;
            if (ticketHandlerChanged == null)
                return;
            ticketHandlerChanged(this, handler1);
        }

        public void Delete()
        {
            TicketMgr.Instance.lck.EnterWriteLock();
            try
            {
                if (this.m_previous != null)
                    this.m_previous.m_next = this.m_next;
                if (this.m_next != null)
                    this.m_next.m_previous = this.m_previous;
                TicketMgr instance = TicketMgr.Instance;
                if (this == instance.first)
                    instance.first = (Ticket) null;
                if (this == instance.last)
                    instance.last = (Ticket) null;
                if (this.m_handler != null)
                {
                    this.m_handler.HandlingTicket = (Ticket) null;
                    this.m_handler = (ITicketHandler) null;
                }

                if (this.m_owner != null)
                    this.m_owner.Ticket = (Ticket) null;
                TicketMgr.Instance.ticketsById.Remove(this.m_charId);
            }
            finally
            {
                TicketMgr.Instance.lck.ExitWriteLock();
            }
        }

        public void Display(ITriggerer triggerer, string info)
        {
            triggerer.Reply("-------------");
            triggerer.Reply("| " + info + (object) this.Type + " in " + this.m_Map.Name + " |");
            triggerer.Reply("-------------------------------------------------------------------");
            triggerer.Reply("| by " + this.m_ownerName + (this.m_owner == null ? (object) " (Offline)" : (object) "") +
                            ", " + (object) this.Age + " ago.");
            triggerer.Reply("-------------------------------------------------------------------");
            triggerer.Reply(this.Message);
            triggerer.Reply("-------------------------------------------------------------------");
        }

        public void DisplayFormat(ITriggerer triggerer, string info)
        {
            triggerer.ReplyFormat("-------------");
            triggerer.ReplyFormat("| " + info + (object) this.Type + " in " + this.m_Map.Name + " |");
            triggerer.ReplyFormat("-------------------------------------------------------------------");
            triggerer.ReplyFormat("| by " + this.m_ownerName +
                                  (this.m_owner == null
                                      ? (object) ChatUtility.Colorize(" (Offline)", Color.Red, true)
                                      : (object) "") + ", " + (object) this.Age + " ago.");
            triggerer.ReplyFormat("-------------------------------------------------------------------");
            triggerer.ReplyFormat(this.Message);
            triggerer.ReplyFormat("-------------------------------------------------------------------");
        }

        internal void OnOwnerLogin(Character chr)
        {
            TicketMgr.Instance.lck.EnterWriteLock();
            try
            {
                this.m_owner = chr;
                if (this.m_handler == null)
                    return;
                this.m_handler.SendMessage("Owner of the Ticket you are handling came back -{0}-.",
                    (object) ChatUtility.Colorize("online", Color.Green));
            }
            finally
            {
                TicketMgr.Instance.lck.ExitWriteLock();
            }
        }

        internal void OnOwnerLogout()
        {
            TicketMgr.Instance.lck.EnterWriteLock();
            try
            {
                this.Position = this.m_owner.Position;
                this.m_Map = this.m_owner.Map;
                this.Phase = this.m_owner.Phase;
                this.m_owner = (Character) null;
                ITicketHandler handler = this.m_handler;
                if (handler == null)
                    return;
                handler.SendMessage("Owner of the Ticket you are handling went -{0}-.",
                    (object) ChatUtility.Colorize("offline", Color.Red));
            }
            finally
            {
                TicketMgr.Instance.lck.ExitWriteLock();
            }
        }

        public delegate void TicketHandlerChangedHandler(Ticket ticket, ITicketHandler oldHandler);
    }
}