using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ListPlayersCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("ListPlayers", "Players");
            this.EnglishParamInfo = "[-[mfcna] [<Map>]|[<Faction>]|[<Class>]|[<namepart>]|[<accountnamepart>]]";
            this.EnglishDescription =
                "Lists all currently logged in Players, or only those that match the given filter(s) - Example: ListPlayers -m kalimdor";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (WCell.RealmServer.Global.World.CharacterCount == 0)
            {
                trigger.Reply("There are no characters online.");
            }
            else
            {
                string str1 = trigger.Text.NextModifiers();
                List<Character> characterList;
                if (str1.Contains("r"))
                {
                    Map nonInstancedMap =
                        WCell.RealmServer.Global.World.GetNonInstancedMap(trigger.Text.NextEnum<MapId>(MapId.End));
                    if (nonInstancedMap != null)
                    {
                        characterList = new List<Character>(nonInstancedMap.CharacterCount);
                        characterList.AddRange((IEnumerable<Character>) nonInstancedMap.Characters);
                    }
                    else
                    {
                        trigger.Reply("Invalid map id.");
                        return;
                    }
                }
                else
                    characterList = WCell.RealmServer.Global.World.GetAllCharacters();

                if (str1.Contains("f"))
                {
                    FactionId factionId = trigger.Text.NextEnum<FactionId>(FactionId.End);
                    switch (factionId)
                    {
                        case FactionId.None:
                        case FactionId.End:
                            trigger.Reply("Invalid FactionID.");
                            return;
                        default:
                            for (int index = characterList.Count - 1; index >= 0; --index)
                            {
                                Character character = characterList[index];
                                if (character.FactionId != factionId)
                                    characterList.Remove(character);
                            }

                            break;
                    }
                }

                if (str1.Contains("c"))
                {
                    ClassId classId = trigger.Text.NextEnum<ClassId>(ClassId.End);
                    if (classId == ClassId.End)
                    {
                        trigger.Reply("Invalid class.");
                        return;
                    }

                    for (int index = characterList.Count - 1; index >= 0; --index)
                    {
                        Character character = characterList[index];
                        if (character.Class != classId)
                            characterList.Remove(character);
                    }
                }

                if (str1.Contains("n"))
                {
                    string str2 = trigger.Text.NextWord();
                    if (str2.Length > 1)
                    {
                        for (int index = characterList.Count - 1; index >= 0; --index)
                        {
                            Character character = characterList[index];
                            if (!character.Name.Contains(str2))
                                characterList.Remove(character);
                        }
                    }
                    else
                    {
                        for (int index = characterList.Count - 1; index >= 0; --index)
                        {
                            Character character = characterList[index];
                            if (!character.Name.StartsWith(str2))
                                characterList.Remove(character);
                        }
                    }
                }

                if (str1.Contains("a"))
                {
                    string str2 = trigger.Text.NextWord();
                    if (str2.Length > 1)
                    {
                        for (int index = characterList.Count - 1; index >= 0; --index)
                        {
                            Character character = characterList[index];
                            if (!character.Account.Name.Contains(str2))
                                characterList.Remove(character);
                        }
                    }
                    else
                    {
                        for (int index = characterList.Count - 1; index >= 0; --index)
                        {
                            Character character = characterList[index];
                            if (!character.Name.StartsWith(str2))
                                characterList.Remove(character);
                        }
                    }
                }

                if (characterList.Count == WCell.RealmServer.Global.World.CharacterCount)
                    trigger.Reply("All Online Players:");
                else if (characterList.Count == 0)
                    trigger.Reply("No players match the given conditions.");
                else
                    trigger.Reply("Matching Players:");
                foreach (Character character in characterList)
                    trigger.Reply(character.ToString());
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }
    }
}