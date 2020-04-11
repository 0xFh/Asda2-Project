using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Titles
{
    public class Asda2TitlesMgr
    {
        //collection of all character title points
        private static Dictionary<string, CharacterTitleRating> _ratings = new Dictionary<string, CharacterTitleRating>();

        public static void OnCharacterTitlePointsUpdate(Character chr)
        {
            lock (typeof(Asda2TitlesMgr))
            {
                var changed = false;
                if (!_ratings.ContainsKey(chr.Name))
                {
                    _ratings.Add(chr.Name, new CharacterTitleRating
                    {
                        CharacterName = chr.Name,
                        TotalPoints = chr.Asda2TitlePoints
                    });
                    changed = true;
                }
                else
                {
                    if (chr.Asda2TitlePoints != _ratings[chr.Name].TotalPoints)
                    {
                        _ratings[chr.Name].TotalPoints = chr.Asda2TitlePoints;
                        changed = true;
                    }
                }
                if(changed)
                    UpdateTitleRating();
            }
        }

        //update thread
        private static void UpdateTitleRating()
        {
            var ratings = _ratings.Values.ToList().OrderByDescending(r => r.TotalPoints).ToList();
            var place = 1;
            foreach (var characterTitleRating in ratings)
            {
                var chr = World.GetCharacter(characterTitleRating.CharacterName, true);
                if (chr != null && chr.PlaceInRating != place)
                {
                    chr.PlaceInRating = place;
                    chr.SendInfoMsg(string.Format("أنت الان رقم {0} في ترتيب الألقاب.", place)); //رسائل الالقاب
                    if (place <= 3)
                    {
                        World.BroadcastMsg("نظام الألقاب", string.Format("{0} أصبح الان رقم {2} في نظام الألقاب ب  {1} نقطة لقـب.",chr.Name,chr.Asda2TitlePoints,place), Color.Gold);
                    }
                    GlobalHandler.BroadcastCharacterPlaceInTitleRatingResponse(chr);
                }
                place++;
            }
        }
    }

    internal class CharacterTitleRating
    {
        public string CharacterName { get; set; }
        public int TotalPoints { get; set; }
    }
}
