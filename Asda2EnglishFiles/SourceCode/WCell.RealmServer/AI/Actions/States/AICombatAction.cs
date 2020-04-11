using NLog;
using System;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.AI.Actions.States
{
    /// <summary>
    /// An action of NPCs that selects targets, according to Threat and other factors and then
    /// updates the given <see cref="P:WCell.RealmServer.AI.Actions.States.AICombatAction.Strategy" /> to kill the current target.
    /// </summary>
    public class AICombatAction : AIAction, IAIStateAction, IAIAction, IDisposable
    {
        /// <summary>Only check for a Threat update every 20 ticks</summary>
        public static int ReevaluateThreatTicks = 20;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected AIAction m_Strategy;
        private bool m_init;

        public AICombatAction(NPC owner)
            : base((Unit) owner)
        {
        }

        public AICombatAction(NPC owner, AIAction combatAction)
            : base((Unit) owner)
        {
            this.m_Strategy = combatAction;
        }

        /// <summary>
        /// Action to be executed after the highest aggro target has been selected.
        /// Start/stop is called everytime the Target changes.
        /// </summary>
        public AIAction Strategy
        {
            get { return this.m_Strategy; }
            set { this.m_Strategy = value; }
        }

        /// <summary>
        /// If true, the owner wants to retreat from combat and go back to its AttractionPoint
        /// due to too big distance and not being hit or hitting itself
        /// </summary>
        public bool WantsToRetreat
        {
            get
            {
                if (!(this.m_owner is NPC) || !((NPC) this.m_owner).CanEvade ||
                    this.m_owner.IsInRadiusSq(this.m_owner.Brain.SourcePoint, NPCMgr.DefaultMaxHomeDistanceInCombatSq))
                    return false;
                if (((NPC) this.m_owner).Entry.Rank < CreatureRank.Elite)
                    return (long) this.m_owner.MillisSinceLastCombatAction > (long) NPCMgr.GiveUpCombatDelay;
                return true;
            }
        }

        public override void Start()
        {
            this.m_init = false;
            this.m_owner.Movement.MoveType = AIMoveType.Run;
        }

        public override void Update()
        {
            if (!this.m_owner.CanDoHarm)
                return;
            NPC owner = (NPC) this.m_owner;
            if (this.WantsToRetreat)
                this.m_owner.Brain.State = BrainState.Evade;
            else if (owner.Target == null || !this.m_owner.CanBeAggroedBy(owner.Target) ||
                     owner.CheckTicks(AICombatAction.ReevaluateThreatTicks))
            {
                Unit currentAggressor;
                while ((currentAggressor = owner.ThreatCollection.CurrentAggressor) != null)
                {
                    if (!this.m_owner.CanBeAggroedBy(currentAggressor))
                        owner.ThreatCollection.Remove(currentAggressor);
                    else if (this.m_Strategy == null)
                    {
                        AICombatAction.Log.Error("Executing " + this.GetType().Name +
                                                 " without having a Strategy set.");
                    }
                    else
                    {
                        if (owner.Target != currentAggressor || !this.m_init)
                        {
                            Unit target = owner.Target;
                            owner.Target = currentAggressor;
                            this.StartEngagingCurrentTarget(target);
                            return;
                        }

                        this.m_Strategy.Update();
                        return;
                    }
                }

                if (owner.CanEvade)
                {
                    if ((long) owner.MillisSinceLastCombatAction <= (long) NPCMgr.CombatEvadeDelay ||
                        this.m_owner.Brain.CheckCombat())
                        return;
                    owner.Brain.ClearCombat(BrainState.Evade);
                }
                else
                {
                    if (owner.Brain.CheckCombat())
                        return;
                    owner.Brain.ClearCombat(owner.Brain.DefaultState);
                }
            }
            else if (!this.m_init)
                this.StartEngagingCurrentTarget((Unit) null);
            else
                this.m_Strategy.Update();
        }

        public override void Stop()
        {
            if (this.m_Strategy != null)
                this.m_Strategy.Stop();
            if (this.m_init && this.m_owner.Target != null)
                this.Disengage(this.m_owner.Target);
            this.m_owner.Target = (Unit) null;
            this.m_owner.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
        }

        /// <summary>Start attacking a new target</summary>
        private void StartEngagingCurrentTarget(Unit oldTarget)
        {
            if (this.m_init)
            {
                if (oldTarget != null)
                    this.Disengage(oldTarget);
            }
            else
                this.m_init = true;

            this.m_owner.IsFighting = true;
            ++this.m_owner.Target.NPCAttackerCount;
            this.m_Strategy.Start();
        }

        /// <summary>Stop attacking the old guy</summary>
        private void Disengage(Unit oldTarget)
        {
            --oldTarget.NPCAttackerCount;
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.HighPriority; }
        }
    }
}