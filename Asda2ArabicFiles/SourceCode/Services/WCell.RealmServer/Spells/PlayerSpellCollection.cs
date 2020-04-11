using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using NLog;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
	public class PlayerSpellCollection : SpellCollection
	{
		static readonly ObjectPool<PlayerSpellCollection> PlayerSpellCollectionPool =
			new ObjectPool<PlayerSpellCollection>(() => new PlayerSpellCollection());

		public static PlayerSpellCollection Obtain(Character chr)
		{
			var spells = PlayerSpellCollectionPool.Obtain();
			spells.Initialize(chr);

			// runes
			if (spells.Runes != null)
			{
				spells.Runes.InitRunes(chr);
			}
			return spells;
		}

		/// <summary>
		/// Whether to send Update Packets
		/// </summary>
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

		/// <summary>
		/// The runes of this Player (if any)
		/// </summary>
		private RuneSet m_runes;

		#region Init & Cleanup
		private PlayerSpellCollection()
		{
			m_idCooldowns =  new List<ISpellIdCooldown>(5);
			m_categoryCooldowns =  new List<ISpellCategoryCooldown>(5);
		}

		protected override void Initialize(Unit owner)
		{
			base.Initialize(owner);

			var chr = (Character)owner;
			m_sendPackets = false;
			if (owner.Class == Constants.ClassId.Balista)
			{
				m_runes = new RuneSet(chr);
			}
		}

		protected internal override void Recycle()
		{
			base.Recycle();

			m_idCooldowns.Clear();
			m_categoryCooldowns.Clear();

			if (m_runes != null)
			{
				m_runes.Dispose();
				m_runes = null;
			}

			PlayerSpellCollectionPool.Recycle(this);
		}
		#endregion

		public IEnumerable<ISpellIdCooldown> IdCooldowns
		{
			get { return m_idCooldowns; }
		}

		public IEnumerable<ISpellCategoryCooldown> CategoryCooldowns
		{
			get { return m_categoryCooldowns; }
		}

		public int IdCooldownCount
		{
			get { return m_idCooldowns.Count; }
		}

		public int CategoryCooldownCount
		{
			get { return m_categoryCooldowns.Count; }
		}

		/// <summary>
		/// Owner as Character
		/// </summary>
		public Character OwnerChar
		{
			get { return (Character)Owner; }
		}

		/// <summary>
		/// The set of runes of this Character (if any)
		/// </summary>
		public RuneSet Runes
		{
			get { return m_runes; }
		}

		#region Add
		public void AddNew(Spell spell)
		{
			AddSpell(spell, false);
		}

		public override void AddSpell(Spell spell)
		{
			AddSpell(spell, true);
		}

		/// <summary>
		/// Adds the spell without doing any further checks nor adding any spell-related skills or showing animations (after load)
		/// </summary>
		internal void OnlyAdd(SpellRecord record)
		{
			var id = record.SpellId;
			//DeleteFromDB(id);
			var spell = SpellHandler.Get(id);
			m_byId[id] = spell;
		}


		/// <summary>
		/// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
		/// </summary>
		void AddSpell(Spell spell, bool sendPacket)
		{
			// make sure the char knows the skill that this spell belongs to
			if (spell.Ability != null)
			{
				var skill = OwnerChar.Skills[spell.Ability.Skill.Id];
				if (skill == null)
				{
					// learn new skill
					skill = OwnerChar.Skills.Add(spell.Ability.Skill, true);
				}

				if (skill.CurrentTierSpell == null || skill.CurrentTierSpell.SkillTier < spell.SkillTier)
				{
					// upgrade tier
					skill.CurrentTierSpell = spell;
				}
			}

			if (!m_byId.ContainsKey(spell.SpellId))
			{
				var owner = OwnerChar;
				

				var specIndex = GetSpecIndex(spell);
				var spells = GetSpellList(spell);
				var newRecord = new SpellRecord(spell.SpellId, owner.EntityId.Low, specIndex);
				newRecord.SaveLater();
				spells.Add(newRecord);
                
				base.AddSpell(spell);
                
			}
            if (m_sendPackets && sendPacket)
            {
                if (spell.Level == 1)
                    Asda2SpellHandler.SendSkillLearnedFirstTimeResponse(OwnerChar.Client, spell.RealId,spell.CooldownTime);
                Asda2SpellHandler.SendSkillLearnedResponse(SkillLearnStatus.Ok, OwnerChar, (uint)spell.RealId, spell.Level);
                //SpellHandler.SendLearnedSpell(owner.Client, spell.Id);
                /*if (!spell.IsPassive)
                {
                    SpellHandler.SendVisual(owner, 362);	// ouchy: Unnamed constants 
                }*/
            }
		}
		#endregion

		#region Remove/Replace/Clear
		/// <summary>
		/// Replaces or (if newSpell == null) removes oldSpell.
		/// </summary>
		public override bool Replace(Spell oldSpell, Spell newSpell)
		{
			var hasOldSpell = oldSpell != null && m_byId.Remove(oldSpell.SpellId);
			if (hasOldSpell)
			{
				OnRemove(oldSpell);
				if (newSpell == null)
				{
					if (m_sendPackets)
					{
						//SpellHandler.SendSpellRemoved(OwnerChar, oldSpell.Id);
					}
					return true;
				}
			}

			if (newSpell != null)
			{
				if (m_sendPackets && hasOldSpell)
				{
					//SpellHandler.SendSpellSuperceded(OwnerChar.Client, oldSpell.Id, newSpell.Id);
				}

				AddSpell(newSpell, true);
			}
			return hasOldSpell;
		}

		/// <summary>
		/// Enqueues a new task to remove that spell from DB
		/// </summary>
		private void OnRemove(Spell spell)
		{
			var chr = OwnerChar;
			if (spell.RepresentsSkillTier)
			{
				// TODO: Skill might now be represented by a lower tier, and only the MaxValue changes
				chr.Skills.Remove(spell.Ability.Skill.Id);
			}

			// Remove any active auras from the player
			if (spell.IsAura)
			{
				chr.Auras.Remove(spell);
			}

			// figure out from where to remove and do it
			var spells = GetSpellList(spell);
			for (var i = 0; i < spells.Count; i++)
			{
				var record = spells[i];
				if (record.SpellId == spell.SpellId)
				{
					// delete and remove
					RealmServer.IOQueue.AddMessage(new Message(record.Delete));
					spells.RemoveAt(i);
					return;
				}
			}
		}

		int GetSpecIndex(Spell spell)
		{
			var chr = OwnerChar;
			return spell.IsTalent ? chr.Talents.CurrentSpecIndex : SpellRecord.NoSpecIndex;
		}

		List<SpellRecord> GetSpellList(Spell spell)
		{
			var chr = OwnerChar;
			if (spell.IsTalent)
			{
				return chr.CurrentSpecProfile.TalentSpells;
			}
			else
			{
				return chr.Record.AbilitySpells;
			}
		}

		public override void Clear()
		{
			foreach (var spell in m_byId.Values.ToArray())
			{
				OnRemove(spell);
				
			}

			base.Clear();
		}
		#endregion

		#region Init
		internal void PlayerInitialize()
		{
			// re-apply passive effects
			var chr = OwnerChar;
			foreach (var spell in m_byId.Values)
			{
				if (spell.Talent != null)
				{
					// add talents silently to TalentCollection
					chr.Talents.AddExisting(spell.Talent, spell.Rank);
				}
				else if (spell.IsPassive && !spell.HasHarmfulEffects)
				{
					// cast passive spells
					chr.SpellCast.Start(spell, true, Owner);
				}
			}

			// apply all highest ranks of all Talents
			foreach (var talent in chr.Talents)
			{
				var spell = talent.Spell;
				if (spell.IsPassive)
				{
					// cast passive Talent spells
					chr.SpellCast.Start(spell, true, Owner);
				}
			}

			m_sendPackets = true;
		}

		public override void AddDefaultSpells()
		{
			// add the default Spells for the race/class
			/*for (var i = 0; i < OwnerChar.Archetype.Spells.Count; i++)
			{
				var spell = OwnerChar.Archetype.Spells[i];
				AddNew(spell);
			}*/
            AddNew(SpellHandler.Get(SpellId.BashRank1));
            AddNew(SpellHandler.Get(SpellId.ArrowStrikeRank1));
			// add all default Spells of all Skills the Char already has
			//foreach (var skill in OwnerChar.Skills)
			//{
			//    for (var i = 0; i < skill.SkillLine.InitialAbilities.Count; i++)
			//    {
			//        var ability = skill.SkillLine.InitialAbilities[i];
			//        AddNew(ability.Spell);
			//    }
			//}
		}
		#endregion

		#region Spell Constraints
		/// <summary>
		/// Add everything to the caster that this spell requires
		/// </summary>
		public void AddSpellRequirements(Spell spell)
		{
			/*var chr = OwnerChar;
			// add reagents
			foreach (var reagent in spell.Reagents)
			{
				var templ = reagent.Template;
				if (templ != null)
				{
					var amt = reagent.Amount * 10;
					chr.Inventory.Ensure(templ, amt);
				}
			}

			// add tools
			if (spell.RequiredTools != null)
			{
				foreach (var tool in spell.RequiredTools)
				{
					chr.Inventory.Ensure(tool.Template, 1);
				}
			}
			if (spell.RequiredToolCategories != null)
			{
				foreach (var cat in spell.RequiredToolCategories)
				{
					var tool = ItemMgr.GetFirstItemOfToolCategory(cat);
					if (tool != null)
					{
						chr.Inventory.Ensure(tool, 1);
					}
				}
			}

			// Profession
			if (spell.Ability != null && spell.Ability.Skill != null)
			{
				chr.Skills.TryLearn(spell.Ability.Skill.Id);
			}


			// add spellfocus object (if not present)
			if (spell.RequiredSpellFocus != 0)
			{
				var range = Owner.GetSpellMaxRange(spell);
				var go = chr.Map.GetGOWithSpellFocus(chr.Position, spell.RequiredSpellFocus,
					range > 0 ? (range) : 5f, chr.Phase);

				if (go == null)
				{
					foreach (var entry in GOMgr.Entries.Values)
					{
						if (entry is GOSpellFocusEntry &&
							((GOSpellFocusEntry)entry).SpellFocus == spell.RequiredSpellFocus)
						{
							entry.Spawn(chr, chr);
							break;
						}
					}
				}
			}*/
		}
		#endregion

		#region Cooldowns
		/// <summary>
		/// Returns true if spell is currently cooling down.
		/// Removes expired cooldowns of that spell.
		/// </summary>
		public override bool IsReady(Spell spell)
		{
			// check for individual cooldown
			ISpellIdCooldown idCooldown = null;
			if (spell.CooldownTime > 0)
			{
				for (var i = 0; i < m_idCooldowns.Count; i++)
				{
					idCooldown = m_idCooldowns[i];
					if (idCooldown.SpellId == spell.Id)
					{
						if (idCooldown.Until > DateTime.Now)
						{
							return false;
						}
						m_idCooldowns.RemoveAt(i);
						break;
					}
				}
			}

			/*// enqueue task to delete persistent cooldowns
			if (idCooldown is PersistentSpellIdCooldown )//|| catCooldown is PersistentSpellCategoryCooldown)
			{
				var removedId = idCooldown as PersistentSpellIdCooldown;
				//var removedCat = catCooldown as PersistentSpellCategoryCooldown;
				RealmServer.IOQueue.AddMessage(new Message(() =>
				{
					if (removedId != null)
					{
						removedId.Delete();
					}
					/*if (removedCat != null)
					{
						removedCat.Delete();
					}#1#
				}));
			}*/
			return true;
		}

		public override void AddCooldown(Spell spell, Item casterItem)
		{
			var itemSpell = casterItem != null && casterItem.Template.UseSpell != null;

			var cd = 0;
			if (itemSpell)
			{
				cd = casterItem.Template.UseSpell.Cooldown;
			}
			if (cd == 0)
			{
				cd = spell.GetCooldown(Owner);
			}


			if (cd > 0)
			{
				var idCooldown = new SpellIdCooldown
				{
					SpellId = spell.Id,
					Until = (DateTime.Now + TimeSpan.FromMilliseconds(cd))
				};

				if (itemSpell)
				{
					idCooldown.ItemId = casterItem.Template.Id;
				}
				m_idCooldowns.Add(idCooldown);

                OwnerChar.Map.CallDelayed(500, () => Asda2SpellHandler.SendSetSkillCooldownResponse(OwnerChar, spell));
                OwnerChar.Map.CallDelayed(cd, () => Asda2SpellHandler.SendClearCoolDown(OwnerChar, spell.RealId));
			}


		}

		/// <summary>
		/// Clears all pending spell cooldowns.
		/// </summary>
		public override void ClearCooldowns()
		{
			// send cooldown updates to client
			foreach (var cd in m_idCooldowns)
			{
				Asda2SpellHandler.SendClearCoolDown(OwnerChar, (SpellId)cd.SpellId);
			}

			foreach (var spell in m_byId.Values)
			{
				foreach (var cd in m_categoryCooldowns)
				{
					if (spell.Category == cd.CategoryId)
					{
						Asda2SpellHandler.SendClearCoolDown(OwnerChar, spell.SpellId);
						break;
					}
				}
			}

			// remove and delete all cooldowns
			var cds = m_idCooldowns.ToArray();
			var catCds = m_categoryCooldowns.ToArray();
			m_idCooldowns.Clear();
			m_categoryCooldowns.Clear();

			RealmServer.IOQueue.AddMessage(new Message(() =>
			{
				foreach (var cooldown in cds)
				{
					if (cooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)cooldown).Delete();
					}
				}
				foreach (var cooldown in catCds)
				{
					if (cooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)cooldown).Delete();
					}
				}
			}));

		}

		/// <summary>
		/// Clears the cooldown for this spell
		/// </summary>
		public override void ClearCooldown(Spell cooldownSpell, bool alsoCategory = true)
		{
			var ownerChar = OwnerChar;

			// send cooldown update to client
			Asda2SpellHandler.SendClearCoolDown(ownerChar, cooldownSpell.SpellId);
			if (alsoCategory && cooldownSpell.Category != 0)
			{
				foreach (var spell in m_byId.Values)
				{
					if (spell.Category == cooldownSpell.Category)
					{
						Asda2SpellHandler.SendClearCoolDown(ownerChar, spell.SpellId);
					}
				}
			}

			// remove and delete
			ISpellIdCooldown idCooldown = m_idCooldowns.RemoveFirst(cd => cd.SpellId == cooldownSpell.Id);
			ISpellCategoryCooldown catCooldown = m_categoryCooldowns.RemoveFirst(cd => cd.CategoryId == cooldownSpell.Category);

			// enqueue task
			if (idCooldown is ActiveRecordBase || catCooldown is ActiveRecordBase)
			{
				RealmServer.IOQueue.AddMessage(new Message(() =>
				{
					if (idCooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)idCooldown).Delete();
					}
					if (catCooldown is ActiveRecordBase)
					{
						((ActiveRecordBase)catCooldown).Delete();
					}
				}
				));
			}
		}

		private void SaveCooldowns()
		{
			SaveCooldowns(m_idCooldowns);
			//SaveCooldowns(m_categoryCooldowns);
		}

		private void SaveCooldowns<T>(List<T> cooldowns) where T : ICooldown
		{
			for (var i = cooldowns.Count - 1; i >= 0; i--)
			{
				ICooldown cooldown = cooldowns[i];
			    if (cooldown.Until < DateTime.Now.AddMilliseconds(SpellHandler.MinCooldownSaveTimeMillis))
			    {
			        // already expired or will expire very soon
			        if (cooldown is ActiveRecordBase)
			        {
			                    ((ActiveRecordBase) cooldown).DeleteLater();
			        }
			    }
			    else
			    {
			                var cd = cooldown.AsConsistent();
			                cd.CharId = Owner.EntityId.Low;
			                cd.Save(); // update or create
			                cooldowns.Add((T) cd);
			    }
                cooldowns.RemoveAt(i);
			}
		}

		#endregion

		#region Save / Load
		/// <summary>
		/// Called to save runes (cds & spells are saved in another way)
		/// </summary>
		internal void OnSave()
		{
		    try
            {
                SaveCooldowns();
		    }
		    catch (Exception ex)
		    {
		        LogUtil.ErrorException(ex,string.Format("Failed to save cooldowns for {0}.",OwnerChar==null?"Not character":OwnerChar.Name));
		    }
		/*	if (m_runes != null)
			{
				var record = OwnerChar.Record;
				record.RuneSetMask = m_runes.PackRuneSetMask();
				record.RuneCooldowns = m_runes.Cooldowns;
			}*/
		}

		internal void LoadSpellsAndTalents()
		{
			var owner = OwnerChar;
			var ownerRecord = owner.Record;

			// add Spells from DB into the correct collections
			var dbSpells = SpellRecord.LoadAllRecordsFor(owner.EntityId.Low);
			var specs = owner.SpecProfiles;
			foreach (var record in dbSpells)
			{
				var spell = record.Spell;
				if (spell == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Character \"{0}\" had invalid spell: {1} ({2})", this, record.SpellId,
															(uint)record.SpellId);
					continue;
				}

				if (spell.IsTalent)
				{
					if (record.SpecIndex < 0 || record.SpecIndex >= specs.Length)
					{
						LogManager.GetCurrentClassLogger().Warn(
							"Character \"{0}\" had Talent-Spell {1} ({2}) but with invalid SpecIndex: {3}", this, record.SpellId,
							(uint)record.SpellId, record.SpecIndex);
						continue;
					}
					specs[record.SpecIndex].TalentSpells.Add(record);
				}
				else
				{
					ownerRecord.AbilitySpells.Add(record);
					OnlyAdd(spell);		// add ability spell
				}
			}

			// add talents
			foreach (var spellRecord in owner.CurrentSpecProfile.TalentSpells)
			{
                // Add the talent spell to the character
				OnlyAdd(spellRecord);
			}
		}

		internal void LoadCooldowns()
		{
			var owner = OwnerChar;
			var now = DateTime.Now;

			foreach (var cd in PersistentSpellIdCooldown.LoadIdCooldownsFor(owner.EntityId.Low))
			{
				if (cd.Until > now)
				{
					m_idCooldowns.Add(cd);
				}
				else
				{
					cd.DeleteLater();
				}
			}

			foreach (var cd in PersistentSpellCategoryCooldown.LoadCategoryCooldownsFor(owner.EntityId.Low))
			{
				if (cd.Until > now)
				{
					m_categoryCooldowns.Add(cd);
				}
				else
				{
					cd.DeleteLater();
				}
			}
		}
		#endregion

	    public SkillLearnStatus TryLearnSpell(short skillId, byte level)
	    {
	        var spell = SpellHandler.Get((uint) (skillId + level*1000));
            if(spell == null || level <=0)
                return SkillLearnStatus.Fail;
            if(spell.LearnLevel > OwnerChar.Level)
                return SkillLearnStatus.LowLevel;
            if(spell.ClassMask!=Asda2ClassMask.All&&!spell.ClassMask.HasFlag(OwnerChar.Asda2ClassMask))
                return SkillLearnStatus.BadProffession;
	       
            if(spell.ProffNum>OwnerChar.RealProffLevel)
                return SkillLearnStatus.BadProffession;
            if(AvalibleSkillPoints<=0)
                return SkillLearnStatus.NotEnoghtSpellPoints;
            if(!OwnerChar.SubtractMoney((uint) spell.Cost))
                return SkillLearnStatus.NotEnoghtMoney;
	        if(level>1)
            {
                var previusLvlSpell = this.FirstOrDefault(s => s.RealId == skillId);
                if(previusLvlSpell == null || previusLvlSpell.Level!=spell.Level-1)
                    return SkillLearnStatus.BadSpellLevel;
                Replace(previusLvlSpell, spell);
                return SkillLearnStatus.Ok;
            }
            AddSpell(spell,true);
            OwnerChar.SendMoneyUpdate();
            return SkillLearnStatus.Ok;
	    }
	}
}