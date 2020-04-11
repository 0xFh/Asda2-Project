using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class ScriptEffectHandler : SpellEffectHandler
	{
		public ScriptEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
			
		}
	}
}
