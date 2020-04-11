using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Talents
{
    public class PlayerTalentCollection : TalentCollection
    {
        /// <summary>
        /// Every month, the reset tier drops one (but half a month is more than long enough)
        /// </summary>
        public static int PlayerResetTierDecayHours = 360;

        public PlayerTalentCollection(Character owner)
            : base((Unit) owner)
        {
        }

        public override Character OwnerCharacter
        {
            get { return (Character) this.Owner; }
        }

        public override int FreeTalentPoints
        {
            get { return this.OwnerCharacter.FreeTalentPoints; }
            set { this.OwnerCharacter.FreeTalentPoints = value; }
        }

        public override int SpecProfileCount
        {
            get { return this.OwnerCharacter.SpecProfiles.Length; }
            internal set { throw new NotImplementedException("TODO: Create/delete SpecProfiles?"); }
        }

        public override int CurrentSpecIndex
        {
            get { return this.OwnerCharacter.Record.CurrentSpecIndex; }
        }

        public override uint[] ResetPricesPerTier
        {
            get { return TalentMgr.PlayerTalentResetPricesPerTier; }
        }

        protected override int CurrentResetTier
        {
            get { return this.OwnerCharacter.Record.TalentResetPriceTier; }
            set { this.OwnerCharacter.Record.TalentResetPriceTier = value; }
        }

        public override DateTime? LastResetTime
        {
            get { return this.OwnerCharacter.Record.LastTalentResetTime; }
            set { this.OwnerCharacter.Record.LastTalentResetTime = value; }
        }

        public override int ResetTierDecayHours
        {
            get { return PlayerTalentCollection.PlayerResetTierDecayHours; }
        }

        public override int GetFreeTalentPointsForLevel(int level)
        {
            if (level < 10)
                return -this.TotalPointsSpent;
            return level - 9 - this.TotalPointsSpent;
        }

        public override void UpdateFreeTalentPointsSilently(int delta)
        {
            this.OwnerCharacter.SetInt32((UpdateFieldId) PlayerFields.CHARACTER_POINTS1,
                this.OwnerCharacter.FreeTalentPoints + delta);
        }

        public void ChangeTalentGroup(int talentGroupNo)
        {
        }
    }
}