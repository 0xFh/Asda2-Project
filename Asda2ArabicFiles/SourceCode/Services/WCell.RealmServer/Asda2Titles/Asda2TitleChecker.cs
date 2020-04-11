using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;
using Map = NHibernate.Mapping.Map;

namespace WCell.RealmServer.Asda2Titles //«·«·ﬁ«»
{
    static class Asda2TitleCheckerHelper
    {


        public static void DiscoverTitle(this Character chr, Asda2TitleId titleId)
        {
            if (chr == null || chr.GetedTitles == null || chr.DiscoveredTitles == null)
                return;

            if (chr.DiscoveredTitles.GetBit((int)titleId))
                return;
            chr.DiscoveredTitles.SetBit((int)titleId);
            Asda2TitlesHandler.SendTitleDiscoveredResponse(chr.Client, (short)titleId);
        }

        public static void GainTitle(this Character chr, Asda2TitleId titleId)
        {
            if (chr == null || chr.GetedTitles == null || chr.DiscoveredTitles == null)
                return;

            if (chr.GetedTitles.GetBit((int)titleId))
                return;
            chr.DiscoveredTitles.UnsetBit((int)titleId);
            chr.GetedTitles.SetBit((int)titleId);
            chr.Asda2TitlePoints += Asda2TitleTemplate.Templates[(int)titleId].Points;
            Asda2TitlesHandler.SendYouGetNewTitleResponse(chr, (short)titleId);
            Asda2TitlesMgr.OnCharacterTitlePointsUpdate(chr);
            Asda2TitleChecker.OnTitleCountChanged(chr);
        }


        public static void UpdateTitleCounter(this Character chr, Asda2TitleId titleId, int discoverOn, int getOn, int increaceCounterBy = 1)
        {
            if (chr == null || chr.TitleProgress == null || chr.DiscoveredTitles == null) return;

            var counter = chr.TitleProgress.IncreaseCounter(titleId, increaceCounterBy);
            if (counter < getOn)
                chr.SendInfoMsg(string.Format("«··ﬁ» {0} [{1} „‰ {2}]", titleId, counter, getOn));//‰Ÿ«„ «·«·ﬁ«» Ê«·—”«·…
            if (counter >= discoverOn)
                chr.DiscoverTitle(titleId);
            if (counter >= getOn)
                chr.GainTitle(titleId);
        }

        public static Character CheckTitle(this Character chr, Asda2TitleId titleId, Func<bool> discoverPredicate, Func<bool> getPredicate)
        {
            if (chr == null || chr.TitleProgress == null || chr.DiscoveredTitles == null) return chr;

            if (discoverPredicate())
                chr.DiscoverTitle(titleId);
            if (getPredicate())
                chr.GainTitle(titleId);
            return chr;
        }

        public static Character CheckTitlesCollection(this Character chr, Asda2TitleId titleGained,
            params Asda2TitleId[] requaredTitles)
        {
            if (chr == null || chr.TitleProgress == null || chr.DiscoveredTitles == null) return chr;
            var allGetted = true;
            foreach (var requaredTitle in requaredTitles)
            {
                if (!chr.GetedTitles.GetBit((int)requaredTitle))
                {
                    allGetted = false;
                }
                else
                {
                    chr.DiscoverTitle(titleGained);
                }
            }

            if (allGetted)
                chr.GainTitle(titleGained);
            return chr;
        }
    }
    static class Asda2TitleChecker
    {

        public static void OnLevelChanged(Character character)
        {
            character
                .CheckTitle(Asda2TitleId.Novice0, () => character.Level >= 2, () => character.Level >= 5)
                .CheckTitle(Asda2TitleId.Amateur1, () => character.Level >= 5, () => character.Level >= 20)
                .CheckTitle(Asda2TitleId.Intermediate2, () => character.Level >= 20, () => character.Level >= 40)
                .CheckTitle(Asda2TitleId.Trained3, () => character.Level >= 40, () => character.Level >= 60)
                .CheckTitle(Asda2TitleId.Expert4, () => character.Level >= 60, () => character.Level >= 80);
        }

