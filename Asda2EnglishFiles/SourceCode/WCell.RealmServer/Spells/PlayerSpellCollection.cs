using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
    public class PlayerSpellCollection : SpellCollection
    {
        private static readonly ObjectPool<PlayerSpellCollection> PlayerSpellCollectionPool =
            new ObjectPool<PlayerSpellCollection>((Func<PlayerSpellCollection>) (() => new PlayerSpellCollection()));

        /// <summary>Whether to send Update Packets</summary>
        protected bool m_sendPackets;

        /// <summary>
        /// All current Spell-cooldowns.
        /// Each SpellId has an expiry time associated with it
        /// </summary>
        protected List<ISpellIdCooldown> m_idCooldowns;

        /// <summary>
        /// All current category-cooldowns.
        /// Each category has an expiry time associated with it
        /// </summary>
        protected List<ISpellCategoryCooldown> m_categoryCooldowns;

        /// <summary>The runes of this Player (if any)</summary>
        private RuneSet m_runes;

        public static PlayerSpellCollection Obtain(Character chr)
        {
            PlayerSpellCollection playerSpellCollection = PlayerSpellCollection.PlayerSpellCollectionPool.Obtain();
            playerSpellCollection.Initialize((Unit) chr);
            if (playerSpellCollection.Runes != null)
                playerSpellCollection.Runes.InitRunes(chr);
            return playerSpellCollection;
        }

        private PlayerSpellCollection()
        {
            this.m_idCooldowns = new List<ISpellIdCooldown>(5);
            this.m_categoryCooldowns = new List<ISpellCategoryCooldown>(5);
        }

        protected override void Initialize(Unit owner)
        {
            base.Initialize(owner);
            Character owner1 = (Character) owner;
            this.m_sendPackets = false;
            if (owner.Class != ClassId.Balista)
                return;
            this.m_runes = new RuneSet(owner1);
        }

        protected internal override void Recycle()
        {
            base.Recycle();
            this.m_idCooldowns.Clear();
            this.m_categoryCooldowns.Clear();
            if (this.m_runes != null)
            {
                this.m_runes.Dispose();
                this.m_runes = (RuneSet) null;
            }

            PlayerSpellCollection.PlayerSpellCollectionPool.Recycle(this);
        }

        public IEnumerable<ISpellIdCooldown> IdCooldowns
        {
            get { return (IEnumerable<ISpellIdCooldown>) this.m_idCooldowns; }
        }

        public IEnumerable<ISpellCategoryCooldown> CategoryCooldowns
        {
            get { return (IEnumerable<ISpellCategoryCooldown>) this.m_categoryCooldowns; }
        }

        public int IdCooldownCount
        {
            get { return this.m_idCooldowns.Count; }
        }

        public int CategoryCooldownCount
        {
            get { return this.m_categoryCooldowns.Count; }
        }

        /// <summary>Owner as Character</summary>
        public Character OwnerChar
        {
            get { return (Character) this.Owner; }
        }

        /// <summary>The set of runes of this Character (if any)</summary>
        public RuneSet Runes
        {
            get { return this.m_runes; }
        }

        public void AddNew(Spell spell)
        {
            this.AddSpell(spell, false);
        }

        public override void AddSpell(Spell spell)
        {
            this.AddSpell(spell, true);
        }

        /// <summary>
        /// Adds the spell without doing any further checks nor adding any spell-related skills or showing animations (after load)
        /// </summary>
        internal void OnlyAdd(SpellRecord record)
        {
            SpellId spellId = record.SpellId;
            Spell spell = SpellHandler.Get(spellId);
            this.m_byId[spellId] = spell;
        }

        /// <summary>
        /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
        /// </summary>
        private void AddSpell(Spell spell, bool sendPacket)
        {
            if (spell.Ability != null)
            {
                Skill skill = this.OwnerChar.Skills[spell.Ability.Skill.Id] ??
                              this.OwnerChar.Skills.Add(spell.Ability.Skill, true);
                if (skill.CurrentTierSpell == null || skill.CurrentTierSpell.SkillTier < spell.SkillTier)
                    skill.CurrentTierSpell = spell;
            }

            if (!this.m_byId.ContainsKey(spell.SpellId))
            {
                Character ownerChar = this.OwnerChar;
                int specIndex = this.GetSpecIndex(spell);
                List<SpellRecord> spellList = this.GetSpellList(spell);
                SpellRecord record = new SpellRecord(spell.SpellId, ownerChar.EntityId.Low, specIndex);
                record.SaveLater();
                spellList.Add(record);
                base.AddSpell(spell);
            }

            if (!this.m_sendPackets || !sendPacket)
                return;
            if (spell.Level == 1)
                Asda2SpellHandler.SendSkillLearnedFirstTimeResponse(this.OwnerChar.Client, spell.RealId,
                    spell.CooldownTime);
            Asda2SpellHandler.SendSkillLearnedResponse(SkillLearnStatus.Ok, this.OwnerChar, (uint) spell.RealId,
                spell.Level);
        }

        /// <summary>Replaces or (if newSpell == null) removes oldSpell.</summary>
        public override bool Replace(Spell oldSpell, Spell newSpell)
        {
            bool flag = oldSpell != null && this.m_byId.Remove(oldSpell.SpellId);
            if (flag)
            {
                this.OnRemove(oldSpell);
                if (newSpell == null)
                {
                    int num = this.m_sendPackets ? 1 : 0;
                    return true;
                }
            }

            if (newSpell != null)
            {
                if (this.m_sendPackets)
                {
                    int num = flag ? 1 : 0;
                }

                this.AddSpell(newSpell, true);
            }

            return flag;
        }

        /// <summary>Enqueues a new task to remove that spell from DB</summary>
        private void OnRemove(Spell spell)
        {
            Character ownerChar = this.OwnerChar;
            if (spell.RepresentsSkillTier)
                ownerChar.Skills.Remove(spell.Ability.Skill.Id);
            if (spell.IsAura)
                ownerChar.Auras.Remove(spell);
            List<SpellRecord> spellList = this.GetSpellList(spell);
            for (int index = 0; index < spellList.Count; ++index)
            {
                SpellRecord spellRecord = spellList[index];
                if (spellRecord.SpellId == spell.SpellId)
                {
                    ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                        (IMessage) new Message(new Action(((ActiveRecordBase) spellRecord).Delete)));
                    spellList.RemoveAt(index);
                    break;
                }
            }
        }

        private int GetSpecIndex(Spell spell)
        {
            Character ownerChar = this.OwnerChar;
            if (!spell.IsTalent)
                return -1;
            return ownerChar.Talents.CurrentSpecIndex;
        }

        private List<SpellRecord> GetSpellList(Spell spell)
        {
            Character ownerChar = this.OwnerChar;
            if (spell.IsTalent)
                return ownerChar.CurrentSpecProfile.TalentSpells;
            return ownerChar.Record.AbilitySpells;
        }

        public override void Clear()
        {
            foreach (Spell spell in this.m_byId.Values.ToArray<Spell>())
                this.OnRemove(spell);
            base.Clear();
        }

        internal void PlayerInitialize()
        {
            Character ownerChar = this.OwnerChar;
            foreach (Spell spell in this.m_byId.Values)
            {
                if (spell.Talent != null)
                    ownerChar.Talents.AddExisting(spell.Talent, spell.Rank);
                else if (spell.IsPassive && !spell.HasHarmfulEffects)
                {
                    int num = (int) ownerChar.SpellCast.Start(spell, true, (WorldObject) this.Owner);
                }
            }

            foreach (Talent talent in ownerChar.Talents)
            {
                Spell spell = talent.Spell;
                if (spell.IsPassive)
                {
                    int num = (int) ownerChar.SpellCast.Start(spell, true, (WorldObject) this.Owner);
                }
            }

            this.m_sendPackets = true;
        }

        public override void AddDefaultSpells()
        {
            this.AddNew(SpellHandler.Get(SpellId.BashRank1));
            this.AddNew(SpellHandler.Get(SpellId.ArrowStrikeRank1));
        }

        /// <summary>Add everything to the caster that this spell requires</summary>
        public void AddSpellRequirements(Spell spell)
        {
        }

        /// <summary>
        /// Returns true if spell is currently cooling down.
        /// Removes expired cooldowns of that spell.
        /// </summary>
        public override bool IsReady(Spell spell)
        {
            if (spell.CooldownTime > 0)
            {
                for (int index = 0; index < this.m_idCooldowns.Count; ++index)
                {
                    ISpellIdCooldown idCooldown = this.m_idCooldowns[index];
                    if ((int) idCooldown.SpellId == (int) spell.Id)
                    {
                        if (idCooldown.Until > DateTime.Now)
                            return false;
                        this.m_idCooldowns.RemoveAt(index);
                        break;
                    }
                }
            }

            return true;
        }

        public override void AddCooldown(Spell spell, Item casterItem)
        {
            bool flag = casterItem != null && casterItem.Template.UseSpell != null;
            int millis = 0;
            if (flag)
                millis = casterItem.Template.UseSpell.Cooldown;
            if (millis == 0)
                millis = spell.GetCooldown(this.Owner);
            if (millis <= 0)
                return;
            SpellIdCooldown spellIdCooldown = new SpellIdCooldown()
            {
                SpellId = spell.Id,
                Until = DateTime.Now + TimeSpan.FromMilliseconds((double) millis)
            };
            if (flag)
                spellIdCooldown.ItemId = casterItem.Template.Id;
            this.m_idCooldowns.Add((ISpellIdCooldown) spellIdCooldown);
            this.OwnerChar.Map.CallDelayed(500,
                (Action) (() => Asda2SpellHandler.SendSetSkillCooldownResponse(this.OwnerChar, spell)));
            this.OwnerChar.Map.CallDelayed(millis,
                (Action) (() => Asda2SpellHandler.SendClearCoolDown(this.OwnerChar, spell.RealId)));
        }

        /// <summary>Clears all pending spell cooldowns.</summary>
        public override void ClearCooldowns()
        {
            foreach (ISpellIdCooldown idCooldown in this.m_idCooldowns)
                Asda2SpellHandler.SendClearCoolDown(this.OwnerChar, (SpellId) idCooldown.SpellId);
            foreach (Spell spell in this.m_byId.Values)
            {
                foreach (ISpellCategoryCooldown categoryCooldown in this.m_categoryCooldowns)
                {
                    if ((int) spell.Category == (int) categoryCooldown.CategoryId)
                    {
                        Asda2SpellHandler.SendClearCoolDown(this.OwnerChar, spell.SpellId);
                        break;
                    }
                }
            }

            ISpellIdCooldown[] cds = this.m_idCooldowns.ToArray();
            ISpellCategoryCooldown[] catCds = this.m_categoryCooldowns.ToArray();
            this.m_idCooldowns.Clear();
            this.m_categoryCooldowns.Clear();
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
            {
                foreach (ISpellIdCooldown spellIdCooldown in cds)
                {
                    if (spellIdCooldown is ActiveRecordBase)
                        ((ActiveRecordBase) spellIdCooldown).Delete();
                }

                foreach (ISpellCategoryCooldown categoryCooldown in catCds)
                {
                    if (categoryCooldown is ActiveRecordBase)
                        ((ActiveRecordBase) categoryCooldown).Delete();
                }
            })));
        }

        /// <summary>Clears the cooldown for this spell</summary>
        public override void ClearCooldown(Spell cooldownSpell, bool alsoCategory = true)
        {
            Character ownerChar = this.OwnerChar;
            Asda2SpellHandler.SendClearCoolDown(ownerChar, cooldownSpell.SpellId);
            if (alsoCategory && cooldownSpell.Category != 0U)
            {
                foreach (Spell spell in this.m_byId.Values)
                {
                    if ((int) spell.Category == (int) cooldownSpell.Category)
                        Asda2SpellHandler.SendClearCoolDown(ownerChar, spell.SpellId);
                }
            }

            ISpellIdCooldown idCooldown =
                this.m_idCooldowns.RemoveFirst<ISpellIdCooldown>(
                    (Func<ISpellIdCooldown, bool>) (cd => (int) cd.SpellId == (int) cooldownSpell.Id));
            ISpellCategoryCooldown catCooldown = this.m_categoryCooldowns.RemoveFirst<ISpellCategoryCooldown>(
                (Func<ISpellCategoryCooldown, bool>) (cd => (int) cd.CategoryId == (int) cooldownSpell.Category));
            if (!(idCooldown is ActiveRecordBase) && !(catCooldown is ActiveRecordBase))
                return;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
            {
                if (idCooldown is ActiveRecordBase)
                    ((ActiveRecordBase) idCooldown).Delete();
                if (!(catCooldown is ActiveRecordBase))
                    return;
                ((ActiveRecordBase) catCooldown).Delete();
            })));
        }

        private void SaveCooldowns()
        {
            this.SaveCooldowns<ISpellIdCooldown>(this.m_idCooldowns);
        }

        private void SaveCooldowns<T>(List<T> cooldowns) where T : ICooldown
        {
            for (int index = cooldowns.Count - 1; index >= 0; --index)
            {
                ICooldown cooldown = (ICooldown) cooldowns[index];
                if (cooldown.Until < DateTime.Now.AddMilliseconds((double) SpellHandler.MinCooldownSaveTimeMillis))
                {
                    if (cooldown is ActiveRecordBase)
                        ((ActiveRecordBase) cooldown).DeleteLater();
                }
                else
                {
                    IConsistentCooldown consistentCooldown = cooldown.AsConsistent();
                    consistentCooldown.CharId = this.Owner.EntityId.Low;
                    consistentCooldown.Save();
                    cooldowns.Add((T) consistentCooldown);
                }

                cooldowns.RemoveAt(index);
            }
        }

        internal void OnSave()
        {
            try
            {
                this.SaveCooldowns();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    string.Format("Failed to save cooldowns for {0}.",
                        this.OwnerChar == null ? (object) "Not character" : (object) this.OwnerChar.Name),
                    new object[0]);
            }
        }

        internal void LoadSpellsAndTalents()
        {
            Character ownerChar = this.OwnerChar;
            CharacterRecord record = ownerChar.Record;
            SpellRecord[] spellRecordArray = SpellRecord.LoadAllRecordsFor(ownerChar.EntityId.Low);
            SpecProfile[] specProfiles = ownerChar.SpecProfiles;
            foreach (SpellRecord spellRecord in spellRecordArray)
            {
                Spell spell = spellRecord.Spell;
                if (spell == null)
                    LogManager.GetCurrentClassLogger().Warn("Character \"{0}\" had invalid spell: {1} ({2})",
                        (object) this, (object) spellRecord.SpellId, (object) spellRecord.SpellId);
                else if (spell.IsTalent)
                {
                    if (spellRecord.SpecIndex < 0 || spellRecord.SpecIndex >= specProfiles.Length)
                        LogManager.GetCurrentClassLogger().Warn(
                            "Character \"{0}\" had Talent-Spell {1} ({2}) but with invalid SpecIndex: {3}",
                            (object) this, (object) spellRecord.SpellId, (object) spellRecord.SpellId,
                            (object) spellRecord.SpecIndex);
                    else
                        specProfiles[spellRecord.SpecIndex].TalentSpells.Add(spellRecord);
                }
                else
                {
                    record.AbilitySpells.Add(spellRecord);
                    this.OnlyAdd(spell);
                }
            }

            foreach (SpellRecord talentSpell in ownerChar.CurrentSpecProfile.TalentSpells)
                this.OnlyAdd(talentSpell);
        }

        internal void LoadCooldowns()
        {
            Character ownerChar = this.OwnerChar;
            DateTime now = DateTime.Now;
            foreach (PersistentSpellIdCooldown record in PersistentSpellIdCooldown.LoadIdCooldownsFor(ownerChar.EntityId
                .Low))
            {
                if (record.Until > now)
                    this.m_idCooldowns.Add((ISpellIdCooldown) record);
                else
                    record.DeleteLater();
            }

            foreach (PersistentSpellCategoryCooldown record in PersistentSpellCategoryCooldown.LoadCategoryCooldownsFor(
                ownerChar.EntityId.Low))
            {
                if (record.Until > now)
                    this.m_categoryCooldowns.Add((ISpellCategoryCooldown) record);
                else
                    record.DeleteLater();
            }
        }

        public SkillLearnStatus TryLearnSpell(short skillId, byte level)
        {
            Spell spell = SpellHandler.Get((uint) skillId + (uint) level * 1000U);
            if (spell == null || level <= (byte) 0)
                return SkillLearnStatus.Fail;
            if ((int) spell.LearnLevel > this.OwnerChar.Level)
                return SkillLearnStatus.LowLevel;
            if (spell.ClassMask != Asda2ClassMask.All &&
                !spell.ClassMask.HasFlag((Enum) this.OwnerChar.Asda2ClassMask) ||
                (int) spell.ProffNum > (int) this.OwnerChar.RealProffLevel)
                return SkillLearnStatus.JoblevelIsNotHighEnought;
            if (this.AvalibleSkillPoints <= 0)
                return SkillLearnStatus.NotEnoghtSpellPoints;
            if (!this.OwnerChar.SubtractMoney((uint) spell.Cost))
                return SkillLearnStatus.NotEnoghtMoney;
            AchievementProgressRecord progressRecord = this.OwnerChar.Achievements.GetOrCreateProgressRecord(1U);
            ++progressRecord.Counter;
            if (progressRecord.Counter == 45U)
            {
                switch (this.OwnerChar.Profession)
                {
                    case Asda2Profession.Warrior:
                        this.OwnerChar.DiscoverTitle(Asda2TitleId.ofBattle24);
                        break;
                    case Asda2Profession.Archer:
                        this.OwnerChar.DiscoverTitle(Asda2TitleId.ofArchery25);
                        break;
                    case Asda2Profession.Mage:
                        this.OwnerChar.DiscoverTitle(Asda2TitleId.ofMagic26);
                        break;
                }
            }

            if (progressRecord.Counter > 90U)
            {
                switch (this.OwnerChar.Profession)
                {
                    case Asda2Profession.Warrior:
                        this.OwnerChar.GetTitle(Asda2TitleId.ofBattle24);
                        break;
                    case Asda2Profession.Archer:
                        this.OwnerChar.GetTitle(Asda2TitleId.ofArchery25);
                        break;
                    case Asda2Profession.Mage:
                        this.OwnerChar.GetTitle(Asda2TitleId.ofMagic26);
                        break;
                }
            }

            progressRecord.SaveAndFlush();
            if (level > (byte) 1)
            {
                Spell oldSpell = this.FirstOrDefault<Spell>((Func<Spell, bool>) (s => (int) s.RealId == (int) skillId));
                if (oldSpell == null || oldSpell.Level != spell.Level - 1)
                    return SkillLearnStatus.BadSpellLevel;
                this.Replace(oldSpell, spell);
                return SkillLearnStatus.Ok;
            }

            this.AddSpell(spell, true);
            this.OwnerChar.SendMoneyUpdate();
            return SkillLearnStatus.Ok;
        }
    }
}