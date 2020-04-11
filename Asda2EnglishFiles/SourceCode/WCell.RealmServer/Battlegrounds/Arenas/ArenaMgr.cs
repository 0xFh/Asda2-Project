using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Constants.ArenaTeams;
using WCell.RealmServer.NPCs;
using WCell.Util.Collections;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    public static class ArenaMgr
    {
        public static int MaxArenaTeamNameLength = 24;
        private static uint arenateamCharter2v2Cost = 800000;
        private static uint arenateamCharter3v3Cost = 1200000;
        private static uint arenateamCharter5v5Cost = 2000000;
        private static int requiredCharter2v2Signature = 2;
        private static int requiredCharter3v3Signature = 3;
        private static int requiredCharter5v5Signature = 5;

        /// <summary>
        /// Maps char-id to the corresponding ArenaTeamMember object so it can be looked up when char reconnects
        /// </summary>
        public static readonly IDictionary<uint, ArenaTeamMember> OfflineChars =
            (IDictionary<uint, ArenaTeamMember>) new SynchronizedDictionary<uint, ArenaTeamMember>();

        public static readonly IDictionary<uint, ArenaTeam> ArenaTeamsById =
            (IDictionary<uint, ArenaTeam>) new SynchronizedDictionary<uint, ArenaTeam>();

        public static readonly IDictionary<string, ArenaTeam> ArenaTeamsByName =
            (IDictionary<string, ArenaTeam>) new SynchronizedDictionary<string, ArenaTeam>(
                (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        public static uint ArenaTeamCharter2v2Cost
        {
            get { return ArenaMgr.arenateamCharter2v2Cost; }
            set
            {
                ArenaMgr.arenateamCharter2v2Cost = value;
                PetitionerEntry.ArenaPetition2v2Entry.Cost = value;
            }
        }

        public static uint ArenaTeamCharter3v3Cost
        {
            get { return ArenaMgr.arenateamCharter3v3Cost; }
            set
            {
                ArenaMgr.arenateamCharter3v3Cost = value;
                PetitionerEntry.ArenaPetition3v3Entry.Cost = value;
            }
        }

        public static uint ArenaTeamCharter5v5Cost
        {
            get { return ArenaMgr.arenateamCharter5v5Cost; }
            set
            {
                ArenaMgr.arenateamCharter5v5Cost = value;
                PetitionerEntry.ArenaPetition5v5Entry.Cost = value;
            }
        }

        public static int RequiredCharter2v2Signature
        {
            get { return ArenaMgr.requiredCharter2v2Signature; }
            set
            {
                ArenaMgr.requiredCharter2v2Signature = value;
                PetitionerEntry.ArenaPetition2v2Entry.RequiredSignatures = value;
            }
        }

        public static int RequiredCharter3v3Signature
        {
            get { return ArenaMgr.requiredCharter3v3Signature; }
            set
            {
                ArenaMgr.requiredCharter3v3Signature = value;
                PetitionerEntry.ArenaPetition3v3Entry.RequiredSignatures = value;
            }
        }

        public static int RequiredCharter5v5Signature
        {
            get { return ArenaMgr.requiredCharter5v5Signature; }
            set
            {
                ArenaMgr.requiredCharter5v5Signature = value;
                PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures = value;
            }
        }

        public static void Initialize()
        {
            ArenaMgr.LoadFromDB();
        }

        public static void LoadFromDB()
        {
            ArenaTeam[] all = ActiveRecordBase<ArenaTeam>.FindAll();
            if (all == null)
                return;
            foreach (ArenaTeam arenaTeam in all)
                arenaTeam.InitAfterLoad();
        }

        /// <summary>New or loaded arena team</summary>
        /// <param name="guild"></param>
        public static void RegisterArenaTeam(ArenaTeam team)
        {
            ArenaMgr.ArenaTeamsById.Add(team.Id, team);
            ArenaMgr.ArenaTeamsByName.Add(team.Name, team);
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) team.Members.Values)
            {
                if (arenaTeamMember.Character == null && !ArenaMgr.OfflineChars.ContainsKey(arenaTeamMember.Id))
                    ArenaMgr.OfflineChars.Add(arenaTeamMember.Id, arenaTeamMember);
            }
        }

        public static void UnregisterArenaTeam(ArenaTeam team)
        {
            ArenaMgr.ArenaTeamsById.Remove(team.Id);
            ArenaMgr.ArenaTeamsByName.Remove(team.Name);
        }

        public static void RegisterArenaTeamMember(ArenaTeamMember atm)
        {
            if (atm.Character != null)
                return;
            ArenaMgr.OfflineChars.Add(atm.Id, atm);
        }

        public static void UnregisterArenaTeamMember(ArenaTeamMember atm)
        {
            if (!ArenaMgr.OfflineChars.ContainsKey(atm.Id))
                return;
            ArenaMgr.OfflineChars.Remove(atm.Id);
        }

        public static ArenaTeam GetArenaTeam(uint teamId)
        {
            ArenaTeam arenaTeam;
            ArenaMgr.ArenaTeamsById.TryGetValue(teamId, out arenaTeam);
            return arenaTeam;
        }

        public static ArenaTeam GetArenaTeam(string name)
        {
            ArenaTeam arenaTeam;
            ArenaMgr.ArenaTeamsByName.TryGetValue(name, out arenaTeam);
            return arenaTeam;
        }

        public static ArenaTeamSlot GetSlotByType(uint type)
        {
            switch (type)
            {
                case 2:
                    return ArenaTeamSlot.TWO_VS_TWO;
                case 3:
                    return ArenaTeamSlot.THREE_VS_THREE;
                case 5:
                    return ArenaTeamSlot.FIVE_VS_FIVE;
                default:
                    throw new Exception("Invalid Type of arena team: " + (object) type);
            }
        }

        public static bool CanUseName(string name)
        {
            if (ArenaMgr.IsValidArenaTeamName(name))
                return ArenaMgr.GetArenaTeam(name) == null;
            return false;
        }

        public static bool DoesArenaTeamExist(string name)
        {
            return ArenaMgr.GetArenaTeam(name) != null;
        }

        public static bool IsValidArenaTeamName(string name)
        {
            name = name.Trim();
            return name.Length >= 3 || name.Length <= ArenaMgr.MaxArenaTeamNameLength;
        }
    }
}