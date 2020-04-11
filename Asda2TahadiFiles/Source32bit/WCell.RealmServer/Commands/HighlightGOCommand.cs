using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
  public class HighlightGOCommand : RealmServerCommand
  {
    public static readonly SynchronizedDictionary<Character, Dictionary<DynamicObject, GameObject>> Highlighters =
      new SynchronizedDictionary<Character, Dictionary<DynamicObject, GameObject>>();

    protected HighlightGOCommand()
    {
    }

    protected override void Initialize()
    {
      Init("HighlightGOs", "HLGOs");
      EnglishParamInfo = "[0/1]";
      EnglishDescription = "Highlights all GOs around yourself";
      Enabled = true;
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Dictionary<DynamicObject, GameObject> dictionary;
      bool flag = Highlighters.TryGetValue(trigger.Args.Character, out dictionary);
      if(!trigger.Text.NextBool() && (trigger.Text.HasNext || flag))
      {
        if(flag)
        {
          foreach(WorldObject key in dictionary.Keys)
            key.Delete();
          dictionary.Clear();
          Highlighters.Remove(trigger.Args.Character);
        }

        trigger.Reply("GO Highlighters OFF");
      }
      else
      {
        if(!flag)
        {
          Highlighters.Add(trigger.Args.Character,
            dictionary = new Dictionary<DynamicObject, GameObject>());
        }
        else
        {
          foreach(WorldObject key in dictionary.Keys)
            key.Delete();
          dictionary.Clear();
        }

        Character character = trigger.Args.Character;
        foreach(GameObject objectsInRadiu in character.GetObjectsInRadius(50f, ObjectTypes.GameObject, false, 0))
        {
          Map map = objectsInRadiu.Map;
          Vector3 position = objectsInRadiu.Position;
          position.Z += 7f * objectsInRadiu.ScaleX;
          DynamicObject key = new DynamicObject(character, SpellId.ABOUTTOSPAWN, 5f, map, position);
          dictionary.Add(key, objectsInRadiu);
        }

        trigger.Reply("Highlighting {0} GameObjects", (object) dictionary.Count);
      }
    }

    public override bool RequiresCharacter
    {
      get { return true; }
    }
  }
}