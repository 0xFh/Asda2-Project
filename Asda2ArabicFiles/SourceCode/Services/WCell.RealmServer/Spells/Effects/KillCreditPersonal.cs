using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class KillCreditPersonal : SpellEffectHandler
	{
		public KillCreditPersonal(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
			var chr = target as Character;
			if (m_cast.CasterUnit != null && m_cast.CasterUnit is NPC)
			{
				chr.QuestLog.OnNPCInteraction((NPC)m_cast.CasterUnit);
			}
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}
