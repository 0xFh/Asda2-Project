namespace WCell.RealmServer.Spells
{
    public interface ISpellCategoryCooldown : ICooldown
    {
        uint SpellId { get; set; }

        uint CategoryId { get; set; }

        uint ItemId { get; set; }
    }
}