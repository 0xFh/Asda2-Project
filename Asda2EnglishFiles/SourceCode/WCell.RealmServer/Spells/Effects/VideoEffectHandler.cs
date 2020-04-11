using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// Video Effect.
    /// Spells with this effect might be used to trigger client-side video sequences!
    /// 
    /// eg.:
    /// 
    /// every Flight Path
    /// Filming (Id: 28129)
    /// Stormcrow Amulet (Id: 31606)
    /// Elekk Taxi (Id: 31788)
    /// Attack Run 1 (Id: 32059) - Attack Run 4
    /// Nethrandamus Flight (Id: 32551)
    /// Gateways Murket and Shaadraz
    /// Aerial Assault Flight (Horde)
    /// Aerial Assault Flight (Heavy Bomb)
    /// ....
    /// 
    /// </summary>
    public class VideoEffectHandler : SpellEffectHandler
    {
        public VideoEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}