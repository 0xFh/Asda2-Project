using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    /// <summary>
    /// Can be used to create custom GameObjects that will apply the given
    /// Spell to everyone in Radius.
    /// </summary>
    public class GOCustomAreaEffectEntry : GOCustomEntry
    {
        private GOCustomAreaEffectEntry.GOInteractionHandler m_AreaEffectHandler;
        protected float m_Radius;
        protected float m_RadiusSq;

        public GOCustomAreaEffectEntry()
        {
            this.Radius = 5f;
            this.UpdateDelayMillis = 500;
        }

        public float Radius
        {
            get { return this.m_Radius; }
            set
            {
                this.m_Radius = value;
                this.m_RadiusSq = this.m_Radius * this.m_Radius;
            }
        }

        public int UpdateDelayMillis { get; set; }

        /// <summary>
        /// The EffectHandler that will be applied to every Unit that comes into the Radius.
        /// When moving, removing or adding anything in this Method, enqueue a Message!
        /// </summary>
        public GOCustomAreaEffectEntry.GOInteractionHandler AreaEffectHandler
        {
            get { return this.m_AreaEffectHandler; }
            set { this.m_AreaEffectHandler = value; }
        }

        protected internal override void InitGO(GameObject go)
        {
            go.SetUpdatePriority(UpdatePriority.VeryLowPriority);
            if (this.m_AreaEffectHandler == null)
                return;
            go.CallPeriodically(this.UpdateDelayMillis, new Action<WorldObject>(this.ApplyEffectsToArea));
        }

        protected void ApplyEffectsToArea(WorldObject goObj)
        {
            GameObject go = (GameObject) goObj;
            goObj.IterateEnvironment(this.Radius, (Func<WorldObject, bool>) (obj =>
            {
                if (obj is Character)
                    this.AreaEffectHandler(go, (Character) obj);
                return true;
            }));
        }

        public delegate void GOInteractionHandler(GameObject go, Character chr);
    }
}