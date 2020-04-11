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
      get { return CallsNum * Value; }
    }

    public PereodicAction(Character chr, int value, int callsNum, int delay, Asda2PereodicActionType type)
    {
      Chr = chr;
      Value = value;
      CallsNum = callsNum;
      Delay = delay;
      Type = type;
    }

    public void Update(int dt)
    {
      CurrentDelay -= dt;
      if(CurrentDelay > 0)
        return;
      int num = 1 + (int) (-CurrentDelay / (double) Delay);
      CurrentDelay += num * Delay;
      if(num > CallsNum)
        num = CallsNum;
      for(int index = 0; index < num; ++index)
        Process();
      CallsNum -= num;
    }

    private void Process()
    {
      switch(Type)
      {
        case Asda2PereodicActionType.HpRegen:
          Chr.Heal(Value, null, null);
          break;
        case Asda2PereodicActionType.MpRegen:
          Chr.Power += Value;
          break;
        case Asda2PereodicActionType.HpRegenPrc:
          Chr.HealPercent(Value, null, null);
          break;
      }
    }
  }
}