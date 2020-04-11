namespace WCell.RealmServer.Misc
{
    public class HealAction : SimpleUnitAction
    {
        public int Value { get; set; }

        /// <summary>Heal over time</summary>
        public bool IsHot { get; set; }
    }
}