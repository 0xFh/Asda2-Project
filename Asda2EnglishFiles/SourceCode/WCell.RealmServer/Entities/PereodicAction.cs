using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Entities
{
    public class PereodicAction
    {
        public Character Chr { get; set; }

        public int Value { get; set; }

        public int CallsNum { get; set; }

        public int Delay { get; set; }

        public int CurrentDelay { get; set; }

        public Asda2PereodicActionType Type { get; set; }

        public int RemainingHeal
        {
            get { return this.CallsNum * this.Value; }
        }

        public PereodicAction(Character chr, int value, int callsNum, int delay, Asda2PereodicActionType type)
        {
            this.Chr = chr;
            this.Value = value;
            this.CallsNum = callsNum;
            this.Delay = delay;
            this.Type = type;
        }

        public void Update(int dt)
        {
            this.CurrentDelay -= dt;
            if (this.CurrentDelay > 0)
                return;
            int num = 1 + (int) ((double) -this.CurrentDelay / (double) this.Delay);
            this.CurrentDelay += num * this.Delay;
            if (num > this.CallsNum)
                num = this.CallsNum;
            for (int index = 0; index < num; ++index)
                this.Process();
            this.CallsNum -= num;
        }

        private void Process()
        {
            switch (this.Type)
            {
                case Asda2PereodicActionType.HpRegen:
                    this.Chr.Heal(this.Value, (Unit) null, (SpellEffect) null);
                    break;
                case Asda2PereodicActionType.MpRegen:
                    this.Chr.Power += this.Value;
                    break;
                case Asda2PereodicActionType.HpRegenPrc:
                    this.Chr.HealPercent(this.Value, (Unit) null, (SpellEffect) null);
                    break;
            }
        }
    }
}