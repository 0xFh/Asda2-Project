using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>
    /// Adds a custom ProcHandler to the Owner while its active
    /// </summary>
    public class AddProcHandler : AuraEffectHandler
    {
        public AddProcHandler(IProcHandler handler)
        {
            this.ProcHandler = handler;
        }

        public IProcHandler ProcHandler { get; set; }

        protected override void Apply()
        {
            this.Owner.AddProcHandler(this.ProcHandler);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.RemoveProcHandler(this.ProcHandler);
        }
    }
}