using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Brains
{
    /// <summary>The default class for monsters' AI</summary>
    public class BaseBrain : IBrain, IUpdatable, IAICombatEventHandler, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static BrainState DefaultBrainState = BrainState.Roam;
        protected bool IsFirstDamageReceived = true;

        /// <summary>Current state</summary>
        protected BrainState m_state;

        /// <summary>Default state</summary>
        protected BrainState m_defaultState;

        protected Unit m_owner;

        /// <summary>Actions to be executed in the idle state</summary>
        protected Vector3 m_SourcePoint;

        protected IAIActionCollection m_actions;
        protected IAIAction m_currentAction;
        protected bool m_IsAggressive;
        protected bool m_IsRunning;
        private int _updatesCountFrorScanAndAttack;

        public BaseBrain(Unit owner)
            : this(owner, (IAIActionCollection) new DefaultAIActionCollection(), BaseBrain.DefaultBrainState)
        {
        }

        public BaseBrain(Unit owner, BrainState defaultState)
            : this(owner, (IAIActionCollection) new DefaultAIActionCollection(), defaultState)
        {
        }

        public BaseBrain(Unit owner, IAIActionCollection actions)
            : this(owner, actions, BrainState.Idle)
        {
        }

        public BaseBrain(Unit owner, IAIActionCollection actions, BrainState defaultState)
        {
            this.m_owner = owner;
            this.m_defaultState = defaultState;
            this.m_state = this.m_defaultState;
            this.m_actions = actions;
            this.m_IsAggressive = true;
        }

        public Unit Owner
        {
            get { return this.m_owner; }
        }

        /// <summary>
        /// Owner as NPC.
        /// Returns null if Owner is not an NPC
        /// </summary>
        public NPC NPC
        {
            get { return this.m_owner as NPC; }
        }

        public IAIAction CurrentAction
        {
            get { return this.m_currentAction; }
            set
            {
                if (this.m_currentAction != null)
                    this.m_currentAction.Stop();
                this.m_currentAction = value;
                if (value == null)
                    return;
                if (this.m_currentAction.IsGroupAction &&
                    (!(this.m_owner is NPC) || ((NPC) this.m_owner).Group == null))
                {
                    BaseBrain.log.Error("{0} tried to execute {1} but is not in Group.", (object) this.m_owner,
                        (object) this.m_currentAction);
                    this.m_currentAction = (IAIAction) null;
                }
                else
                    this.m_currentAction.Start();
            }
        }

        public BrainState State
        {
            get { return this.m_state; }
            set
            {
                if (this.m_state == value && this.m_currentAction != null)
                    return;
                if (!this.m_owner.IsInWorld)
                {
                    this.m_state = value;
                }
                else
                {
                    AIAction action = this.m_actions[value];
                    if (action == null)
                    {
                        if (this.m_state == this.m_defaultState)
                            return;
                        this.m_state = this.m_defaultState;
                        this.State = this.m_defaultState;
                    }
                    else
                    {
                        this.m_state = value;
                        if (this.m_currentAction != null)
                            this.m_currentAction.Stop();
                        this.CurrentAction = (IAIAction) action;
                    }
                }
            }
        }

        /// <summary>The State to fall back to when nothing else is up.</summary>
        public BrainState DefaultState
        {
            get { return this.m_defaultState; }
            set
            {
                bool flag = this.m_defaultState == this.m_state;
                this.m_defaultState = value;
                if (!flag)
                    return;
                this.State = value;
            }
        }

        public UpdatePriority UpdatePriority
        {
            get
            {
                if (!this.m_IsRunning || this.m_currentAction == null)
                    return UpdatePriority.Background;
                return this.m_currentAction.Priority;
            }
        }

        public bool IsRunning
        {
            get { return this.m_IsRunning; }
            set
            {
                if (this.m_IsRunning == value)
                    return;
                if (value)
                    this.Start();
                else
                    this.Stop();
            }
        }

        /// <summary>Collection of all actions that this brain can execute</summary>
        public IAIActionCollection Actions
        {
            get { return this.m_actions; }
        }

        /// <summary>
        /// The point of attraction where we took off when we started with the
        /// last action
        /// </summary>
        public Vector3 SourcePoint
        {
            get { return this.m_SourcePoint; }
            set { this.m_SourcePoint = value; }
        }

        public List<Vector3> MovingPoints { get; set; }

        public bool IsAggressive
        {
            get { return this.m_IsAggressive; }
            set { this.m_IsAggressive = value; }
        }

        /// <summary>
        /// Returns the default AIAction for the Combat BrainState (to be executed when using State = BrainState.Combat).
        /// Returns null if that Action is not an AICombatAction - In that case use Actions[BrainState.Combat] instead.
        /// </summary>
        public AICombatAction DefaultCombatAction
        {
            get { return this.m_actions[BrainState.Combat] as AICombatAction; }
        }

        /// <summary>
        /// Updates the AIAction by calling Perform. Called every tick by the Map
        /// </summary>
        /// <param name="dt">not used</param>
        public virtual void Update(int dt)
        {
            if (!this.m_IsRunning)
                return;
            this.Perform();
        }

        public void EnterDefaultState()
        {
            this.IsFirstDamageReceived = true;
            this.State = this.m_defaultState;
        }

        protected virtual void Start()
        {
            this.m_IsRunning = true;
        }

        protected virtual void Stop()
        {
            this.m_IsRunning = false;
            this.StopCurrentAction();
        }

        public void StopCurrentAction()
        {
            if (this.m_currentAction != null)
                this.m_currentAction.Stop();
            this.m_currentAction = (IAIAction) null;
        }

        /// <summary>Performs a full Brain cycle</summary>
        public void Perform()
        {
            if (this.m_owner.IsUsingSpell || this.m_owner.CastingTill > DateTime.Now)
                return;
            if (this.m_currentAction == null)
            {
                this.m_currentAction = (IAIAction) this.m_actions[this.m_state];
                if (this.m_currentAction == null)
                    this.State = this.m_defaultState;
                else
                    this.m_currentAction.Start();
            }
            else
                this.m_currentAction.Update();
        }

        public virtual void OnEnterCombat()
        {
        }

        public virtual void OnLeaveCombat()
        {
        }

        public virtual void OnHeal(Unit healer, Unit healed, int amtHealed)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public virtual void OnDamageReceived(IDamageAction action)
        {
        }

        public virtual void OnDamageDealt(IDamageAction action)
        {
        }

        public virtual void OnDebuff(Unit caster, SpellCast cast, Aura debuff)
        {
        }

        public virtual void OnKilled(Unit killerUnit, Unit victimUnit)
        {
        }

        public virtual void OnDeath()
        {
            this.State = BrainState.Dead;
        }

        /// <summary>Called when entering the World and when resurrected</summary>
        public virtual void OnActivate()
        {
            if (!this.m_actions.IsInitialized)
                this.m_actions.Init(this.m_owner);
            this.m_SourcePoint = this.m_owner.Position;
            this.CurrentAction = (IAIAction) this.m_actions[this.m_state];
        }

        public virtual void OnCombatTargetOutOfRange()
        {
        }

        public virtual bool CheckCombat()
        {
            if (!(this.m_owner is NPC) || !this.m_owner.CanDoHarm || this.m_owner.IsInfluenced ||
                !this.m_owner.IsAreaActive && !this.m_owner.Map.ScanInactiveAreas)
                return false;
            NPC owner = (NPC) this.m_owner;
            if ((owner.ThreatCollection.CurrentAggressor == null ||
                 !owner.CanReachForCombat(owner.ThreatCollection.CurrentAggressor)) &&
                (!this.m_IsAggressive || !this.TryScanAndAttack()))
                return false;
            this.State = BrainState.Combat;
            return true;
        }

        /// <summary>scan only every 50 update</summary>
        /// <returns></returns>
        private bool TryScanAndAttack()
        {
            if (this._updatesCountFrorScanAndAttack >= CharacterFormulas.NpcUpdatesToScanAndAttack)
            {
                this._updatesCountFrorScanAndAttack = 0;
                return this.ScanAndAttack();
            }

            ++this._updatesCountFrorScanAndAttack;
            return false;
        }

        public void ClearCombat(BrainState newState)
        {
            NPC owner = this.m_owner as NPC;
            if (owner != null)
            {
                owner.ThreatCollection.Clear();
                owner.Damages.Clear();
            }

            this.m_owner.IsInCombat = false;
            this.m_owner.MarkUpdate((UpdateFieldId) UnitFields.DYNAMIC_FLAGS);
            this.State = newState;
        }

        public void OnGroupChange(AIGroup newGroup)
        {
        }

        /// <summary>
        /// Returns whether it found enemies and started attacking or false if none found.
        /// </summary>
        /// <returns></returns>
        public virtual bool ScanAndAttack()
        {
            if (!(this.m_owner is NPC))
                return false;
            NPC owner = (NPC) this.m_owner;
            return !owner.IterateEnvironment(NPCEntry.AggroRangeMaxDefault, (Func<WorldObject, bool>) (obj =>
            {
                if (!(obj is Unit))
                    return true;
                Unit unit = (Unit) obj;
                if (unit is Character && ((Character) unit).GodMode ||
                    (!unit.CanGenerateThreat || !this.m_owner.IsHostileWith((IFactionMember) unit)) ||
                    (!this.m_owner.CanSee((WorldObject) unit) ||
                     !unit.IsInRadiusSq((IHasPosition) owner, owner.GetAggroRangeSq(unit))) || unit is NPC &&
                    ((NPC) unit).ThreatCollection.CurrentAggressor == null &&
                    !unit.IsHostileWith((IFactionMember) owner))
                    return true;
                owner.ThreatCollection.AddNewIfNotExisted(unit);
                return !owner.CanReachForCombat(unit);
            }));
        }

        public void Dispose()
        {
            this.m_owner = (Unit) null;
        }
    }
}