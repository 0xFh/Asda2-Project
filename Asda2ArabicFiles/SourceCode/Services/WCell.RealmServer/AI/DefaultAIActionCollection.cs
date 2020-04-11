using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

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
		    this[BrainState.Idle] = new AIIdleAction(owner);
			this[BrainState.Dead] = this[BrainState.Idle];

			this[BrainState.Evade] = new AIEvadeAction(owner);
			this[BrainState.Roam] = new AIRoamAction(owner)
			{
				Strategy = new AIWaypointMoveAction(owner, AIMovementType.ForwardThenBack, owner.Waypoints)
			};
            this[BrainState.PatrolCircle] = new AIRoamAction(owner)
            {
                Strategy = new AIPatrolMoveAction(owner,owner.Position)
            };
            this[BrainState.GmMove] = new AIRoamAction(owner)
            {
                Strategy = new AIGMMoveMoveAction(owner, owner.Position)
            };
		    if (owner is NPC)
		    {
                this[BrainState.Fear] = new AIRoamAction((NPC)owner)
                {
                    Strategy = new FearMoveMoveAction(owner,new Util.Graphics.Vector3(1,1))
                };
                this[BrainState.DefenceTownEventMove] = new AIRoamAction((NPC)owner)
		            {
                        Strategy = new TownDefenceEventAction(owner)
		            };
		    }
		    this[BrainState.Follow] = new AIFollowMasterAction(owner);
			this[BrainState.Guard] = new AIGuardMasterAction(owner);

			if (owner is NPC)
			{
				this[BrainState.Combat] = new AICombatAction((NPC)owner)
				{
					Strategy = new AIAttackAction((NPC)owner)
				};

				this[BrainState.FormationMove] = new AIFormationMoveAction((NPC)owner);
			}
		}
	}
}