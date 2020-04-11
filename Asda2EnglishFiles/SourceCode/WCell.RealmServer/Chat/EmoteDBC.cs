using WCell.Constants.Misc;
using WCell.Core.DBC;
using WCell.Util.Data;

namespace WCell.RealmServer.Chat
{
    public static class EmoteDBC
    {
        [NotPersistent] public static MappedDBCReader<EmoteType, EmoteDBC.EmoteRelationConverter> EmoteRelationReader;

        public static void LoadEmotes()
        {
            EmoteDBC.EmoteRelationReader =
                new MappedDBCReader<EmoteType, EmoteDBC.EmoteRelationConverter>(
                    RealmServerConfiguration.GetDBCFile("EmotesText.dbc"));
        }

        /// <summary>Emote relation holder, searches via TextEmote</summary>
        public class EmoteRelationConverter : AdvancedDBCRecordConverter<EmoteType>
        {
            public override EmoteType ConvertTo(byte[] rawData, ref int id)
            {
                id = DBCRecordConverter.GetInt32(rawData, 0);
                return (EmoteType) DBCRecordConverter.GetUInt32(rawData, 2);
            }
        }
    }
}