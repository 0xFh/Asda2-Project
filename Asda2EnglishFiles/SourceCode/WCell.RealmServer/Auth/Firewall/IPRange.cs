namespace WCell.RealmServer.Auth.Firewall
{
    public struct IPRange
    {
        public long Min;
        public long Max;

        public IPRange(long? min, long? max)
        {
            this.Min = min.HasValue ? min.Value : max.Value;
            if (max.HasValue)
                this.Max = max.Value;
            else
                this.Max = min.Value;
        }
    }
}