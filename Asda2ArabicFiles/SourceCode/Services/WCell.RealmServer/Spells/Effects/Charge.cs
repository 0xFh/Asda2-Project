using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class ChargeEffectHandler : SpellEffectHandler
	{
		public ChargeEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason Initialize()
		{
			

			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
		    if (Effect.Spell.Id == 207 || Effect.Spell.Id == 97)
		    {
		        var distance = m_cast.CasterObject.Position.GetDistance(target.Position) - ((Unit) target).BoundingRadius -
		                       0.7f;
		        var direction = target.Position - m_cast.CasterObject.Position;
		        direction.Normalize();
		        Cast.CasterChar.Position = Cast.CasterChar.Position + direction*distance;
                if (target is NPC || Effect.Spell.Id == 207)
		            Asda2MovmentHandler.SendStartMoveCommonToAreaResponse(Cast.CasterChar, true);
		    }
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}