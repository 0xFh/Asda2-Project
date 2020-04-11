using System;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Mail;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    public class AchievementReward : IDataHolder
    {
        public uint AchievementEntryId;
        public GenderType Gender;
        public TitleId AllianceTitle;
        public TitleId HordeTitle;
        public Asda2ItemId Item;
        public NPCId Sender;
        [Persistent(8)] public string[] Subjects;
        [Persistent(8)] public string[] Bodies;

        /// <summary>Subject of the mail.</summary>
        [NotPersistent]
        public string DefaultSubject
        {
            get
            {
                if (this.Subjects == null)
                    return "[unknown]";
                return this.Subjects.LocalizeWithDefaultLocale();
            }
        }

        /// <summary>Body of the mail.</summary>
        [NotPersistent]
        public string DefaultBody
        {
            get
            {
                if (this.Bodies == null)
                    return "[unknown]";
                return this.Bodies.LocalizeWithDefaultLocale();
            }
        }

        public void GiveReward(Character character)
        {
            if (character.Gender != this.Gender && this.Gender != GenderType.Neutral)
                return;
            if (character.FactionGroup == FactionGroup.Alliance && this.AllianceTitle != TitleId.None)
                character.SetTitle(this.AllianceTitle, false);
            else if (character.FactionGroup == FactionGroup.Horde && this.HordeTitle != TitleId.None)
                character.SetTitle(this.HordeTitle, false);
            if (this.Item == (Asda2ItemId) 0)
                return;
            MailMessage letter =
                new MailMessage(this.Subjects.Localize(character.Locale), this.Bodies.Localize(character.Locale))
                {
                    ReceiverId = character.EntityId.Low,
                    DeliveryTime = DateTime.Now,
                    SendTime = DateTime.Now,
                    ExpireTime = DateTime.Now.AddMonths(1),
                    MessageStationary = MailStationary.Normal
                };
            letter.AddItem(this.Item);
            MailMgr.SendMail(letter);
        }

        public void FinalizeDataHolder()
        {
            AchievementEntry achievementEntry = AchievementMgr.AchievementEntries[this.AchievementEntryId];
            if (achievementEntry == null)
                ContentMgr.OnInvalidDBData("{0} had an invalid AchievementEntryId.", (object) this);
            else
                achievementEntry.Rewards.Add(this);
        }
    }
}