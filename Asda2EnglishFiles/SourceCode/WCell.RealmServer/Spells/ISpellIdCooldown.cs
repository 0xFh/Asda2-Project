namespace WCell.RealmServer.Spells
{
    public interface ISpellIdCooldown : ICooldown
    {
        uint SpellId { get; set; }

        uint ItemId { get; set; }
    }
}