        public static void OnDancing(Character character)
        {
            character.UpdateTitleCounter(Asda2TitleId.Dancer41, 100, 500);
        }
        public static void OnCrazy(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Naughty, 100, 500);
        }
        public static void OnGreet(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Greeting, 100, 500);
        }
        public static void OnChlng(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Challenger, 100, 500);
        }
        public static void OnThank(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Thankfull, 100, 500);
        }
        public static void OnSad(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.TheSad, 100, 500);
        }
        public static void OnAngry(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Angry, 100, 500);
        }
        public static void OnHappy(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Happy, 100, 500);
        }
        public static void OnRomanc(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Romantic, 100, 500);
        }
        public static void OnClow(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Clown, 100, 500);
        }
        public static void OnArt(Character character)
        {

            character.UpdateTitleCounter(Asda2TitleId.Acrobatic, 100, 500);
        }
        public class EmoteChecker
        {
            static readonly Dictionary<string, DateTime> EmoteTimers = new Dictionary<string, DateTime>();

            public static void OnEmote(short emote, Character chr)
            {
                if (EmoteTimers.ContainsKey(chr.Name))
                {
                    var lastTimeEmote = EmoteTimers[chr.Name];
                    if (DateTime.Now.Subtract(lastTimeEmote).TotalMilliseconds < 2000)
                        return;
                    EmoteTimers[chr.Name] = DateTime.Now;
                }
                else
                {
                    EmoteTimers.Add(chr.Name, DateTime.Now);
                }
                if (emote == 113 || emote == 112)
                    OnDancing(chr);
                if (emote == 105)
                    OnCrazy(chr);
                if (emote == 104)
                    OnGreet(chr);
                if (emote == 106)
                    OnChlng(chr);
                if (emote == 103)
                    OnThank(chr);
                if (emote == 102)
                    OnSad(chr);
                if (emote == 101)
                    OnAngry(chr);
                if (emote == 100)
                    OnHappy(chr);
                if (emote == 115)
                    OnRomanc(chr);
                if (emote == 116)
                    OnClow(chr);
                if (emote == 117)
                    OnArt(chr);

            }
        }


        public static void OnItemUpgrade(byte enchant, Character owner, Asda2Item item)
        {
            owner.UpdateTitleCounter(Asda2TitleId.Upgradernew, 10, 100);
            owner.UpdateTitleCounter(Asda2TitleId.Advancedupgrader, 100, 300);
            owner.UpdateTitleCounter(Asda2TitleId.Bronzeupgrader, 300, 500);
            owner.UpdateTitleCounter(Asda2TitleId.Silverupgrader, 500, 1000);
            owner.UpdateTitleCounter(Asda2TitleId.Goldupgrader, 1000, 2000);
            owner.UpdateTitleCounter(Asda2TitleId.Diamondupgrader, 2000, 3000);
            owner.UpdateTitleCounter(Asda2TitleId.PlatinumUpgrader, 3000, 5000);
            owner.UpdateTitleCounter(Asda2TitleId.UltimateUpgrader, 5000, 10000);
            owner.CheckTitlesCollection(Asda2TitleId.UpgradeMaster, Asda2TitleId.Upgradernew, Asda2TitleId.Advancedupgrader,
            Asda2TitleId.Bronzeupgrader, Asda2TitleId.Silverupgrader, Asda2TitleId.Goldupgrader, Asda2TitleId.Diamondupgrader,
           Asda2TitleId.PlatinumUpgrader, Asda2TitleId.UltimateUpgrader);
            if (item.Template.Quality == Asda2ItemQuality.White)
            {
                owner.UpdateTitleCounter(Asda2TitleId.CommonUpgrader, 200, 2000);
            }
            if (item.Template.Quality == Asda2ItemQuality.Yello)
            {
                owner.UpdateTitleCounter(Asda2TitleId.UncommonUpgrader, 100, 1000);
            }
            if (item.Template.Quality == Asda2ItemQuality.Purple)
            {
                owner.UpdateTitleCounter(Asda2TitleId.RareUpgrader, 50, 500);
            }
            if (item.Template.Quality == Asda2ItemQuality.Green)
            {
                owner.UpdateTitleCounter(Asda2TitleId.HeroUpgrader, 30, 300);
            }
            if (item.Template.Quality == Asda2ItemQuality.Orange)
            {
                owner.UpdateTitleCounter(Asda2TitleId.Legendryupgrader, 20, 200);
            }
            owner.CheckTitlesCollection(Asda2TitleId.Itemupgrader, Asda2TitleId.CommonUpgrader, Asda2TitleId.UncommonUpgrader,
              Asda2TitleId.RareUpgrader, Asda2TitleId.HeroUpgrader, Asda2TitleId.Legendryupgrader);
            if (enchant >= 20)
                owner.GainTitle(Asda2TitleId.Absolute257);
            if (enchant >= 20)
                owner.UpdateTitleCounter(Asda2TitleId.awosomeupgrade, 1, 5);
            if (enchant >= 20)
                owner.UpdateTitleCounter(Asda2TitleId.upgrademper, 5, 20);

            if (enchant == 15)
                owner.UpdateTitleCounter(Asda2TitleId.Upgrader, 1, 5);

            if (enchant == 10)
                owner.UpdateTitleCounter(Asda2TitleId.Elite256, 10, 30);

            if (enchant == 5)
                owner.UpdateTitleCounter(Asda2TitleId.Upgraded255, 20, 50);
        }

        public static void OnNpcDeath(NPC npc, Character killerChr)
        {
            if (Math.Abs(killerChr.Level - npc.Level) <= 31)
            {
                killerChr.UpdateTitleCounter(Asda2TitleId.Hunter150, 10, 100);
                killerChr.UpdateTitleCounter(Asda2TitleId.Exterminator151, 100, 1000);
                killerChr.UpdateTitleCounter(Asda2TitleId.Slayer152, 1000, 10000);
                killerChr.UpdateTitleCounter(Asda2TitleId.Fanatic153, 10000, 100000);
                killerChr.UpdateTitleCounter(Asda2TitleId.Maniac, 100000, 250000);
                killerChr.UpdateTitleCounter(Asda2TitleId.Lunatic, 250000, 500000);
                killerChr.UpdateTitleCounter(Asda2TitleId.Psycho, 500000, 1000000);
                if (npc.Entry.Rank == CreatureRank.Boss || npc.Entry.Rank == CreatureRank.WorldBoss)
                {
                    killerChr.UpdateTitleCounter(Asda2TitleId.Boss154, 200, 1000);
                    killerChr.UpdateTitleCounter(Asda2TitleId.KingBeast, 1000, 10000);
                    killerChr.UpdateTitleCounter(Asda2TitleId.BossReapr, 10000, 25000);
                    killerChr.UpdateTitleCounter(Asda2TitleId.BloodSucker, 25000, 50000);
                    killerChr.UpdateTitleCounter(Asda2TitleId.GreatReapr, 50000, 100000);
                    killerChr.CheckTitlesCollection(Asda2TitleId.BurtolReapr, Asda2TitleId.Boss154, Asda2TitleId.KingBeast,
                     Asda2TitleId.BossReapr, Asda2TitleId.BloodSucker, Asda2TitleId.GreatReapr);
                }


            }
            if (npc.Template.Id == 801)
            {
                killerChr.UpdateTitleCounter(Asda2TitleId.BlackChicken, 100, 1000);
            }
            if (npc.Template.Id == 805)
            {
                killerChr.UpdateTitleCounter(Asda2TitleId.BlueChicken, 10, 100);
            }
            if (npc.Template.Id == 800)
            {
                killerChr.UpdateTitleCounter(Asda2TitleId.GoldenChicken, 5, 30);
            }
            if (npc.Template.Id == 621 || npc.Template.Id == 622 || npc.Template.Id == 623 || npc.Template.Id == 624 || npc.Template.Id == 625 || npc.Template.Id == 802 || npc.Template.Id == 803)
            {
                killerChr.UpdateTitleCounter(Asda2TitleId.RedChicken, 10, 100);
            }
            killerChr.CheckTitlesCollection(Asda2TitleId.ChickenHunter, Asda2TitleId.BlackChicken,
                Asda2TitleId.BlueChicken, Asda2TitleId.GoldenChicken, Asda2TitleId.RedChicken);

        }

        public static void OnGuildWaveEnd(Character chr, int wave)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Reserved140, 5, 10);
            chr.UpdateTitleCounter(Asda2TitleId.Reserved141, 25, 50);
            chr.UpdateTitleCounter(Asda2TitleId.Reserved142, 75, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Reserved143, 250, 500);

            if (wave > 15 && wave < 28)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Reserved144, 25, 50);
            }

            if (wave > 28)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Reserved145, 5, 10);
            }

            chr.CheckTitlesCollection(Asda2TitleId.Reserved146, Asda2TitleId.Reserved143, Asda2TitleId.Reserved144, Asda2TitleId.Reserved145);
        }

        public static void OnItemSold(Asda2Item item, Character chr, int soldAmount)
        {
            if (item.ItemId == 33418)
                chr.UpdateTitleCounter(Asda2TitleId.Wolf155, 1, 500, soldAmount);
            if (item.ItemId == 33419)
                chr.UpdateTitleCounter(Asda2TitleId.Parasol156, 1, 500, soldAmount);
            if (item.ItemId == 33420)
                chr.UpdateTitleCounter(Asda2TitleId.Crab157, 1, 500, soldAmount);
            if (item.ItemId == 33421)
                chr.UpdateTitleCounter(Asda2TitleId.Duck158, 1, 500, soldAmount);
            if (item.ItemId == 33422)
                chr.UpdateTitleCounter(Asda2TitleId.Stamp159, 1, 500, soldAmount);
            if (item.ItemId == 33423)
                chr.UpdateTitleCounter(Asda2TitleId.Cat160, 1, 500, soldAmount);
            if (item.ItemId == 33424)
                chr.UpdateTitleCounter(Asda2TitleId.Mushroom161, 1, 500, soldAmount);
            if (item.ItemId == 33425)
                chr.UpdateTitleCounter(Asda2TitleId.Pickle162, 1, 500, soldAmount);
            if (item.ItemId == 33426)
                chr.UpdateTitleCounter(Asda2TitleId.Woodfolk163, 1, 500, soldAmount);
            if (item.ItemId == 33427)
                chr.UpdateTitleCounter(Asda2TitleId.Parrot164, 1, 500, soldAmount);
            if (item.ItemId == 33428)
                chr.UpdateTitleCounter(Asda2TitleId.Rat165, 1, 500, soldAmount);
            if (item.ItemId == 33429)
                chr.UpdateTitleCounter(Asda2TitleId.Golem167, 1, 500, soldAmount);
            if (item.ItemId == 33430)
                chr.UpdateTitleCounter(Asda2TitleId.Junkman168, 1, 500, soldAmount);
            if (item.ItemId == 33431)
                chr.UpdateTitleCounter(Asda2TitleId.Slime169, 1, 500, soldAmount);
            if (item.ItemId == 33432)
                chr.UpdateTitleCounter(Asda2TitleId.Scorpion170, 1, 500, soldAmount);
            if (item.ItemId == 33433)
                chr.UpdateTitleCounter(Asda2TitleId.Gnom171, 1, 500, soldAmount);
            if (item.ItemId == 33434)
                chr.UpdateTitleCounter(Asda2TitleId.Lizard172, 1, 500, soldAmount);
            if (item.ItemId == 33435)
                chr.UpdateTitleCounter(Asda2TitleId.Serpent173, 1, 500, soldAmount);
            if (item.ItemId == 33436)
                chr.UpdateTitleCounter(Asda2TitleId.Pawn174, 1, 500, soldAmount);
            if (item.ItemId == 33437)
                chr.UpdateTitleCounter(Asda2TitleId.Rook175, 1, 500, soldAmount);
            if (item.ItemId == 33438)
                chr.UpdateTitleCounter(Asda2TitleId.Bishop176, 1, 500, soldAmount);
            if (item.ItemId == 33439)
                chr.UpdateTitleCounter(Asda2TitleId.Knight177, 1, 500, soldAmount);
            if (item.ItemId == 33440)
                chr.UpdateTitleCounter(Asda2TitleId.Bugly179, 1, 500, soldAmount);
            if (item.ItemId == 33441)
                chr.UpdateTitleCounter(Asda2TitleId.Mutant180, 1, 100, soldAmount);
            if (item.ItemId == 33442)
                chr.UpdateTitleCounter(Asda2TitleId.Deckron181, 1, 400, soldAmount);
            if (item.ItemId == 33443)
                chr.UpdateTitleCounter(Asda2TitleId.River182, 1, 30, soldAmount);
            if (item.ItemId == 33444)
                chr.UpdateTitleCounter(Asda2TitleId.Spring183, 1, 30, soldAmount);
            if (item.ItemId == 33445)
                chr.UpdateTitleCounter(Asda2TitleId.Jungle184, 1, 30, soldAmount);
            if (item.ItemId == 33446)
                chr.UpdateTitleCounter(Asda2TitleId.Coast185, 1, 30, soldAmount);
            if (item.ItemId == 33447)
                chr.UpdateTitleCounter(Asda2TitleId.Desert186, 1, 30, soldAmount);
            if (item.ItemId == 33448)
                chr.UpdateTitleCounter(Asda2TitleId.Tyrant187, 1, 30, soldAmount);
            if (item.ItemId == 33449)
                chr.UpdateTitleCounter(Asda2TitleId.Queen191, 1, 20, soldAmount);
            if (item.ItemId == 33450)
                chr.UpdateTitleCounter(Asda2TitleId.Night192, 1, 20, soldAmount);
            if (item.ItemId == 33451)
                chr.UpdateTitleCounter(Asda2TitleId.Volcano193, 1, 20, soldAmount);
            if (item.ItemId == 33452)
                chr.UpdateTitleCounter(Asda2TitleId.King194, 1, 20, soldAmount);
            if (item.ItemId == 33453)
                chr.UpdateTitleCounter(Asda2TitleId.Sphinx195, 1, 20, soldAmount);
            if (item.ItemId == 33454)
                chr.UpdateTitleCounter(Asda2TitleId.Dragon196, 1, 20, soldAmount);
            if (item.ItemId == 33455)
                chr.UpdateTitleCounter(Asda2TitleId.Reserved197, 1, 20, soldAmount);
            if (item.ItemId == 33457)
                chr.UpdateTitleCounter(Asda2TitleId.Reserved198, 1, 20, soldAmount);
            
            if (item.ItemId == 33456)
                chr.UpdateTitleCounter(Asda2TitleId.U188, 1, 30, soldAmount);
            if (item.ItemId == 33458)
                chr.UpdateTitleCounter(Asda2TitleId.U189, 1, 30, soldAmount);
            if (item.ItemId == 33459)
                chr.UpdateTitleCounter(Asda2TitleId.U190, 1, 30, soldAmount);
            //if (item.ItemId == 36981)
            //    chr.UpdateTitleCounter(Asda2TitleId.ThanksGiving428, 1, 1, soldAmount);
            //if (item.ItemId == 36983)
            //    chr.UpdateTitleCounter(Asda2TitleId.Pie430, 1, 10, soldAmount);
            //if (item.ItemId == 36984)
            //    chr.UpdateTitleCounter(Asda2TitleId.Potato431, 1, 10, soldAmount);
            //if (item.ItemId == 40600)
            //    chr.UpdateTitleCounter(Asda2TitleId.Winter434, 1, 5, soldAmount);
            //if (item.ItemId == 40602)
            //    chr.UpdateTitleCounter(Asda2TitleId.Snowman435, 1, 10, soldAmount);
            //if (item.ItemId == 40603)
            //    chr.UpdateTitleCounter(Asda2TitleId.Stanta436, 1, 50, soldAmount);
            if (item.ItemId == 56455)
                chr.UpdateTitleCounter(Asda2TitleId.mummy, 1, 300, soldAmount);
            if (item.ItemId == 56456)
                chr.UpdateTitleCounter(Asda2TitleId.mummy1, 1, 300, soldAmount);
            if (item.ItemId == 56457)
                chr.UpdateTitleCounter(Asda2TitleId.mummy2, 1, 300, soldAmount);
            if (item.ItemId == 56458)
                chr.UpdateTitleCounter(Asda2TitleId.mummy3, 1, 300, soldAmount);
            if (item.ItemId == 56459)
                chr.UpdateTitleCounter(Asda2TitleId.mummy4, 1, 300, soldAmount);
            if (item.ItemId == 56460)
                chr.UpdateTitleCounter(Asda2TitleId.mummy5, 1, 300, soldAmount);
            if (item.ItemId == 56461)
                chr.UpdateTitleCounter(Asda2TitleId.mummy6, 1, 300, soldAmount);
            if (item.ItemId == 56462)
                chr.UpdateTitleCounter(Asda2TitleId.mummy7, 1, 300, soldAmount);
            if (item.ItemId == 56463)
                chr.UpdateTitleCounter(Asda2TitleId.mummy8, 1, 150, soldAmount);
            if (item.ItemId == 56464)
                chr.UpdateTitleCounter(Asda2TitleId.mummy9, 1, 150, soldAmount);
            if (item.ItemId == 56470)
                chr.UpdateTitleCounter(Asda2TitleId.mummy10, 1, 150, soldAmount);
            if (item.ItemId == 56465)
                chr.UpdateTitleCounter(Asda2TitleId.mummy11, 1, 50, soldAmount);
            if (item.Template.Quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareSeller, 200, 2000, soldAmount);
            }
            if (item.Template.Quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroSeller, 100, 1000, soldAmount);
            }
            if (item.Template.Quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendrySeller, 50, 500, soldAmount);
            }
            if (item.Template.Quality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpeicalSeller, 10, 100, soldAmount);
            }
            if (item.Template.Quality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UniqueSeller, 5, 50, soldAmount);
            }
            if (item.Template.Quality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.MakSeller, 1, 5, soldAmount);
            }
            chr.CheckTitlesCollection(Asda2TitleId.UltimateSeller, Asda2TitleId.RareSeller, Asda2TitleId.HeroSeller,
               Asda2TitleId.LegendrySeller, Asda2TitleId.SpeicalSeller, Asda2TitleId.UniqueSeller, Asda2TitleId.MakSeller);

        }

        public static void OnFindSoulmateWindowOpened(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Searching85, 1, 5);
            CheckTrueLove(chr);
        }

        public static void OnNewSoulmate(Character chr)
        {
            chr.GainTitle(Asda2TitleId.Friend86);
            CheckTrueLove(chr);
        }

        public static void OnNewSoulmatingEnd(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Heartbreaker89, 10, 30);
            CheckTrueLove(chr);
        }

        public static void OnSoulmatingLevelChanged(byte level, Character chr)
        {
            chr.CheckTitle(Asda2TitleId.Companion87, () => level >= 5, () => level >= 15);
            CheckTrueLove(chr);
            chr.CheckTitle(Asda2TitleId.Soulmate88, () => level >= 15, () => level >= 30);
            CheckTrueLove(chr);
        }

        public static void OnSoulmateMessage(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.LoveNote90, 100, 300);
            CheckTrueLove(chr);
        }

        public static void OnSoulmateHealing(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Cherished91, 100, 1000);
            CheckTrueLove(chr);
        }

        public static void OnSoulmateEatApple(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.SnowWhite93, 100, 500);
            CheckTrueLove(chr);
        }

        public static void OnSoulmateInfoRequest(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.OldFlame95, 50, 300);
            CheckTrueLove(chr);
        }

        public static void CheckTrueLove(Character chr)
        {
            chr.CheckTitlesCollection(Asda2TitleId.TrueLove94, Asda2TitleId.Searching85, Asda2TitleId.Friend86,
                Asda2TitleId.Heartbreaker89, Asda2TitleId.Companion87, Asda2TitleId.LoveNote90, Asda2TitleId.Cherished91,
                Asda2TitleId.SnowWhite93, Asda2TitleId.OldFlame95);
        }

        public static void OnSuccessDig(Character chr, int itemid, Asda2ItemQuality itemqaulity, IRealmClient client)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Shovel285, 10, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Excavator286, 100, 5000);
            chr.UpdateTitleCounter(Asda2TitleId.PremiumShovel, 5000, 10000);
            chr.UpdateTitleCounter(Asda2TitleId.LegendryShovel, 10000, 30000);
            chr.UpdateTitleCounter(Asda2TitleId.ShovelLover, 30000, 50000);
            chr.UpdateTitleCounter(Asda2TitleId.crazyshovel, 50000, 75000);
            if (itemid == 20622)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Geologist294, 1000, 10000);
            }
            if (itemid == 31407)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Treasure293, 100, 1000);
            }
            if (itemid == 31408)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Lucky295, 77, 777);
            }
            var map = World.GetNonInstancedMap(MapId.Silaris);
            var map1 = World.GetNonInstancedMap(MapId.Alpia);
            var map2 = World.GetNonInstancedMap(MapId.SunnyCoast);
            var map3 = World.GetNonInstancedMap(MapId.Flabis);
            var vector = new Vector3(131f + map1.Offset, 265f + map1.Offset, 0f);
            var vector1 = new Vector3(110f + map.Offset, 144f + map.Offset, 0f);
            var vector2 = new Vector3(226f + map2.Offset, 353f + map2.Offset, 0f);
            var vector3 = new Vector3(270f + map3.Offset, 263f + map3.Offset, 0f);
            if (client.ActiveCharacter.Position.GetDistance(vector) < 10)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Burial287, 100, 500);
            }
            if (client.ActiveCharacter.Position.GetDistance(vector1) < 50)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Archaeologist288, 100, 500);
            }
            if (client.ActiveCharacter.Position.GetDistance(vector2) < 25)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Shipwreck289, 100, 500);
            }
            if (client.ActiveCharacter.Position.GetDistance(vector3) < 50)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Oasis290, 100, 500);
            }
            chr.CheckTitlesCollection(Asda2TitleId.Explorer291, Asda2TitleId.Burial287, Asda2TitleId.Archaeologist288,
                Asda2TitleId.Shipwreck289, Asda2TitleId.Oasis290);

        }

        public static void OnSuccessFishing(Character chr, int fishId, int fishSize)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Fisherman305, 200, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.AmFisher, 3000, 5000);
            chr.UpdateTitleCounter(Asda2TitleId.RFisher, 5000, 10000);
            chr.UpdateTitleCounter(Asda2TitleId.PFisher, 10000, 30000);
            chr.UpdateTitleCounter(Asda2TitleId.LFisher, 30000, 50000);
            chr.UpdateTitleCounter(Asda2TitleId.FishLover, 50000, 100000);
            chr.UpdateTitleCounter(Asda2TitleId.fishcrazy, 100000, 125000);

            if (fishId == 31715)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Carp310, 100, 500);

            }
            if (fishId == 31720)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Koi311, 100, 500);

            }
            if (fishId == 31725)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Goldfish312, 100, 500);

            }
            if (fishId == 31730)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Eel313, 100, 500);

            }
            if (fishId == 31735)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Catfish314, 100, 500);

            }
            if (fishId == 31740)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Mackerel315, 100, 500);

            }
            if (fishId == 31745)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Tuna316, 100, 500);

            }
            if (fishId == 31750)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Cod317, 100, 500);

            }
            if (fishId == 31755)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Black318, 100, 500);

            }
            if (fishId == 31718)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Golden328, 100, 500);

            }
            if (fishId == 31723)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Thief329, 100, 500);

            }
            if (fishId == 31728)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Spotted330, 100, 500);

            }
            if (fishId == 31733)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Armored331, 100, 500);

            }
            if (fishId == 31738)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Stray332, 100, 500);

            }
            if (fishId == 31743)
            {
                chr.UpdateTitleCounter(Asda2TitleId.School333, 100, 500);

            }
            if (fishId == 31748)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Tiger334, 100, 500);

            }
            if (fishId == 31753)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Muscular335, 100, 500);

            }
            if (fishId == 31719)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Rainbow336, 100, 300);

            }
            if (fishId == 31724)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Glowing337, 100, 300);

            }
            if (fishId == 31729)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Ruby338, 100, 300);

            }
            if (fishId == 31734)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Blast339, 100, 300);

            }
            if (fishId == 31739)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Gravity340, 100, 300);

            }
            if (fishId == 31744)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Powerful341, 100, 300);

            }
            if (fishId == 31749)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Millenium342, 100, 300);

            }
            if (fishId == 31754)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Emerald343, 100, 300);

            }


            if (fishId == 31716)
            {
                if (fishSize >= 85)
                    chr.GainTitle(Asda2TitleId.Copper320);

            }

            if (fishId == 31721)
            {
                if (fishSize >= 95)
                    chr.GainTitle(Asda2TitleId.Clay321);
            }

            if (fishId == 31726)
            {
                if (fishSize >= 75)
                    chr.GainTitle(Asda2TitleId.Metallic322);
            }

            if (fishId == 31731)
            {
                if (fishSize >= 105)
                    chr.GainTitle(Asda2TitleId.Sharp323);
            }

            if (fishId == 31736)
            {
                if (fishSize >= 115)
                    chr.GainTitle(Asda2TitleId.Stone324);
            }

            if (fishId == 31741)
            {
                if (fishSize >= 95)
                    chr.GainTitle(Asda2TitleId.Angry325);
            }

            if (fishId == 31746)
            {
                if (fishSize >= 165)
                    chr.GainTitle(Asda2TitleId.Iron326);
            }

            if (fishId == 31751)
            {
                if (fishSize >= 145)
                    chr.GainTitle(Asda2TitleId.Wooden327);
            }
        }


        public static void OnNewPet(Character chr, int eggId, Asda2ItemQuality eggQuality)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Adopted355, 1, 10);

            chr.GainTitle(Asda2TitleId.Pet356);

            if (eggId == 31869 || eggId == 31871 || eggId == 31875 || eggId == 31878 || eggId == 31880 || eggId == 31884 || eggId == 31887 || eggId == 31889 || eggId == 31893)
                chr.GainTitle(Asda2TitleId.Beast359);
            if (eggId == 31870 || eggId == 31873 || eggId == 31874 || eggId == 31879 || eggId == 31882 || eggId == 31883 || eggId == 31888 || eggId == 31891 || eggId == 31892)
                chr.GainTitle(Asda2TitleId.Vegetable360);
            if (eggId == 31872 || eggId == 31876 || eggId == 31877 || eggId == 31881 || eggId == 31885 || eggId == 31886 || eggId == 31890 || eggId == 31894 || eggId == 31895)
                chr.GainTitle(Asda2TitleId.Machine361);

            if (eggQuality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Exotic362, 1, 10);
                chr.UpdateTitleCounter(Asda2TitleId.TrainerRare, 4, 40);
            }
            if (eggQuality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroPet, 1, 5);
                chr.UpdateTitleCounter(Asda2TitleId.TrainerHero, 2, 20);
            }
               
            if (eggQuality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.PetKing, 1, 1);
                chr.UpdateTitleCounter(Asda2TitleId.TrainerLegendry, 1, 10);
            }
                
            /*if (eggQuality == Asda2ItemQuality.Spiecal)
                chr.UpdateTitleCounter(Asda2TitleId.Speicalpet, 1, 1);*/
            if (eggQuality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.eventpet, 1, 1);
            }
                
            chr.CheckTitlesCollection(Asda2TitleId.UlitmateTrainer, Asda2TitleId.TrainerRare, Asda2TitleId.TrainerHero, Asda2TitleId.TrainerLegendry);

        }

        public static void OnPetCountChanged(int count, Character chr)
        {
            chr.CheckTitle(Asda2TitleId.Farm357, () => count >= 1, () => count >= 10);
            chr.CheckTitle(Asda2TitleId.Zoo358, () => count >= 10, () => count >= 25);
            chr.CheckTitle(Asda2TitleId.pets1, () => count >= 25, () => count >= 40);
            chr.CheckTitle(Asda2TitleId.pets2, () => count >= 40, () => count >= 50);
            chr.CheckTitle(Asda2TitleId.pets3, () => count >= 50, () => count >= 60);
            chr.CheckTitle(Asda2TitleId.pets4, () => count >= 60, () => count >= 70);

        }

        public static void OnNewMount(Character chr, int id)
        {
            /*chr.GainTitle(Asda2TitleId.Reserved241);
            if (id == 189)
            {
                chr.GainTitle(Asda2TitleId.Bugly179);
            }*/

        }
        public static void OnSelectFaction(byte factionId, Character chr)
        {
            switch (factionId)
            {
                case 0://light
                    chr.GainTitle(Asda2TitleId.Light120);
                    break;
                case 1://dark
                    chr.GainTitle(Asda2TitleId.Dark121);
                    break;

            }
        }

        public static void OnWinDuel(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Duelist122, 10, 25);
            chr.UpdateTitleCounter(Asda2TitleId.Brawler123, 25, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Undefeated124, 100, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.BloodyDuelist, 1000, 2000);
            chr.UpdateTitleCounter(Asda2TitleId.LegendDuel, 2000, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.BloodLover, 3000, 5000);

        }

        public static void OnWinWar(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Challenger125, 1, 10);
            chr.UpdateTitleCounter(Asda2TitleId.Winner126, 10, 50);
            chr.UpdateTitleCounter(Asda2TitleId.Champion127, 50, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Conqueror128, 100, 500);
            CheckGod(chr);
        }

        public static void OnLoseWar(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Loser, 1, 10);
            chr.UpdateTitleCounter(Asda2TitleId.BigLose, 10, 50);
            chr.UpdateTitleCounter(Asda2TitleId.GreatLose, 50, 100);
            chr.UpdateTitleCounter(Asda2TitleId.UltimateLose, 100, 500);

        }

        public static void OnWarKilling(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Soldier129, 10, 25);
            chr.UpdateTitleCounter(Asda2TitleId.Killer130, 25, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Assassin131, 100, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.BloodyKiller, 1000, 5000);
            chr.UpdateTitleCounter(Asda2TitleId.BeastKiller, 5000, 10000);
            CheckGod(chr);
        }

        public static void OnCraftingLevelChanged(byte craftingLevel, Character chr)
        {
            chr.CheckTitle(Asda2TitleId.Apprentice265, () => craftingLevel >= 1, () => craftingLevel >= 2);
            chr.CheckTitle(Asda2TitleId.Master266, () => craftingLevel >= 2, () => craftingLevel >= 10);
        }

        public static void OnItemCrafted(int craftedItemId, Character chr, byte rarity)
        {
            var itemTemplate = Asda2ItemMgr.GetTemplate(craftedItemId);

            if (itemTemplate.IsWeapon)
                chr.UpdateTitleCounter(Asda2TitleId.Weapon272, 1, 10);

            if (itemTemplate.IsArmor)
                chr.UpdateTitleCounter(Asda2TitleId.Armor273, 1, 10);

            if (itemTemplate.IsAccessories)
                chr.UpdateTitleCounter(Asda2TitleId.Jewel274, 1, 10);

            if (itemTemplate.IsEquipment)
                chr.UpdateTitleCounter(Asda2TitleId.Blacksmith268, 10, 100);

            if (itemTemplate.IsPotion)
                chr.UpdateTitleCounter(Asda2TitleId.Alchemist269, 20, 300);

            if ((Asda2ItemQuality)rarity == Asda2ItemQuality.Purple)
                chr.UpdateTitleCounter(Asda2TitleId.Rare270, 10, 100);
        }

        public static void OnPetRemoved(Character chr, int rarity)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Abandoned363, 5, 20);
            if (rarity == 2)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareLeaver, 5, 30);
            }
            if (rarity == 3)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroLeaver, 5, 15);
            }
            if (rarity == 4)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendLeaver, 1, 5);
            }
            chr.CheckTitlesCollection(Asda2TitleId.TheBlackHeart, Asda2TitleId.Abandoned363, Asda2TitleId.RareLeaver,
                Asda2TitleId.HeroLeaver, Asda2TitleId.LegendLeaver);
        }

        public static void OnPetStarve(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Neglected364, 1, 10);
        }

        public static void OnPetLevelChanged(byte level, Character chr)
        {
            if (level == 5)
                chr.UpdateTitleCounter(Asda2TitleId.Mature368, 1, 5);

            chr.CheckTitle(Asda2TitleId.Trainer369, () => level >= 6, () => level == 10);
        }

        public static void OnClassChange(ClassId @class, Character chr, byte realProffLevel)
        {
            chr.CheckTitle(Asda2TitleId.Warrior15, () => realProffLevel >= 1, () => realProffLevel >= 2);

            switch (@class)
            {
                case ClassId.AtackMage:
                    chr.GainTitle(Asda2TitleId.Hells12);
                    break;
                case ClassId.HealMage:
                    chr.GainTitle(Asda2TitleId.Heavens14);
                    break;
                case ClassId.SupportMage:
                    chr.GainTitle(Asda2TitleId.Earths13);
                    break;
                case ClassId.OHS:
                    chr.GainTitle(Asda2TitleId.Impenetrable7);
                    break;
                case ClassId.Spear:
                    chr.GainTitle(Asda2TitleId.Berserk9);
                    break;
                case ClassId.THS:
                    chr.GainTitle(Asda2TitleId.Mighty8);
                    break;
                case ClassId.Bow:
                    chr.GainTitle(Asda2TitleId.Bloody11);
                    break;
                case ClassId.Crossbow:
                    chr.GainTitle(Asda2TitleId.Critical10);
                    break;
            }
        }

        private const uint AfkDiscoverSeconds = 60 * 60 * 1;
        private const uint AfkGainSeconds = 60 * 60 * 12;

        public static void OnTotalPlayTimeChanged(Character chr, uint totalseconds)
        {
            chr.CheckTitle(Asda2TitleId.AFK35,
                () => totalseconds >= AfkDiscoverSeconds,
                () => totalseconds >= AfkGainSeconds);
        }

        public static void OnWheIsHere(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Boring39, 50, 500);
        }

        public static void OnTeleportedByCrystal(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Lazy40, 1000, 3000);
        }

        public static void OnTitleCountChanged(Character chr)
        {
            var titleCount = chr.GetTitlesCount();
            chr.CheckTitle(Asda2TitleId.Collector42, () => titleCount >= 10, () => titleCount >= 50);
            chr.CheckTitle(Asda2TitleId.Maniac43, () => titleCount >= 50, () => titleCount >= 150);
            chr.CheckTitle(Asda2TitleId.TitleMaster, () => titleCount >= 150, () => titleCount >= 250);
            chr.CheckTitle(Asda2TitleId.TitleKing, () => titleCount >= 250, () => titleCount >= 400);
            chr.CheckTitle(Asda2TitleId.TitleLegend, () => titleCount >= 400, () => titleCount >= 500);
        }

        public static void OnSkillUsed(Character chr, short skillId)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Skilled44, 50, 300);
        }

        public static void OnWishperChat(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Whispering45, 200, 3000);
        }

        public static void OnGetCharacterInfo(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Curious46, 50, 100);
        }

        public static void OnHairChange(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Beautiful47, 1, 10);
        }

        public static void OnAuctionItemSold(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Merchant48, 1, 50);
        }

        public static void OnNewFriend(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Popular49, 1, 25);
        }

        public static void OnJoinGuild(Character chr)
        {
            chr.GainTitle(Asda2TitleId.Loyal105);
        }

        public static void OnGuildCreated(Character chr)
        {
            chr.GainTitle(Asda2TitleId.Leader106);
        }

        public static void OnFactionRankChanged(Character chr, int rank)
        {
            /*if (chr.Asda2FactionRank == 1)
                chr.DiscoverTitle(Asda2TitleId.Private132);
            if (chr.Asda2FactionRank == 2)
                chr.GainTitle(Asda2TitleId.Private132);
            if (chr.Asda2FactionRank == 3)
                chr.DiscoverTitle(Asda2TitleId.Sergeant133);
            if (chr.Asda2FactionRank == 4)
                chr.GainTitle(Asda2TitleId.Sergeant133);
            if (chr.Asda2FactionRank == 6)
                chr.DiscoverTitle(Asda2TitleId.Officer134);
            if (chr.Asda2FactionRank == 9)
                chr.GainTitle(Asda2TitleId.Officer134);
            if (chr.Asda2FactionRank == 10)
                chr.DiscoverTitle(Asda2TitleId.Captain135);
            if (chr.Asda2FactionRank == 13)
                chr.GainTitle(Asda2TitleId.Captain135);
            if (chr.Asda2FactionRank == 14)
                chr.DiscoverTitle(Asda2TitleId.Major136);
            if (chr.Asda2FactionRank == 16)
                chr.GainTitle(Asda2TitleId.Major136);
            if (chr.Asda2FactionRank == 17)
                chr.DiscoverTitle(Asda2TitleId.Colonel137);
            if (chr.Asda2FactionRank == 19)
                chr.GainTitle(Asda2TitleId.Colonel137);
            if (chr.Asda2FactionRank == 19)
                chr.DiscoverTitle(Asda2TitleId.General138);
            if (chr.Asda2FactionRank == 20)
                chr.GainTitle(Asda2TitleId.General138);
            CheckGod(chr);*/
            chr.CheckTitle(Asda2TitleId.Private132, () => rank >= 1, () => rank >= 2);
            chr.CheckTitle(Asda2TitleId.Sergeant133, () => rank >= 2, () => rank >= 4);
            chr.CheckTitle(Asda2TitleId.Officer134, () => rank >= 4, () => rank >= 9);
            chr.CheckTitle(Asda2TitleId.Captain135, () => rank >= 9, () => rank >= 13);
            chr.CheckTitle(Asda2TitleId.Major136, () => rank >= 13, () => rank >= 16);
            chr.CheckTitle(Asda2TitleId.Colonel137, () => rank >= 16, () => rank >= 19);
            chr.CheckTitle(Asda2TitleId.General138, () => rank >= 19, () => rank >= 20);
            CheckGod(chr);
        }
        public static void CheckGod(Character chr)
        {
            chr.CheckTitlesCollection(Asda2TitleId.General138, Asda2TitleId.Conqueror128,
                Asda2TitleId.Assassin131);

        }
        public static void OnHealPotionUse(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Weakling215, 100, 3000);
        }

        public static void OnTeleportScrolUse(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Stalker222, 10, 50);
            //chr.UpdateTitleCounter(Asda2TitleId.Traveler223, 10, 50);
            /*if (chr.TeleportPoints.Any())
            {
                chr.UpdateTitleCounter(Asda2TitleId.Traveler223, 10, 50);
            }*/
        }

        public static void OnTeleportingToTelepotPoint(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Traveler223, 10, 50);
        }

        public static void OnReturnScrollUse(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Returning216, 50, 500);
        }

        public static void OnResurectUse(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Savior217, 30, 300);
        }

        public static void OnResetAllSkills(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Perfectionist218, 1, 7);
        }

        public static void OnUseVeiche(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Rapid219, 1, 7);
        }

        public static void OnUseAutoFishDig(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Automatic225, 1, 7);
        }

        public static void OnBuyItem(Asda2Item item, Character chr)
        {

            if (item.Template.IsPotion)
                chr.UpdateTitleCounter(Asda2TitleId.Stocked226, 100, 3000, item.Amount);

        }

        public static void OnItemDeleted(Character chr, Asda2Item item)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Trash227, 1000, 3000);
            if (item.Template.Quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareDelettoer, 100, 1000);
            }
            if (item.Template.Quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroDelete, 50, 500);
            }

            if (item.Template.Quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryDelete, 25, 100);
            }
            if (item.Template.Quality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpecialDelete, 5, 20);
            }
            if (item.Template.Quality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Uniquedelete, 1, 5);
            }
            if (item.Template.Quality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventDelete, 25, 200);
            }
            if (item.Template.Quality == Asda2ItemQuality.Machinic)
            {
               chr.UpdateTitleCounter(Asda2TitleId.WiredDeletor, 1,5);
            }
            chr.CheckTitlesCollection(Asda2TitleId.TrashMaster, Asda2TitleId.RareDelettoer, Asda2TitleId.HeroDelete,
                Asda2TitleId.LegendryDelete, Asda2TitleId.SpecialDelete, Asda2TitleId.Uniquedelete,
                Asda2TitleId.EventDelete);
        }

        public static void OnItemDisasembled(Character chr, Asda2Item newItem, Asda2Item item)
        {
            if (newItem != null && newItem.ItemId == 20679)
                chr.UpdateTitleCounter(Asda2TitleId.Glittering229, 50, 500, newItem.Amount);
            if (newItem != null && newItem.ItemId == 20680)
                chr.UpdateTitleCounter(Asda2TitleId.Mystic230, 50, 300, newItem.Amount);
            if (newItem != null && newItem.ItemId == 20681)
                chr.UpdateTitleCounter(Asda2TitleId.Ultimate231, 10, 100, newItem.Amount);

            chr.UpdateTitleCounter(Asda2TitleId.Destructive228, 50, 500);
            if (item.Template.Quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.DisRare, 30, 300);
            }
            if (item.Template.Quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.DisHero, 15, 150);
            }
            if (item.Template.Quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.DisLegend, 5, 50);
            }
            chr.CheckTitlesCollection(Asda2TitleId.ItemDis, Asda2TitleId.Destructive228, Asda2TitleId.DisRare,
                Asda2TitleId.DisHero, Asda2TitleId.DisLegend);

        }

        public static void OnAvatarDisasembled(Character chr, Asda2Item avatar, Asda2ItemQuality qaulity)
        {
            chr.UpdateTitleCounter(Asda2TitleId.AvatarDism, 10, 100);
            chr.UpdateTitleCounter(Asda2TitleId.DismDev, 100, 500);
            chr.UpdateTitleCounter(Asda2TitleId.SilverDism, 500, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.GoldenDism, 1000, 2000);
            chr.UpdateTitleCounter(Asda2TitleId.PlatinumDis, 2000, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.DaimondDis, 3000, 5000);
            chr.CheckTitlesCollection(Asda2TitleId.TheGreatDism, Asda2TitleId.AvatarDism, Asda2TitleId.DismDev,
                Asda2TitleId.SilverDism, Asda2TitleId.GoldenDism, Asda2TitleId.PlatinumDis, Asda2TitleId.DaimondDis);
            if (qaulity == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareAvatar, 100, 1000);
            }
            if (qaulity == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroAvatar, 50, 500);
                chr.UpdateTitleCounter(Asda2TitleId.DisAvhero, 200, 2000);
            }
            if (qaulity == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryAvatar, 10, 100);
                chr.UpdateTitleCounter(Asda2TitleId.DisavLegend, 50, 500);
            }
            if (qaulity == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpeicalAvatar, 5, 20);
                chr.UpdateTitleCounter(Asda2TitleId.DisavSpeical, 20, 200);
            }
            if (qaulity == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UniqueAvatar, 1, 10);
                chr.UpdateTitleCounter(Asda2TitleId.DisavUnique, 10, 100);
            }
            if (qaulity == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventAvatar, 5, 30);
            }
            if (qaulity == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.DangerosDis, 1, 5);
            }

            chr.CheckTitlesCollection(Asda2TitleId.AvatarDestroyer, Asda2TitleId.RareAvatar, Asda2TitleId.HeroAvatar,
                Asda2TitleId.LegendryAvatar, Asda2TitleId.SpeicalAvatar, Asda2TitleId.UniqueAvatar,
                Asda2TitleId.EventAvatar);
            chr.CheckTitlesCollection(Asda2TitleId.DisavUltimate, Asda2TitleId.DisAvhero, Asda2TitleId.DisavLegend, Asda2TitleId.DisavLegend,
                Asda2TitleId.DisavSpeical, Asda2TitleId.DisavUnique);

        }

        public static void OnAdvnacedEnchant(Character chr, int itemId, Asda2ItemQuality quality)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Advanced, 1, 10);
            chr.UpdateTitleCounter(Asda2TitleId.AdvanceUpgrade, 10, 50);
            chr.UpdateTitleCounter(Asda2TitleId.AdvancedBronze, 50, 100);
            chr.UpdateTitleCounter(Asda2TitleId.AdvancedSilver, 100, 200);
            chr.UpdateTitleCounter(Asda2TitleId.AdvancedGold, 200, 300);
            chr.UpdateTitleCounter(Asda2TitleId.AdvancedPlatinum, 300, 500);
            chr.UpdateTitleCounter(Asda2TitleId.AdvancedDiamond, 500, 1000);
            chr.CheckTitlesCollection(Asda2TitleId.AdvancedMaster, Asda2TitleId.Advanced, Asda2TitleId.AdvanceUpgrade,
                Asda2TitleId.AdvancedBronze, Asda2TitleId.AdvancedSilver, Asda2TitleId.AdvancedGold,
                Asda2TitleId.AdvancedPlatinum, Asda2TitleId.AdvancedDiamond);
            if (quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.AdavncedRare, 25, 250);
            }
            if (quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.AdvancedHero, 15, 150);
            }
            if (quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.AdvancedLegendry, 10, 100);
            }
            chr.CheckTitlesCollection(Asda2TitleId.GearAdvancer, Asda2TitleId.AdavncedRare, Asda2TitleId.AdvancedHero, Asda2TitleId.AdvancedLegendry);

        }

        public static void OnBossSummon(Character chr, uint mobid)
        {

        }

        public static void OnMailItems(Character chr, int itemid)
        {

        }

        public static void OnOptionScrollUse(Character chr, Asda2ItemQuality quality)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Options, 10, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Bronzeoption, 100, 300);
            chr.UpdateTitleCounter(Asda2TitleId.Silveroption, 300, 500);
            chr.UpdateTitleCounter(Asda2TitleId.Goldenoption, 500, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.Platinumoption, 1000, 2000);
            chr.UpdateTitleCounter(Asda2TitleId.Dimondoption, 2000, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.Ultimateoption, 3000, 5000);
            chr.CheckTitlesCollection(Asda2TitleId.OptionMaster, Asda2TitleId.Options, Asda2TitleId.Bronzeoption,
            Asda2TitleId.Silveroption, Asda2TitleId.Goldenoption, Asda2TitleId.Platinumoption,
            Asda2TitleId.Dimondoption, Asda2TitleId.Ultimateoption);
            if (quality == Asda2ItemQuality.White)
            {
                chr.UpdateTitleCounter(Asda2TitleId.CommonOption, 100, 1000);
            }
            if (quality == Asda2ItemQuality.Yello)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Uncommonoption, 50, 500);
            }
            if (quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Rareoption, 30, 300);
            }
            if (quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroOption, 20, 200);
            }
            if (quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryOption, 10, 100);
            }
            chr.CheckTitlesCollection(Asda2TitleId.WeaponOptioner, Asda2TitleId.CommonOption, Asda2TitleId.Uncommonoption,
                Asda2TitleId.Rareoption, Asda2TitleId.HeroOption, Asda2TitleId.LegendryOption);

        }
        public static void OnMainOpenRequest(Character chr)
        {

        }

        public static void OnMailMessageSendRequest(Character chr)
        {

        }
        public static void OnStealLoot(Character chr, Asda2ItemQuality itemqality)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Bandit232, 100, 1000);
            if (itemqality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareThif, 50, 500);

            }
            if (itemqality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroThif, 20, 200);

            }
            if (itemqality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryThif, 10, 100);

            }
            if (itemqality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpeicalThif, 2, 20);

            }
            if (itemqality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UniqueThif, 1, 10);

            }
            if (itemqality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventThif, 10, 30);

            }
            if (itemqality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UltimateThif, 1, 5);

            }
            chr.CheckTitlesCollection(Asda2TitleId.ProThif, Asda2TitleId.RareThif, Asda2TitleId.HeroThif,
                Asda2TitleId.LegendryThif, Asda2TitleId.SpeicalThif, Asda2TitleId.UniqueThif, Asda2TitleId.EventThif);

        }

        public static void OnItemPickUp(Character chr, Asda2ItemQuality quality, int itemid)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Gatherer233, 1000, 3000);
            if (quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RarePicker, 100, 1000);
            }
            if (quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroPicker, 50, 500);
            }
            if (quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryPicker, 30, 300);
            }
            if (quality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpeicalPicker, 10, 20);
            }
            if (quality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UniquePicker, 5, 10);
            }
            if (quality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventPicker, 10, 100);
            }
            if (quality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UtlimatePicker, 1, 1);
            }

            chr.CheckTitlesCollection(Asda2TitleId.PickingMaster, Asda2TitleId.RarePicker, Asda2TitleId.HeroPicker,
                Asda2TitleId.LegendryPicker, Asda2TitleId.SpeicalPicker, Asda2TitleId.UniquePicker,
                Asda2TitleId.EventPicker);
            if (itemid == 30949)
            {
                chr.UpdateTitleCounter(Asda2TitleId.GoldBooster, 30, 500);
            }
            if (itemid == 56789)
            {
                chr.UpdateTitleCounter(Asda2TitleId.AvatarBostter, 35, 350);
            }
            if (itemid == 56790)
            {
                chr.UpdateTitleCounter(Asda2TitleId.PetBooster, 35, 350);
            }
            if (itemid == 56791)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UpgradeBooster, 35, 350);
            }
            if (itemid == 56852)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RingBostter, 35, 350);
            }
            if (itemid == 56520)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LuckyCoinBooster, 10, 200);
            }
            if (itemid == 56851)
            {
                chr.UpdateTitleCounter(Asda2TitleId.TheUltimateBooster, 5, 50);
            }
            chr.CheckTitlesCollection(Asda2TitleId.TheUltimatePicker, Asda2TitleId.GoldBooster, Asda2TitleId.AvatarBostter,
                Asda2TitleId.PetBooster,
                Asda2TitleId.RingBostter, Asda2TitleId.UpgradeBooster, Asda2TitleId.TheUltimateBooster, Asda2TitleId.LuckyCoinBooster);

        }

        public static void OnGoldPickUp(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Wealthy234, 1000, 3000);
        }

        public static void OnOpenPackage(Character chr, Asda2Item package)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Package, 10, 100);
            chr.UpdateTitleCounter(Asda2TitleId.PackageLover, 100, 200);
            chr.UpdateTitleCounter(Asda2TitleId.PackageHolder, 200, 300);
            chr.UpdateTitleCounter(Asda2TitleId.PackageMaster, 500, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.Packagelegend, 1000, 2000);
            chr.UpdateTitleCounter(Asda2TitleId.PackageKing, 2000, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.PackageEmpor, 1000, 5000);
            chr.CheckTitlesCollection(Asda2TitleId.PackageCrazyNess, Asda2TitleId.Package, Asda2TitleId.PackageLover,
                Asda2TitleId.PackageHolder, Asda2TitleId.PackageMaster, Asda2TitleId.Packagelegend, Asda2TitleId.PackageKing,
                Asda2TitleId.PackageEmpor);


            if (package.ItemId == 31547 || package.ItemId == 31548 || package.ItemId == 31549 || package.ItemId == 31550 || package.ItemId == 31551 || package.ItemId == 31552 || package.ItemId == 31553 || package.ItemId == 31554 || package.ItemId == 31555 || package.ItemId == 31557 || package.ItemId == 31558 || package.ItemId == 31559 || package.ItemId == 31560 || package.ItemId == 31561 || package.ItemId == 31562 || package.ItemId == 31563 || package.ItemId == 31564 || package.ItemId == 31565 || package.ItemId == 31566 || package.ItemId == 31567 || package.ItemId == 31568 || package.ItemId == 31569 || package.ItemId == 31570 || package.ItemId == 31571 || package.ItemId == 31572 || package.ItemId == 31573 || package.ItemId == 31574 || package.ItemId == 31575 || package.ItemId == 31576 || package.ItemId == 31577 || package.ItemId == 31578 || package.ItemId == 31579 || package.ItemId == 31580 || package.ItemId == 31581 || package.ItemId == 31582 || package.ItemId == 31583 || package.ItemId == 31584 || package.ItemId == 31585 || package.ItemId == 31586 || package.ItemId == 31587 || package.ItemId == 31588 || package.ItemId == 31584 || package.ItemId == 31589 || package.ItemId == 31584 || package.ItemId == 31590 || package.ItemId == 31591 || package.ItemId == 31592 || package.ItemId == 31593 || package.ItemId == 31594 || package.ItemId == 31595 || package.ItemId == 31596 || package.ItemId == 31597 || package.ItemId == 31598 || package.ItemId == 31599 || package.ItemId == 31600 || package.ItemId == 31601 || package.ItemId == 31602 || package.ItemId == 31603 || package.ItemId == 31604 || package.ItemId == 31605 || package.ItemId == 31606)
                chr.UpdateTitleCounter(Asda2TitleId.Zodiac235, 1, 7);
            if (package.Template.Quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RarePackage, 100, 500);
            }
            if (package.Template.Quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroPackage, 30, 300);
            }
            if (package.Template.Quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryPackage, 10, 100);
            }
            if (package.Template.Quality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.SpeicalPackage, 5, 20);
            }
            if (package.Template.Quality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UniquePackage, 1, 5);
            }
            if (package.Template.Quality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventPackage, 10, 20);
            }
            if (package.Template.Quality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UltimatePackage , 1, 5);
            }
            chr.CheckTitlesCollection(Asda2TitleId.PackagesCollector, Asda2TitleId.RarePackage, Asda2TitleId.HeroPackage,
                Asda2TitleId.LegendryPackage, Asda2TitleId.SpeicalPackage, Asda2TitleId.UniquePackage,
                Asda2TitleId.EventPackage);

        }

        public static void OnOpenBooster(Character chr, Asda2Item booster, Asda2ItemQuality quality, Asda2ItemQuality itemquality)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Booster, 10, 100);
            chr.UpdateTitleCounter(Asda2TitleId.Boosterlover, 100, 200);
            chr.UpdateTitleCounter(Asda2TitleId.Boosterlover1, 100, 500);
            chr.UpdateTitleCounter(Asda2TitleId.Bboosterlover2, 500, 1000);
            chr.UpdateTitleCounter(Asda2TitleId.Boostermaster, 1000, 2000);
            chr.UpdateTitleCounter(Asda2TitleId.Boosterking, 2000, 3000);
            chr.UpdateTitleCounter(Asda2TitleId.BoosterCrazyNess, 3000, 5000);
            chr.CheckTitlesCollection(Asda2TitleId.BoosterCrazy, Asda2TitleId.Booster, Asda2TitleId.Boosterlover,
                Asda2TitleId.Boosterlover1, Asda2TitleId.Bboosterlover2, Asda2TitleId.Boostermaster,
                Asda2TitleId.Boosterking, Asda2TitleId.BoosterCrazyNess);

            if (booster.ItemId == 56816)
            {
                chr.GainTitle(Asda2TitleId.BlackGuard);
            }
            if (booster.ItemId == 56836 || booster.ItemId == 56828)
            {
                chr.GainTitle(Asda2TitleId.UltimateRing);
            }
            if (booster.ItemId == 56844)
            {
                chr.GainTitle(Asda2TitleId.UltimateNac);
            }
            if (booster.ItemId == 56886 || booster.ItemId == 56887)
            {
                chr.GainTitle(Asda2TitleId.Specialpet);
            }
            if (booster.ItemId == 56888 || booster.ItemId == 56889)
            {
                chr.GainTitle(Asda2TitleId.UniquePet);
            }
            if (quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Herobooster, 50, 500);
            }
            if (quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Legendrybooster, 25, 200);
            }
            if (quality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Specialbooster, 5, 20);
            }
            if (quality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Uniquebooster, 1, 10);
            }
            if (quality == Asda2ItemQuality.Quest)
            {
                chr.UpdateTitleCounter(Asda2TitleId.EventBooster, 100, 500);
            }
            if (quality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UltimateBooster, 1, 5);
            }
            chr.CheckTitlesCollection(Asda2TitleId.BoosterEmpoer, Asda2TitleId.Herobooster, Asda2TitleId.Legendrybooster,
                Asda2TitleId.Specialbooster, Asda2TitleId.Uniquebooster, Asda2TitleId.EventBooster);
            if (itemquality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Herolucky, 50, 200);
            }
            if (itemquality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Legendryluck, 25, 100);
            }
            if (itemquality == Asda2ItemQuality.Spiecal)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Specialluck, 5, 20);
            }
            if (itemquality == Asda2ItemQuality.Unique)
            {
                chr.UpdateTitleCounter(Asda2TitleId.Uniqueluck, 1, 5);
            }
            if (itemquality == Asda2ItemQuality.Machinic)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UltimateLuck, 1, 5);
            }
            chr.CheckTitlesCollection(Asda2TitleId.LuckMaster, Asda2TitleId.Herolucky, Asda2TitleId.Legendryluck,
                Asda2TitleId.Specialluck, Asda2TitleId.Uniqueluck);
        }

        public static void OnSowelFailed(Character chr, Asda2Item sowel)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Misfortune236, 10, 200);
            if (sowel.Template.Quality == Asda2ItemQuality.Purple)
            {
                chr.UpdateTitleCounter(Asda2TitleId.RareCurs, 20, 200);
            }
            if (sowel.Template.Quality == Asda2ItemQuality.Green)
            {
                chr.UpdateTitleCounter(Asda2TitleId.HeroCurs, 10, 100);
            }
            if (sowel.Template.Quality == Asda2ItemQuality.Orange)
            {
                chr.UpdateTitleCounter(Asda2TitleId.LegendryCurs, 5, 50);
            }
            chr.CheckTitlesCollection(Asda2TitleId.Cursed, Asda2TitleId.RareCurs, Asda2TitleId.HeroCurs,
                Asda2TitleId.LegendryCurs);
        }

        public static void OnItemBroken(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Damaged237, 10, 50);

        }

        public static void OnEquipmentChanged(Character chr)
        {
            if (chr == null)
                return;
            var helm = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Head];
            var gloves = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Gloves];
            var shirt = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Shirt];
            var pans = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Pans];
            var boots = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Boots];
            var weapon = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Weapon];

            var items = new List<Asda2Item> { helm, gloves, shirt, pans, boots, weapon };

            if (!items.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Green))
                chr.GainTitle(Asda2TitleId.Superior238);
            if (!items.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Orange))
                chr.GainTitle(Asda2TitleId.Legend260);

            var ahelm = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.AvatarHead];
            var agloves = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.AvatarGloves];
            var ashirt = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.AvatarShirt];
            var apans = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.AvatarGloves];
            var aboots = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.AvatarBoots];
            var wings = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Cape];
            var aweapon = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Wings];
            var accessers = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Accessory];


            var aitems = new List<Asda2Item> { ahelm, agloves, ashirt, apans, aboots };

            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Green))
                chr.GainTitle(Asda2TitleId.Stylish239);
            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Orange))
                chr.GainTitle(Asda2TitleId.StylishMaster);
            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Spiecal))
                chr.GainTitle(Asda2TitleId.SpeicalStylesh);
            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Unique))
                chr.GainTitle(Asda2TitleId.TheBestStyler);
            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Machinic))
                chr.GainTitle(Asda2TitleId.GreatStylish);
            if (!aitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Quest))
                chr.GainTitle(Asda2TitleId.EventStylish);

            var avitems = new List<Asda2Item> { ahelm, agloves, ashirt, apans, aboots, wings, aweapon, accessers };
            if (!avitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Unique))
                chr.GainTitle(Asda2TitleId.UltimateStylish);
            var aviitems = new List<Asda2Item> { ahelm, agloves, ashirt, apans, aboots, wings, accessers };
            if (!aviitems.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Machinic))
                chr.GainTitle(Asda2TitleId.DarknessStylish);
            chr.CheckTitlesCollection(Asda2TitleId.StlishKing, Asda2TitleId.Stylish239, Asda2TitleId.StylishMaster, Asda2TitleId.SpeicalStylesh, Asda2TitleId.TheBestStyler, Asda2TitleId.GreatStylish,
                Asda2TitleId.EventStylish, Asda2TitleId.UltimateStylish, Asda2TitleId.DarknessStylish);
            var leftring = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.LeftRing];
            var nakeles = chr.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Nackles];

            var rings = new List<Asda2Item> { leftring, nakeles };
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.White))
                chr.GainTitle(Asda2TitleId.Ring);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Yello))
                chr.GainTitle(Asda2TitleId.EnchRing);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Purple))
                chr.GainTitle(Asda2TitleId.RareRing);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Green))
                chr.GainTitle(Asda2TitleId.HeroRing);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Orange))
                chr.GainTitle(Asda2TitleId.LegendryRing);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Spiecal))
                chr.GainTitle(Asda2TitleId.LordOfTheRing);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Unique))
                chr.GainTitle(Asda2TitleId.RingEmporer);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Quest))
                chr.GainTitle(Asda2TitleId.eventring);
            if (!rings.Any(asda2Item => asda2Item == null || asda2Item.Template.Quality != Asda2ItemQuality.Machinic))
                chr.GainTitle(Asda2TitleId.UltimateRings);
            chr.CheckTitlesCollection(Asda2TitleId.RingsCollecter, Asda2TitleId.Ring, Asda2TitleId.EnchRing,
                Asda2TitleId.RareRing , Asda2TitleId.LegendryRing, Asda2TitleId.LordOfTheRing, Asda2TitleId.RingEmporer
                , Asda2TitleId.eventring, Asda2TitleId.UltimateRings);

        }

        public static void OnEnachantFail(Character chr)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Cursed258, 100, 500);


        }

        public static void OnItemBrokenByUpdate(Character chr, byte enchant, Asda2ItemQuality qaulity)
        {
            chr.UpdateTitleCounter(Asda2TitleId.Broken259, 10, 50);
            if (enchant == 15)
            {
                chr.UpdateTitleCounter(Asda2TitleId.UpgradeCrusher, 10, 30);
                if (qaulity == Asda2ItemQuality.Green)
                {
                    chr.UpdateTitleCounter(Asda2TitleId.CrazyHero, 1, 15);
                }
                if (qaulity == Asda2ItemQuality.Orange)
                {
                    chr.UpdateTitleCounter(Asda2TitleId.CrazyLegendry, 1, 10);
                }

            }
            if (enchant >= 20)
            {
                chr.UpdateTitleCounter(Asda2TitleId.TheStupid, 1, 20);
                if (qaulity == Asda2ItemQuality.Green)
                {
                    chr.UpdateTitleCounter(Asda2TitleId.Moron, 1, 1);
                    chr.UpdateTitleCounter(Asda2TitleId.BigMoron, 1, 10);
                }
                if (qaulity == Asda2ItemQuality.Orange)
                {
                    chr.UpdateTitleCounter(Asda2TitleId.Stupid, 1, 1);
                    chr.UpdateTitleCounter(Asda2TitleId.BigStupid, 1, 5);
                }
                chr.CheckTitlesCollection(Asda2TitleId.TheMostStupid, Asda2TitleId.TheStupid, Asda2TitleId.Moron, Asda2TitleId.BigMoron,
                    Asda2TitleId.Stupid, Asda2TitleId.BigStupid);
                chr.CheckTitlesCollection(Asda2TitleId.UpgradeDestroyer, Asda2TitleId.TheMostStupid,
                    Asda2TitleId.UpgradeCrusher, Asda2TitleId.CrazyHero, Asda2TitleId.CrazyLegendry);

            }
        }
    }
}