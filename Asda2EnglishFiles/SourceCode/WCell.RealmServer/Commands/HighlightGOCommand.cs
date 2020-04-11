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
            this.Init("HighlightGOs", "HLGOs");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Highlights all GOs around yourself";
            this.Enabled = true;
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Dictionary<DynamicObject, GameObject> dictionary;
            bool flag = HighlightGOCommand.Highlighters.TryGetValue(trigger.Args.Character, out dictionary);
            if (!trigger.Text.NextBool() && (trigger.Text.HasNext || flag))
            {
                if (flag)
                {
                    foreach (WorldObject key in dictionary.Keys)
                        key.Delete();
                    dictionary.Clear();
                    HighlightGOCommand.Highlighters.Remove(trigger.Args.Character);
                }

                trigger.Reply("GO Highlighters OFF");
            }
            else
            {
                if (!flag)
                {
                    HighlightGOCommand.Highlighters.Add(trigger.Args.Character,
                        dictionary = new Dictionary<DynamicObject, GameObject>());
                }
                else
                {
                    foreach (WorldObject key in dictionary.Keys)
                        key.Delete();
                    dictionary.Clear();
                }

                Character character = trigger.Args.Character;
                foreach (GameObject objectsInRadiu in (IEnumerable<WorldObject>)
                    character.GetObjectsInRadius<Character>(50f, ObjectTypes.GameObject, false, 0))
                {
                    Map map = objectsInRadiu.Map;
                    Vector3 position = objectsInRadiu.Position;
                    position.Z += 7f * objectsInRadiu.ScaleX;
                    DynamicObject key = new DynamicObject((Unit) character, SpellId.ABOUTTOSPAWN, 5f, map, position);
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