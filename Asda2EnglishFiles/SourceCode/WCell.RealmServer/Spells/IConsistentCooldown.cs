namespace WCell.RealmServer.Spells
{
    public interface IConsistentCooldown : ICooldown
    {
        uint CharId { get; set; }

        void Save();

        void Update();

        void Create();

        void Delete();
    }
}