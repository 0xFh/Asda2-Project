using System;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.GameObjects
{
    public abstract class GameObjectHandler
    {
        protected GameObject m_go;

        /// <summary>The unit who is currently using the GO</summary>
        protected Unit m_user;

        public GameObject GO
        {
            get { return this.m_go; }
        }

        /// <summary>Whether this GO can be used by the given user</summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool CanBeUsedBy(Character chr)
        {
            if (!chr.CanSee((WorldObject) this.m_go) || this.m_go.State == GameObjectState.Disabled)
                return false;
            if (chr.GodMode)
                return true;
            if (this.m_go.Faction != Faction.NullFaction && this.m_go.Faction.Group != (FactionGroup) 0 &&
                this.m_go.Faction.Group != chr.Faction.Group || this.m_go.Entry.IsPartyOnly &&
                this.m_go.Owner != null && !this.m_go.Owner.IsAlliedWith((IFactionMember) chr))
                return false;
            Character character = chr;
            if (!character.IsAlive || !character.CanInteract)
                return false;
            return this.m_go.IsInRadiusSq((IHasPosition) chr, (float) GOMgr.DefaultInteractDistanceSq);
        }

        protected internal virtual void Initialize(GameObject go)
        {
            this.m_go = go;
        }

        /// <summary>
        /// Tries to use this Object and returns whether the user succeeded using it.
        /// </summary>
        public bool TryUse(Character user)
        {
            if (!this.CanBeUsedBy(user) || this.m_go.Flags.HasFlag((Enum) GameObjectFlags.InUse))
                return false;
            if (!this.m_go.Entry.AllowMounted)
                user.Dismount();
            LockEntry lockEntry = this.m_go.Entry.Lock;
            if (lockEntry != null && !lockEntry.RequiresAttack && lockEntry.RequiresKneeling)
                user.StandState = StandState.Kneeling;
            if (this.m_go.CanOpen(user))
                return this.DoUse(user);
            return false;
        }

        private bool DoUse(Character user)
        {
            if (this.Use(user))
                return this.m_go.Entry.NotifyUsed(this.m_go, user);
            return false;
        }

        /// <summary>Makes the given Unit use this GameObject</summary>
        public abstract bool Use(Character user);

        /// <summary>Called when the GameObject is being destroyed</summary>
        protected internal virtual void OnRemove()
        {
        }

        /// <summary>
        /// GO is being removed -&gt; Clean up everthing that needs cleanup
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}