using System;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    public class InstancedMap : Map
    {
        protected internal uint m_InstanceId;
        protected DateTime m_creationTime;

        protected InstancedMap()
        {
            this.m_creationTime = DateTime.Now;
            this.XpCalculator = new ExperienceCalculator(XpGenerator.CalcDefaultXp);
        }

        protected internal override void InitMap()
        {
            this.m_InstanceId = this.m_MapTemplate.NextId();
            base.InitMap();
        }

        /// <summary>
        /// The instances unique identifier, raid and instance IDs are seperate
        /// </summary>
        public override uint InstanceId
        {
            get { return this.m_InstanceId; }
        }

        /// <summary>
        /// Whether this Instance is active.
        /// Once the last boss has been killed, an instance turns inactive.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>Instances are stopped manually</summary>
        public override bool ShouldStop
        {
            get { return false; }
        }

        /// <summary>If its a saving type instance, raid or heroic</summary>
        public bool IsRaid
        {
            get { return this.m_MapTemplate.Type == MapType.Raid; }
        }

        /// <summary>If its a PVP area, BattleGround or Arena</summary>
        public bool IsPVPArea
        {
            get
            {
                if (this.m_MapTemplate.Type != MapType.Battleground)
                    return this.m_MapTemplate.Type == MapType.Arena;
                return true;
            }
        }

        public override DateTime CreationTime
        {
            get { return this.m_creationTime; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public virtual bool CanReset(Character chr)
        {
            return chr.Role.IsStaff;
        }

        protected virtual void OnTimeout(int timeElapsed)
        {
            Map.s_log.Debug("{0} #{1} timeout.", (object) this.Name, (object) this.m_InstanceId);
            this.Delete();
        }

        /// <summary>Teleports player into an instance</summary>
        /// <param name="chr"></param>
        public virtual void TeleportInside(Character chr)
        {
            this.TeleportInside(chr, 0);
        }

        public void TeleportInside(Character chr, int entrance)
        {
            if ((long) (uint) entrance >= (long) this.m_MapTemplate.EntrancePositions.Length)
                entrance = 0;
            this.TeleportInside(chr, this.m_MapTemplate.EntrancePositions[entrance]);
        }

        public void TeleportInside(Character chr, Vector3 pos)
        {
            chr.TeleportTo((Map) this, ref pos);
            chr.SendSystemMessage("Welcome to {0} #{1} (created at {2})", (object) this.Name, (object) this.InstanceId,
                (object) this.m_creationTime.ToString());
        }

        public override void RemoveAll()
        {
            base.RemoveAll();
            foreach (Character character in this.m_characters)
                this.TeleportOutside(character);
        }

        public virtual void Delete()
        {
            if (!this.IsRunning)
                this.DeleteNow();
            else
                this.AddMessage(new Action(this.DeleteNow));
        }

        public virtual void DeleteNow()
        {
            this.EnsureContext();
            this.EnsureNotUpdating();
            this.Stop();
            this.RemoveAll();
            this.IsDisposed = true;
            this.m_MapTemplate.RecycleId(this.m_InstanceId);
        }
    }
}