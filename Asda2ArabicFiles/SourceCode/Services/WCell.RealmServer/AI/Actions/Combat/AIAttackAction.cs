using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Combat
{
	/// <summary>
	/// Attack with the main weapon
	/// </summary>
	public class AIAttackAction : AITargetMoveAction
	{
		protected float minDist, maxDist, desiredDist;

		public AIAttackAction(NPC owner)
			: base(owner)
		{
            minDist = 0;// owner.BoundingRadius;
		}

		public override float DistanceMin
		{
			get { return minDist; }
		}

		public override float DistanceMax
		{
			get { return maxDist; }
		}

		public override float DesiredDistance
		{
			get { return desiredDist; }
		}

		/// <summary>
		/// Called when starting to attack a new Target
		/// </summary>
		public override void Start()
		{
			m_owner.IsFighting = true;
			if (UsesSpells)
			{
				((NPC)m_owner).NPCSpells.ShuffleReadySpells();
			}

			m_target = m_owner.Target;
			if (m_target != null)
			{
				maxDist = m_owner.GetBaseAttackRange(m_target) - 1;
				if (maxDist < 0.5f)
				{
					maxDist = 0.5f;
				}
				desiredDist = maxDist / 2;
			}
			if (m_owner.CanMelee)
			{
				base.Start();
			}
		}

		/// <summary>
		/// Called during every Brain tick
		/// </summary>
		public override void Update()
		{
		    var targetChr = Target as Character;

			// Check for spells that we can cast
            if (targetChr!=null&&UsesSpells && HasSpellReady && m_owner.CanCastSpells && m_owner.CastingTill < DateTime.Now)
			{
				if (TryCastSpell())
				{
					m_owner.Movement.Stop();
					return;
				}
			}

			// Move in on the target
            if (m_owner.CanMelee && m_owner.CastingTill < DateTime.Now)
			{
				base.Update();
			}
		}

		/// <summary>
		/// Called when we stop attacking a Target
		/// </summary>
		public override void Stop()
		{
			m_owner.IsFighting = false;
			base.Stop();
		}

		/// <summary>
		/// Tries to cast a Spell that is ready and allowed in the current context.
		/// </summary>
		/// <returns></returns>
		protected bool TryCastSpell()
        {
            if (Target == null)
                return false;
			var owner = (NPC)m_owner;
			foreach (var spell in owner.NPCSpells.ReadySpells)
			{
				var err = spell.CheckCasterConstraints(owner);
				if (err == SpellFailedReason.Ok && owner.CastingTill<DateTime.Now)
                {
                    owner.CastingTill = DateTime.Now.AddMilliseconds(spell.CastDelay);
					return m_owner.SpellCast.Start(spell,Target) == SpellFailedReason.Ok;
				}
			}
			return false;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Active; }
		}
	}
}