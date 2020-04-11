using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    /// <summary>
    /// Can be used to create custom GameObjects that will apply the given
    /// Spell to everyone in Radius.
    /// </summary>
    public class GOCustomAuraEntry : GOCustomEntry, ISpellParameters
    {
        [NotPersistent] public Spell Spell { get; set; }

        [NotPersistent] public int MaxCharges { get; set; }

        [NotPersistent] public int Amplitude { get; set; }

        [NotPersistent] public int StartDelay { get; set; }

        [NotPersistent] public int Radius { get; set; }
    }
}