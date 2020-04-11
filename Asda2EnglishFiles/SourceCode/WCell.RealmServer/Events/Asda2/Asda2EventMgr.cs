using System;
using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Events.Asda2
{
    public static class Asda2EventMgr
    {
        [NotVariable] public static bool IsGuessWordEventStarted = false;

        internal static Dictionary<MapId, DeffenceTownEvent> DefenceTownEvents =
            new Dictionary<MapId, DeffenceTownEvent>();

        private static float _percision;
        private static string _word;

        /// <summary>Начинает эвент угадай слово</summary>
        /// <param name="word">секретное слово</param>
        /// <param name="precison">точность от 50 до 100 в процентах</param>
        public static void StartGueesWordEvent(string word, int precison, string gmName)
        {
            if (Asda2EventMgr.IsGuessWordEventStarted || word == null)
                return;
            Asda2EventMgr.SendMessageToWorld(
                "Guess word event started. {0} is event manager. Type your answer to global chat.", (object) gmName);
            Asda2EventMgr.IsGuessWordEventStarted = true;
            Asda2EventMgr._percision = 100f / (float) precison;
            Asda2EventMgr._word = word.ToLower();
        }

        public static void StopGueesWordEvent()
        {
            Asda2EventMgr.SendMessageToWorld("Guess word event ended.");
            Asda2EventMgr.IsGuessWordEventStarted = false;
        }

        public static void TryGuess(string word, Character senderChr)
        {
            lock (typeof(Asda2EventMgr))
            {
                if (!Asda2EventMgr.IsGuessWordEventStarted)
                    return;
                string lower = word.ToLower();
                float num = 0.0f;
                for (int index = 0; index < lower.Length && index < Asda2EventMgr._word.Length; ++index)
                {
                    if ((int) lower[index] == (int) Asda2EventMgr._word[index])
                        ++num;
                }

                if ((double) num / (double) Asda2EventMgr._word.Length < (double) Asda2EventMgr._percision)
                    return;
                int experience = CharacterFormulas.CalcExpForGuessWordEvent(senderChr.Level);
                int eventItems = CharacterFormulas.EventItemsForGuessEvent;
                Asda2EventMgr.SendMessageToWorld("{0} is winner. Prize is {1} exp and {2} event items.",
                    (object) senderChr.Name, (object) experience, (object) eventItems);
                senderChr.GainXp(experience, "guess_event", false);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                    senderChr.Asda2Inventory.AddDonateItem(Asda2ItemMgr.GetTemplate(CharacterFormulas.EventItemId),
                        eventItems, "guess_event", false)));
                Asda2EventMgr.StopGueesWordEvent();
                Log.Create(Log.Types.EventOperations, LogSourceType.Character, senderChr.EntryId)
                    .AddAttribute("win", (double) eventItems, "guess_event").Write();
            }
        }

        public static void SendMessageToWorld(string message, params object[] p)
        {
            WCell.RealmServer.Global.World.BroadcastMsg("Event Manager", string.Format(message, p), Color.LightPink);
        }

        public static bool StartDeffenceTownEvent(Map map, int minLevel, int maxLevel, float amountMod, float healthMod,
            float otherStatsMod, float speedMod, float difficulty)
        {
            if (map.Id != MapId.Alpia)
                return false;
            if (Asda2EventMgr.DefenceTownEvents.ContainsKey(map.Id))
            {
                Asda2EventMgr.DefenceTownEvents[map.Id].Stop(false);
                Asda2EventMgr.DefenceTownEvents.Remove(map.Id);
            }

            DefenceTownEventAplia defenceTownEventAplia = new DefenceTownEventAplia(map, minLevel, maxLevel, amountMod,
                healthMod, otherStatsMod, speedMod, difficulty);
            Asda2EventMgr.DefenceTownEvents.Add(map.Id, (DeffenceTownEvent) defenceTownEventAplia);
            defenceTownEventAplia.Start();
            return true;
        }

        public static bool StopDeffenceTownEvent(Map map, bool success)
        {
            if (map.Id != MapId.Alpia)
                return false;
            if (Asda2EventMgr.DefenceTownEvents.ContainsKey(map.Id))
            {
                Asda2EventMgr.DefenceTownEvents[map.Id].Stop(success);
                Asda2EventMgr.DefenceTownEvents.Remove(map.Id);
            }

            return true;
        }
    }
}