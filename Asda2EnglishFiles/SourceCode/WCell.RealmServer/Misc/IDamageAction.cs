using WCell.Constants;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
    public interface IDamageAction : IUnitAction
    {
        SpellEffect SpellEffect { get; }

        int ActualDamage { get; }

        int Damage { get; set; }

        bool IsDot { get; }

        DamageSchool UsedSchool { get; }

        IAsda2Weapon Weapon { get; }
    }
}