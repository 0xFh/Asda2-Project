using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	public class PlayMusicEffectHandler : SpellEffectHandler
	{
		public PlayMusicEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
			MiscHandler.SendPlayMusic(target, (uint)Effect.MiscValue, Effect.Radius);
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
