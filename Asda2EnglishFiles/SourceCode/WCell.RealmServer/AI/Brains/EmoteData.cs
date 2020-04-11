namespace WCell.RealmServer.AI.Brains
{
    public struct EmoteData
    {
        public string mText;
        public NPCBrainEvents mEvent;
        public NPCEmoteType mType;
        public uint mSoundId;

        public EmoteData(string pText, NPCBrainEvents pEvent, NPCEmoteType pType, uint pSoundId)
        {
            this.mText = pText;
            this.mEvent = pEvent;
            this.mType = pType;
            this.mSoundId = pSoundId;
        }
    }
}