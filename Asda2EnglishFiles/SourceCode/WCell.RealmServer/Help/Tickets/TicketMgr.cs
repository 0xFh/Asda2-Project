using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WCell.RealmServer.Help.Tickets
{
    /// <summary>
    /// Manages support, help-requests, GM Tickets, surveys etc.
    /// 
    /// TODO: Commands - Summon, Goto, Next, Previous
    /// TODO: Staff chat-channeling for enforced help
    /// TODO: Individual settings for staff/ticket issuing? Use ticket types?
    /// TODO: Backup command to store certain tickets?
    /// </summary>
    public class TicketMgr
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static readonly TicketMgr Instance = new TicketMgr();
        internal readonly Dictionary<uint, Ticket> ticketsById = new Dictionary<uint, Ticket>();
        internal readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();
        internal Ticket first;
        internal Ticket last;

        public Ticket First
        {
            get { return this.first; }
        }

        public Ticket Last
        {
            get { return this.last; }
        }

        public int TicketCount
        {
            get { return this.ticketsById.Count; }
        }

        public void AddTicket(Ticket ticket)
        {
            this.lck.EnterWriteLock();
            try
            {
                if (this.first == null)
                {
                    this.first = this.last = ticket;
                }
                else
                {
                    this.last.m_next = ticket;
                    ticket.m_previous = this.last;
                    this.last = ticket;
                }

                this.ticketsById.Add(ticket.CharId, ticket);
            }
            finally
            {
                this.lck.ExitWriteLock();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="charId"></param>
        /// <returns>The Ticket of the Character with the given charId or null if the Character did not issue any.</returns>
        public Ticket GetTicket(uint charId)
        {
            this.lck.EnterReadLock();
            try
            {
                Ticket ticket;
                this.ticketsById.TryGetValue(charId, out ticket);
                return ticket;
            }
            finally
            {
                this.lck.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler">May be null</param>
        /// <returns>Returns the next Ticket that is not already handled and may be handled by the given handler.</returns>
        public Ticket GetNextUnhandledTicket(ITicketHandler handler)
        {
            this.lck.EnterReadLock();
            try
            {
                for (Ticket ticket = this.first; ticket != null; ticket = ticket.Next)
                {
                    if (handler == null || handler.MayHandle(ticket))
                        return ticket;
                }
            }
            finally
            {
                this.lck.ExitReadLock();
            }

            return (Ticket) null;
        }

        public Ticket HandleNextUnhandledTicket(ITicketHandler handler)
        {
            Ticket nextUnhandledTicket;
            do
            {
                nextUnhandledTicket = this.GetNextUnhandledTicket(handler);
                if (nextUnhandledTicket != null)
                {
                    this.lck.EnterWriteLock();
                    try
                    {
                        if (nextUnhandledTicket.Handler == null)
                        {
                            nextUnhandledTicket.SetHandlerUnlocked(handler);
                            break;
                        }
                    }
                    finally
                    {
                        this.lck.ExitWriteLock();
                    }
                }
            } while (nextUnhandledTicket != null);

            return nextUnhandledTicket;
        }

        public bool HandleTicket(ITicketHandler handler, Ticket ticket)
        {
            if (!handler.MayHandle(ticket))
                return false;
            ticket.Handler = handler;
            return true;
        }

        public Ticket[] GetAllTickets()
        {
            this.lck.EnterReadLock();
            Ticket[] ticketArray = new Ticket[this.ticketsById.Count];
            try
            {
                Ticket ticket = this.first;
                int num = 0;
                for (; ticket != null; ticket = ticket.Next)
                    ticketArray[num++] = ticket;
            }
            finally
            {
                this.lck.ExitReadLock();
            }

            return ticketArray;
        }

        public IEnumerator<Ticket> GetEnumerator()
        {
            return (IEnumerator<Ticket>) new TicketMgr.TicketEnumerator(this);
        }

        private class TicketEnumerator : IEnumerator<Ticket>, IDisposable, IEnumerator
        {
            private TicketMgr mgr;
            private Ticket current;

            public TicketEnumerator(TicketMgr mgr)
            {
                this.mgr = mgr;
            }

            public void Dispose()
            {
                this.mgr = (TicketMgr) null;
                this.current = (Ticket) null;
            }

            public bool MoveNext()
            {
                if (this.current == null)
                {
                    this.current = this.mgr.first;
                    return this.current != null;
                }

                if (this.current.Next == null)
                    return false;
                this.current = this.current.Next;
                return true;
            }

            public void Reset()
            {
                this.current = (Ticket) null;
            }

            public Ticket Current
            {
                get { return this.current; }
            }

            object IEnumerator.Current
            {
                get { return (object) this.current; }
            }
        }
    }
}