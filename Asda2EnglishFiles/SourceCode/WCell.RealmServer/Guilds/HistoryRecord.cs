namespace WCell.RealmServer.Guilds
{
    public class HistoryRecord
    {
        public byte Type { get; set; }

        public int Value { get; set; }

        public string TrigerName { get; set; }

        public string Time { get; set; }

        public HistoryRecord(byte msgType, int value, string trigerName, string time)
        {
            this.Type = msgType;
            this.TrigerName = trigerName;
            this.Time = time;
            this.Value = value;
        }
    }
}