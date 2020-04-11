﻿using WCell.Constants.Items;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.NPCs
{
    public struct PetitionerEntry
    {
        public static PetitionerEntry GuildPetitionEntry = new PetitionerEntry()
        {
            Index = 1,
            DisplayId = 16161,
            Cost = GuildMgr.GuildCharterCost,
            RequiredSignatures = GuildMgr.RequiredCharterSignature
        };

        public static PetitionerEntry ArenaPetition2v2Entry = new PetitionerEntry()
        {
            Index = 1,
            DisplayId = 16161,
            Cost = ArenaMgr.ArenaTeamCharter2v2Cost,
            RequiredSignatures = ArenaMgr.RequiredCharter2v2Signature
        };

        public static PetitionerEntry ArenaPetition3v3Entry = new PetitionerEntry()
        {
            Index = 2,
            DisplayId = 16161,
            Cost = ArenaMgr.ArenaTeamCharter3v3Cost,
            RequiredSignatures = ArenaMgr.RequiredCharter3v3Signature
        };

        public static PetitionerEntry ArenaPetition5v5Entry = new PetitionerEntry()
        {
            Index = 3,
            DisplayId = 16161,
            Cost = ArenaMgr.ArenaTeamCharter5v5Cost,
            RequiredSignatures = ArenaMgr.RequiredCharter5v5Signature
        };

        public uint Index;
        public Asda2ItemId ItemId;
        public uint DisplayId;
        public uint Cost;
        public int RequiredSignatures;
    }
}