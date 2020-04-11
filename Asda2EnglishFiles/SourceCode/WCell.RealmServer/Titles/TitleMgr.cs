using System.Collections.Generic;
using WCell.Constants.Misc;
using WCell.Core.DBC;

namespace WCell.RealmServer.Titles
{
    public static class TitleMgr
    {
        public static readonly Dictionary<TitleId, CharacterTitleEntry> CharacterTitleEntries =
            new Dictionary<TitleId, CharacterTitleEntry>();

        public static void InitTitles()
        {
            DBCReader<TitleConverter> dbcReader =
                new DBCReader<TitleConverter>(RealmServerConfiguration.GetDBCFile("CharTitles.dbc"));
        }

        public static CharacterTitleEntry GetTitleEntry(TitleId titleId)
        {
            return TitleMgr.CharacterTitleEntries[titleId];
        }

        public static CharacterTitleEntry GetTitleEntry(TitleBitId titleBitId)
        {
            foreach (CharacterTitleEntry characterTitleEntry in TitleMgr.CharacterTitleEntries.Values)
            {
                if (characterTitleEntry.BitIndex == titleBitId)
                    return characterTitleEntry;
            }

            return (CharacterTitleEntry) null;
        }
    }
}