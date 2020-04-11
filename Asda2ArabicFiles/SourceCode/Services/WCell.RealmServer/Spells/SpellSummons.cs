using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
	#region SpellSummonHandlers
    [System.Serializable]
	/// <summary>
	/// SpellSummonHandler's define how objects/NPCs of certain summontypes are to be summoned
	/// </summary>
	public class SpellSummonHandler
	{
		public virtual bool CanSummon(SpellCast cast, NPCEntry entry)
		{
			return true;
		}

		public virtual NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var caster = cast.CasterUnit;
			var duration = cast.Spell.GetDuration(cast.CasterReference);

			NPC minion;
			if (caster != null)
			{
				minion = caster.SpawnMinion(entry, ref targetLoc, duration);
			}
			else
			{
				minion = entry.Create(cast.TargetMap.DifficultyIndex);

				minion.Position = targetLoc;
				minion.Brain.IsRunning = true;
				minion.Phase = cast.Phase;
				cast.Map.AddObject(minion);
			}

			if (caster is Character)
			{
				minion.Level = caster.Level;
			}
			minion.Summoner = caster;
			minion.Creator = cast.CasterReference.EntityId;
			if(caster != null)
			{
				caster.Summon = minion.EntityId;
				if(caster.HasMaster)
					minion.Master = caster.Master;
			}

			return minion;
		}
	}

	public class SpellGenericSummonHandler : SpellSummonHandler
	{
		public delegate NPC Delegate(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry);

		public Delegate Callback { get; set; }

		public SpellGenericSummonHandler(Delegate callback)
		{
			Callback = callback;
		}
	}

	/// <summary>
	/// Non-combat pets
	/// </summary>
	public class SpellSummonCritterHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var pet = base.Summon(cast, ref targetLoc, entry);
			return pet;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SpellSummonPetHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var caster = cast.CasterUnit;
			if (caster is Character)
			{
				return ((Character)caster).SpawnPet(entry, ref targetLoc, cast.Spell.GetDuration(caster.SharedReference));
			}
			else
			{
				return base.Summon(cast, ref targetLoc, entry);
			}
		}
	}

	public class SpellSummonImmovableHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var npc = base.Summon(cast, ref targetLoc, entry);
			npc.HasPermissionToMove = false;
			return npc;
		}
	}

	public class SpellSummonPossessedHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var npc = base.Summon(cast, ref targetLoc, entry);
			if(cast.CasterChar != null)
			{
				//Client needs to think we are charmer not summoner!
				cast.CasterChar.Summon = EntityId.Zero;
				npc.Summoner = null;
				npc.Master = cast.CasterChar;
				npc.AddMessage(() => cast.CasterChar.Possess(0, npc, true, false)); 
			}
			return npc;
		}
	}

	public class SpellSummonTotemHandler : SpellSummonHandler
	{
		public SpellSummonTotemHandler(uint index)
		{
			Index = index;
		}

		public uint Index
		{
			get;
			private set;
		}
	}

	public class SpellSummonDoomguardHandler : SpellSummonHandler
	{
		public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
		{
			var npc = entry.SpawnAt(cast.Map, targetLoc);
			npc.RemainingDecayDelayMillis = cast.Spell.GetDuration(cast.CasterReference);
			npc.Creator = cast.CasterReference.EntityId;	// should be right
			return npc;
		}
	}
	#endregion
    [System.Serializable]
	public class SpellSummonEntry
	{
		public SummonType Id;
		public SummonGroup Group;
		public FactionTemplateId FactionTemplateId;
		public SummonPropertyType Type;
		public uint Slot;
		public SummonFlags Flags;

		/// <summary>
		/// If set to false, the amount determines health
		/// </summary>
		public bool DetermineAmountBySpellEffect = true;

		public SpellSummonHandler Handler;
	}

	public class SummonPropertiesConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var entry = new SpellSummonEntry
			{
				Id = (SummonType)rawData.GetInt32(0),
				Group = (SummonGroup)rawData.GetInt32(1),
				FactionTemplateId = (FactionTemplateId)rawData.GetInt32(2),
				Type = (SummonPropertyType)rawData.GetInt32(3),
				Slot = rawData.GetUInt32(4),
				Flags = (SummonFlags)rawData.GetInt32(5)
			};

			SpellHandler.SummonEntries[entry.Id] = entry;
		}
	}
}