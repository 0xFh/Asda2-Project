using System;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
	/// <summary>
	/// AI movemement action for roaming
	/// </summary>
	public class AIRoamAction : AIAction, IAIStateAction
	{

		public AIRoamAction(Unit owner)
			: base(owner)
		{
		}

		public AIRoamAction(Unit owner, AIAction roamAction) :
			base(owner)
		{
			Strategy = roamAction;
		}

		public int MinimumRoamSpellCastDelay
		{
			get;
			set;
		}

		/// <summary>
		/// The strategy to be used while roaming
		/// </summary>
		public AIAction Strategy { get; set; }

		public override void Start()
		{
			m_owner.FirstAttacker = null;
			m_owner.Target = null;
		    var npc = Owner as NPC;
            if (npc != null && !npc.IsInCombat)
            {
                npc.ThreatCollection.Clear();
                npc.Damages.Clear();
            }
			Strategy.Start();
		}

		public override void Update()
		{
			if (!m_owner.Brain.CheckCombat())
			{
				Strategy.Update();
			}
		}

		public override void Stop()
		{
			Strategy.Stop();
		}
        
		public override UpdatePriority Priority
		{
			get { return UpdatePriority.VeryLowPriority; }
		}
	}
}