using WCell.Constants.NPCs;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI
{
    public class DefaultAIActionCollection : AIActionCollection
    {
        /// <summary>
        /// Method is called before the Unit is actually in the World
        /// to initialize the set of Actions.
        /// </summary>
        /// <param name="owner"></param>
        public override void Init(Unit owner)
        {
            base.Init(owner);
            this[BrainState.Idle] = (AIAction) new AIIdleAction(owner);
            this[BrainState.Dead] = this[BrainState.Idle];
            this[BrainState.Evade] = (AIAction) new AIEvadeAction(owner);
            this[BrainState.Roam] = (AIAction) new AIRoamAction(owner)
            {
                Strategy = (AIAction) new AIWaypointMoveAction(owner, AIMovementType.ForwardThenBack, owner.Waypoints)
            };
            this[BrainState.PatrolCircle] = (AIAction) new AIRoamAction(owner)
            {
                Strategy = (AIAction) new AIPatrolMoveAction(owner, owner.Position)
            };
            this[BrainState.GmMove] = (AIAction) new AIRoamAction(owner)
            {
                Strategy = (AIAction) new AIGMMoveMoveAction(owner, owner.Position)
            };
            if (owner is NPC)
            {
                this[BrainState.Fear] = (AIAction) new AIRoamAction(owner)
                {
                    Strategy = (AIAction) new FearMoveMoveAction(owner, new Vector3(1f, 1f))
                };
                this[BrainState.DefenceTownEventMove] = (AIAction) new AIRoamAction(owner)
                {
                    Strategy = (AIAction) new TownDefenceEventAction(owner)
                };
            }

            this[BrainState.Follow] = (AIAction) new AIFollowMasterAction(owner);
            this[BrainState.Guard] = (AIAction) new AIGuardMasterAction(owner);
            if (!(owner is NPC))
                return;
            this[BrainState.Combat] = (AIAction) new AICombatAction((NPC) owner)
            {
                Strategy = (AIAction) new AIAttackAction((NPC) owner)
            };
            this[BrainState.FormationMove] = (AIAction) new AIFormationMoveAction((NPC) owner);
        }
    }
}