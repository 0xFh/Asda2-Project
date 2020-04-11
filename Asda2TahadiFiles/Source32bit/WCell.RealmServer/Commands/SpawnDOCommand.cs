using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SpawnDOCommand : RealmServerCommand
  {
    protected SpawnDOCommand()
    {
    }

    protected override void Initialize()
    {
      Init("SpawnDO");
      EnglishParamInfo = "<spellid> <radius> [<scale>]";
      EnglishDescription = "Spawns a new DynamicObjects with the given parameters";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      SpellId spellId = trigger.Text.NextEnum(SpellId.None);
      float radius = trigger.Text.NextFloat(5f);
      float num = trigger.Text.NextFloat(1f);
      DynamicObject dynamicObject1 = new DynamicObject(trigger.Args.Character, spellId, radius,
        trigger.Args.Target.Map, trigger.Args.Target.Position);
      dynamicObject1.ScaleX = num;
      DynamicObject dynamicObject2 = dynamicObject1;
      SpellHandler.StaticDOs.Add(dynamicObject2.EntityId, dynamicObject2);
      trigger.Reply("DynamicObject created.");
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.All; }
    }
  }
}