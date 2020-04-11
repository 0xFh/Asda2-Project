/*************************************************************************
 *
 *   file		: Spell.Fields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $

 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
	public partial class Spell
    {
        [Persistent]
	    public short RealId;
        [Persistent]
		public uint Id;//1
		public SpellId SpellId;//1
        [Persistent]
		/// <summary>
		/// SpellCategory.dbc
		/// </summary>
		public uint Category;//3
        [Persistent]
		/// <summary>
		/// SpellDispelType.dbc
		/// </summary>
		public DispelType DispelType;//5
        [Persistent]
		/// <summary>
		/// SpellMechanic.dbc
		/// </summary>
		public SpellMechanic Mechanic;//6
        [Persistent]
		public SpellAttributes Attributes;//7
        [Persistent]
		public SpellAttributesEx AttributesEx;//8
        [Persistent]
		public SpellAttributesExB AttributesExB;//9\
        [Persistent]
		public SpellAttributesExC AttributesExC;//10
        [Persistent]
		public SpellAttributesExD AttributesExD;//11
        [Persistent]
		public SpellAttributesExE AttributesExE;//12
        [Persistent]
		public SpellAttributesExF AttributesExF;//13

        // 3.2.2 unk
        public uint Unk_322_1;
        public uint Unk_322_2;
        public uint Unk_322_3;
        public float Unk_322_4_1;
        public float Unk_322_4_2;
        public float Unk_322_4_3;

        /// <summary>
        /// 3.2.2 related to description?
        /// </summary>
        public uint spellDescriptionVariablesID;

		/// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask RequiredShapeshiftMask;//13
		/// <summary>
		/// SpellShapeshiftForm.dbc
		/// </summary>
		public ShapeshiftMask ExcludeShapeshiftMask;//14
        [Persistent]
		public SpellTargetFlags TargetFlags;
		/// <summary>
		/// CreatureType.dbc
		/// </summary>
		public CreatureMask CreatureMask;
		/// <summary>
		/// SpellFocusObject.dbc
		/// </summary>
		public SpellFocus RequiredSpellFocus;//17

		public SpellFacingFlags FacingFlags;
        [Persistent]
		public AuraState RequiredCasterAuraState;//18
        [Persistent]
		public AuraState RequiredTargetAuraState;//19
        [Persistent]
		public AuraState ExcludeCasterAuraState;
        [Persistent]
		public AuraState ExcludeTargetAuraState;
        [Persistent]
		/// <summary>
		/// Can only cast if caster has this Aura
		/// Used for some new BG features (Homing missiles etc)
		/// </summary>
        public SpellId RequiredCasterAuraId;
        [Persistent]
		/// <summary>
		/// Can only cast if target has this Aura
		/// </summary>
        public SpellId RequiredTargetAuraId;
        [Persistent]
		/// <summary>
		/// Cannot be cast if caster has this
		/// </summary>
        public SpellId ExcludeCasterAuraId;
        [Persistent]
		/// <summary>
		/// Cannot be cast on target if he has this
		/// </summary>
		public SpellId ExcludeTargetAuraId;
        [Persistent]
		/// <summary>
		/// Cast delay in milliseconds
		/// </summary>
		public uint CastDelay;//22
        [Persistent]
		public int CooldownTime;//23
        [Persistent]
		public int categoryCooldownTime;//24

		public int CategoryCooldownTime
		{
			get { return categoryCooldownTime; }
		}
        [Persistent]
		public InterruptFlags InterruptFlags;//25
        [Persistent]
		public AuraInterruptFlags AuraInterruptFlags;//26
        [Persistent]
		public ChannelInterruptFlags ChannelInterruptFlags;//27
        [Persistent]
        public ProcTriggerFlags ProcTriggerFlags;//28

		/// <summary>
		/// Indicates events which cause this spell to trigger its proc effect 
		/// </summary>
		/// <remarks>
		/// This spell must be a proc <see cref="IsProc"/>
		/// </remarks>
		public ProcTriggerFlags ProcTriggerFlagsProp
		{
			get { return ProcTriggerFlags; }
			set
			{
				ProcTriggerFlags = value;

				if (ProcTriggerFlags.RequireHitFlags() && ProcHitFlags == ProcHitFlags.None)
				{
					// Default proc on hit
					ProcHitFlags = ProcHitFlags.Hit;
				}
			}
		}

		/// <summary>
		/// Contains information needed for ProcTriggerFlags depending on hit result
		/// </summary>
		/// <remarks> 
		/// This spell must be a proc <see cref="IsProc"/>
		/// </remarks>
		public ProcHitFlags ProcHitFlags { get; set; }
        [Persistent]
		public uint ProcChance;//29
        [Persistent]
		public int ProcCharges;//30
        [Persistent]
		public int MaxLevel;//31
        [Persistent]
		public int BaseLevel;//32
        [Persistent]
		public int Level;//33
		/// <summary>
		/// SpellDuration.dbc
		/// </summary>
		public int DurationIndex;
		[NotPersistent]
		public DurationEntry Durations;//34
        [Persistent]
		public PowerType PowerType;//35
        [Persistent]
		public int PowerCost;//36
        [Persistent]
		public int PowerCostPerlevel;//37
        [Persistent]
		public int PowerPerSecond;//38
		/// <summary>
		/// Unused so far
		/// </summary>
		public int PowerPerSecondPerLevel;//39

		/// <summary>
		/// SpellRange.dbc
		/// </summary>
		public int RangeIndex;
		/// <summary>
		/// Read from SpellRange.dbc
		/// </summary>
		[NotPersistent]
		public SimpleRange Range;//40
		/// <summary>
		/// The speed of the projectile in yards per second
		/// </summary>
		public float ProjectileSpeed;//41
		/// <summary>
		/// Hunter ranged spells have this. It seems always to be 75
		/// </summary>
		public SpellId ModalNextSpell;//42
        [Persistent]
		public int MaxStackCount;//43

		[Persistent(2)]
		public uint[] RequiredToolIds;//44 - 45
		[Persistent(8)]
		public uint[] ReagentIds;
		[Persistent(8)]
		public uint[] ReagentCounts;
		[NotPersistent]
		public ItemStackDescription[] Reagents; // 46 - 61

		/// <summary>
		/// ItemClass.dbc
		/// </summary>
		public ItemClass RequiredItemClass;//62

		/// <summary>
		/// Mask of ItemSubClasses, used for Enchants and Combat Abilities
		/// </summary>
		public ItemSubClassMask RequiredItemSubClassMask;//63

		/// <summary>
		/// Mask of InventorySlots, used for Enchants only
		/// </summary>
		public InventorySlotTypeMask RequiredItemInventorySlotMask;//64

		/// <summary>
		/// Does not count void effect handlers
		/// </summary>
		[NotPersistent]
		public int EffectHandlerCount;

		[NotPersistent]
		public SpellEffect[] Effects;//65 - 118

		/// <summary>
		/// SpellVisual.dbc
		/// </summary>
		public uint Visual;//119
		/// <summary>
		/// SpellVisual.dbc
		/// </summary>
		public uint Visual2;//120

		/// <summary>
		/// SpellIcon.dbc
		/// </summary>
		public uint SpellbookIconId;//121
		/// <summary>
		/// SpellIcon.dbc
		/// </summary>
		public uint BuffIconId;//122

		public uint Priority;//123
        [Persistent]
		public string Name;// 124 - 140
		private string m_RankDesc;

		public string RankDesc// 141 - 157
		{
			get { return m_RankDesc; }
			set
			{
				m_RankDesc = value;

				if (value.Length > 0)
				{
					var rank = numberRegex.Match(value);
					if (rank.Success)
					{
						int.TryParse(rank.Value, out Rank);
					}
				}
			}
		}

        public int Rank;
        [Persistent]
		public string Description; // 158 - 174
		public string BuffDescription; // 175 - 191

		public int PowerCostPercentage;              //192
		/// <summary>
		/// Always 0?
		/// </summary>
		public int StartRecoveryTime;               //194
		public int StartRecoveryCategory;           //195
		public uint MaxTargetLevel;
		private SpellClassSet spellClassSet;		 //196
		public SpellClassSet SpellClassSet
		{
			get { return spellClassSet; }
			set
			{
				spellClassSet = value;
				ClassId = value.ToClassId();
			}
		}
		public ClassId ClassId;

		[Persistent(3)]
		public uint[] SpellClassMask = new uint[SpellConstants.SpellClassMaskSize];
		public uint MaxTargets;                      //199 
        [Persistent]
		public DamageType DamageType;
		public SpellPreventionType PreventionType;
		public int StanceBarOrder;
		/// <summary>
		/// Used for effect-value damping when using chain targets, eg:
		///		DamageMultipliers: 0.6, 1, 1
		///		"Each jump reduces the effectiveness of the heal by 40%.  Heals $x1 total targets."
		/// </summary>
		[Persistent(3)]
		public float[] DamageMultipliers = new float[3];
		/// <summary>
		/// only one spellid:6994 has this value = 369
		/// </summary>
		public uint MinFactionId;
		/// <summary>
		/// only one spellid:6994 has this value = 4
		/// </summary>
		public uint MinReputation;
		/// <summary>
		/// only one spellid:26869  has this flag = 1 
		/// </summary>
		public uint RequiredAuraVision;

		[NotPersistent]
		public ToolCategory[] RequiredToolCategories = new ToolCategory[2];// 209 - 210
		/// <summary>
		/// AreaGroup.dbc
		/// </summary>
		public uint AreaGroupId;// 211
        [Persistent]
		public DamageSchoolMask SchoolMask;

		/// <summary>
		/// SpellRuneCost.dbc
		/// </summary>
		public RuneCostEntry RuneCostEntry;

		/// <summary>
		/// SpellMissile.dbc
		/// </summary>
		public uint MissileId;

		/// <summary>
		/// PowerDisplay.dbc
		/// </summary>
		/// <remarks>Added in 3.1.0</remarks>
		public int PowerDisplayId;

		[NotPersistent]
		public DamageSchool[] Schools;

        [Persistent]
        public SpellEffectType Effect0_EffectType;
        [Persistent]
        public SpellMechanic Effect0_Mehanic;
        [Persistent]
        public ImplicitSpellTargetType Effect0_ImplicitTargetA;
        [Persistent]
        public ImplicitSpellTargetType Effect0_ImplicitTargetB;
        [Persistent]
        public float Effect0_Radius;
        [Persistent]
        public AuraType Effect0_AuraType;
        [Persistent]
        public int Effect0_Amplitude;
        [Persistent]
        public float Effect0_ProcValue;
        [Persistent]
        public int Effect0_MiscValue;
        [Persistent]
        public int Effect0_MiscValueB;
        [Persistent]
        public int Effect0_MiscValueC;
        [Persistent]
        public SpellEffectType Effect1_EffectType;
        [Persistent]
        public SpellMechanic Effect1_Mehanic;
        [Persistent]
        public ImplicitSpellTargetType Effect1_ImplicitTargetA;
        [Persistent]
        public ImplicitSpellTargetType Effect1_ImplicitTargetB;
        [Persistent]
        public float Effect1_Radius;
        [Persistent]
        public AuraType Effect1_AuraType;
        [Persistent]
        public int Effect1_Amplitude;
        [Persistent]
        public float Effect1_ProcValue;
        [Persistent]
        public int Effect1_MiscValue;
        [Persistent]
        public int Effect1_MiscValueB;
        [Persistent]
        public int Effect1_MiscValueC;
	}

}