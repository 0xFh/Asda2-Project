using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    /// <summary>Abstract atomary action of AI</summary>
    public abstract class AIAction : IAIAction, IDisposable
    {
        protected Unit m_owner;

        protected AIAction(Unit owner)
        {
            this.m_owner = owner;
        }

        protected AIAction()
        {
        }

        /// <summary>Owner of the action</summary>
        public Unit Owner
        {
            get { return this.m_owner; }
        }

        public bool UsesSpells
        {
            get { return this.m_owner.HasSpells; }
        }

        public bool HasSpellReady
        {
            get { return ((NPC) this.m_owner).NPCSpells.ReadyCount > 0; }
        }

        public virtual bool IsGroupAction
        {
            get { return false; }
        }

        /// <summary>What can break the action.</summary>
        public virtual ProcTriggerFlags InterruptFlags
        {
            get { return ProcTriggerFlags.None; }
        }

        /// <summary>Start a new Action</summary>
        /// <returns></returns>
        public abstract void Start();

        /// <summary>Update</summary>
        /// <returns></returns>
        public abstract void Update();

        /// <summary>
        /// Stop (usually called before switching to another Action)
        /// </summary>
        /// <returns></returns>
        public abstract void Stop();

        public abstract UpdatePriority Priority { get; }

        public virtual void Dispose()
        {
        }
    }
}