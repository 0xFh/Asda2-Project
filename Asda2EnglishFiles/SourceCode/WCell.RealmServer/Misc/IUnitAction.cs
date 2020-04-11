using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
    /// <summary>Any kind of Action a Unit can perform</summary>
    public interface IUnitAction
    {
        /// <summary>The Attacker or Caster</summary>
        Unit Attacker { get; }

        /// <summary>Victim or Target or Receiver</summary>
        Unit Victim { get; }

        /// <summary>
        /// Whether this was a critical action (might be meaningless for some actions)
        /// </summary>
        bool IsCritical { get; }

        Spell Spell { get; }

        /// <summary>Reference count is used to support pooling</summary>
        int ReferenceCount { get; set; }
    }
}