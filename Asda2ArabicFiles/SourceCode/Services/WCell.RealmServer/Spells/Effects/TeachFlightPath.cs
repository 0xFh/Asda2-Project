using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class TeachFlightPathEffectHandler : SpellEffectHandler
	{
		public TeachFlightPathEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
			var chr = target as Character;
			chr.TaxiNodes.Activate((uint)Effect.MiscValue);
			TaxiHandler.SendTaxiPathActivated(chr.Client);
			TaxiHandler.SendTaxiPathUpdate(chr.Client, Cast.CasterUnit.EntityId, true);
		}
	}
}
