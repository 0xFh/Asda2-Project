using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
  /// <summary>
  /// Quest Templates represent all information that is associated with a possible ingame Quest.
  /// TODO: AreaTrigger relations
  /// </summary>
  [Serializable]
  public class QuestTemplate : IDataHolder
  {
    /// <summary>The list of all entities that can start this Quest</summary>
    public readonly List<IQuestHolderEntry> Starters = new List<IQuestHolderEntry>(3);

    /// <summary>The List of all entities that can finish this Quest</summary>
    public readonly List<IQuestHolderEntry> Finishers = new List<IQuestHolderEntry>(3);

    /// <summary>Determines whether quest is active or not.</summary>
    public QuestTemplateStatus IsActive = QuestTemplateStatus.Active;

    /// <summary>
    /// Array of interactions containing ID, index and quantity.
    /// </summary>
    [Persistent(4)]public QuestInteractionTemplate[] ObjectOrSpellInteractions = new QuestInteractionTemplate[4];

    /// <summary>
    /// Array of source items interactions containing ID and quantity.
    /// </summary>
    [Persistent(4)]public Asda2ItemStackDescription[] CollectableSourceItems = new Asda2ItemStackDescription[4];

    /// <summary>
    /// Array of quest objectives text, every value is a short note that is shown
    /// once all objectives of the corresponding slot have been fullfilled.
    /// </summary>
    [Persistent(8)]public QuestObjectiveSet[] ObjectiveTexts = new QuestObjectiveSet[8];

    [NotPersistent]public List<uint> EventIds = new List<uint>();

    /// <summary>
    /// Array of Items to be given upon accepting the quest. These items will be destroyed when the Quest is solved or canceled.
    /// </summary>
    [NotPersistent]public List<Asda2ItemStackDescription> ProvidedItems = new List<Asda2ItemStackDescription>(1);

    /// <summary>
    /// Quests that may must all be active in order to get this Quest
    /// </summary>
    [NotPersistent]public readonly List<uint> ReqAllActiveQuests = new List<uint>(2);

    /// <summary>
    /// Quests that must all be finished in order to get this Quest
    /// </summary>
    [NotPersistent]public readonly List<uint> ReqAllFinishedQuests = new List<uint>(2);

    /// <summary>
    /// Quests of which at least one must be active to get this Quest
    /// </summary>
    [NotPersistent]public readonly List<uint> ReqAnyActiveQuests = new List<uint>(2);

    /// <summary>
    /// Quests of which at least one must be finished to get this Quest
    /// </summary>
    [NotPersistent]public readonly List<uint> ReqAnyFinishedQuests = new List<uint>(2);

    /// <summary>
    /// Quests of which none may have been accepted or completed
    /// </summary>
    [NotPersistent]public readonly List<uint> ReqUndoneQuests = new List<uint>(2);

    /// <summary>
    /// Triggers or areas which are objective to be explored as requirements.
    /// </summary>
    [NotPersistent]public uint[] AreaTriggerObjectives = new uint[0];

    /// <summary>
    /// Array of <see href="ReputationReward">ReputationRewards</see>
    /// </summary>
    [Persistent(5)]public ReputationReward[] RewardReputations = new ReputationReward[5];

    [Persistent(4)]public EmoteTemplate[] QuestDetailedEmotes = new EmoteTemplate[4];
    [Persistent(4)]public EmoteTemplate[] OfferRewardEmotes = new EmoteTemplate[4];
    private const uint MaxId = 200000;

    /// <summary>
    /// Single handler to verify whether a Quest has been completed
    /// </summary>
    [NotPersistent]public Func<Quest, bool> CompleteHandler;

    /// <summary>Unique identifier of quest.</summary>
    public uint Id;

    /// <summary>
    /// Level of given quest, for which the quest is optimized. (If FFFFFFFF, then it's level independent or special)
    /// </summary>
    public uint Level;

    /// <summary>
    /// Level of given quest, for which the quest is optimized. (If FFFFFFFF, then it's level independent or special)
    /// </summary>
    public uint MinLevel;

    /// <summary>
    /// 
    /// </summary>
    public int Category;

    /// <summary>Restricted to this Zone</summary>
    [NotPersistent]public ZoneTemplate ZoneTemplate;

    /// <summary>
    /// QuestType, for more detailed description of type look at <seealso cref="T:WCell.Constants.Quests.QuestType" />
    /// </summary>
    public QuestType QuestType;

    /// <summary>Number of players quest is optimized for.</summary>
    public uint SuggestedPlayers;

    /// <summary>
    /// 
    /// </summary>
    public FactionReputationEntry ObjectiveMinReputation;

    /// <summary>Player cannot have more than this in reputation</summary>
    public FactionReputationEntry ObjectiveMaxReputation;

    /// <summary>
    /// Gets or sets the reward money in copper, if it's negative,
    /// money will be required for quest completition and deducted
    /// from player's money after completition.
    /// 1     = 1 copper
    /// 10    = 10 copper
    /// 100   = 1 silver
    /// 1000  = 10 silver
    /// 10000 = 1 gold
    /// </summary>
    public int RewMoney;

    /// <summary>Money gained instead of RewMoney at level 70.</summary>
    public uint MoneyAtMaxLevel;

    /// <summary>
    /// Given spell id, which is added to character's spell book when finishing the quest.
    /// </summary>
    public SpellId RewSpell;

    /// <summary>
    /// Cast spell id of spell which is casted on character when finishing the quest.
    /// </summary>
    public SpellId CastSpell;

    /// <summary>
    /// 
    /// </summary>
    [NotPersistent]public uint BonusHonor;

    /// <summary>An Quest-starting Item</summary>
    public Asda2ItemId SrcItemId;

    /// <summary>
    /// QuestFlags, for more detailed description of flags look at <see cref="T:WCell.Constants.Quests.QuestFlags" />
    /// </summary>
    public QuestFlags Flags;

    /// <summary>
    /// 
    /// </summary>
    public TitleId RewardTitleId;

    public uint RewardTalents;

    /// <summary>
    /// The id of the loot to be sent via mail to the finisher after completion
    /// </summary>
    public uint RewardMailTemplateId;

    /// <summary>The delay after which the Reward Mail should be sent</summary>
    public uint RewardMailDelaySeconds;

    /// <summary>
    /// Array of items containing item ID, index and quantity of items.
    /// </summary>
    [Persistent(4)]public ItemStackDescription[] RewardItems;

    /// <summary>
    /// Array of items containing item ID, index and quantity of items.
    /// </summary>
    [Persistent(6)]public ItemStackDescription[] RewardChoiceItems;

    /// <summary>Map Id of point showing something.</summary>
    public MapId MapId;

    /// <summary>X-coordinate of point showing something.</summary>
    public float PointX;

    /// <summary>Y-coordinate of point showing something.</summary>
    public float PointY;

    /// <summary>Options of point showing something.</summary>
    public uint PointOpt;

    [Persistent(8)]public string[] Titles;

    /// <summary>Text sumarizing the objectives of quest.</summary>
    [Persistent(8)]public string[] Instructions;

    /// <summary>
    /// Detailed quest descriptions shown in <see cref="T:WCell.RealmServer.Quests.QuestLog" />
    /// </summary>
    [Persistent(8)]public string[] Details;

    /// <summary>
    /// Text which is QuestGiver going to say upon quest finishing.
    /// </summary>
    [Persistent(8)]public string[] EndTexts;

    /// <summary>
    /// Text which is displayed in quest objectives window once all objectives are completed
    /// </summary>
    [Persistent(8)]public string[] CompletedTexts;

    [NotPersistent]public QuestInteractionTemplate[] GOInteractions;
    [NotPersistent]public QuestInteractionTemplate[] NPCInteractions;
    [NotPersistent]public QuestInteractionTemplate[] SpellInteractions;

    /// <summary>
    /// Array of items you need to collect.
    /// If the items are quest-only,
    /// they will be deleted upon canceling quest.
    /// </summary>
    [Persistent(4)]public Asda2ItemStackDescription[] CollectableItems;

    /// <summary>Special Quest flags, unknown purpose.</summary>
    public QuestSpecialFlags SpecialFlags;

    /// <summary>
    /// Time limit for timed Quest. It's not taken into account
    /// if there's no Timed flag set in QuestFlags
    /// </summary>
    public uint TimeLimit;

    /// <summary>
    /// Text which will be shown when the objectives are done. In the
    /// offering rewards window.
    /// </summary>
    [Persistent(8)]public string[] OfferRewardTexts;

    /// <summary>
    /// Text which will be shown when the objectives aren't done yet. In the
    /// window where you have to have items.
    /// </summary>
    [Persistent(8)]public string[] ProgressTexts;

    /// <summary>
    /// Value indicating whether this <see cref="T:WCell.RealmServer.Quests.QuestTemplate" /> is repeatable.
    /// </summary>
    public bool Repeatable;

    /// <summary>
    /// Value indicating whether this <see cref="T:WCell.RealmServer.Quests.QuestTemplate" /> is available only for clients
    /// with expansion.
    /// probably obsolete, there is QuestFlag for this
    /// </summary>
    public ClientId RequiredClientId;

    /// <summary>Required minimal level to be able to see this quest.</summary>
    public uint RequiredLevel;

    /// <summary>Required race mask to check availability to player.</summary>
    public RaceMask RequiredRaces;

    /// <summary>Required class mask to check availability to player.</summary>
    public ClassMask RequiredClass;

    public int ReqSkillOrClass;
    public SkillId RequiredSkill;

    /// <summary>
    /// Tradeskill level which is required to accept this quest.
    /// </summary>
    public uint RequiredSkillValue;

    /// <summary>Represents the Reward XP column id.</summary>
    public int RewXPId;

    public int PreviousQuestId;
    public int NextQuestId;
    public int ExclusiveGroup;
    public uint FollowupQuestId;

    /// <summary>Number of players to kill</summary>
    public uint PlayersSlain;

    public uint RewHonorAddition;

    /// <summary>Multiplier of reward honor</summary>
    public float RewHonorMultiplier;

    public uint OfferRewardEmoteDelay;
    public EmoteType OfferRewardEmoteType;
    public uint RequestItemsEmoteDelay;
    public EmoteType RequestItemsEmoteType;
    public EmoteType RequestEmoteType;

    public event Action<Quest> QuestStarted;

    public event Action<Quest> QuestFinished;

    public event QuestCancelHandler QuestCancelled;

    public event QuestNPCHandler NPCInteracted;

    public event QuestGOHandler GOInteraction;

    /// <summary>
    /// Title (name) of the quest to be shown in <see cref="T:WCell.RealmServer.Quests.QuestLog" /> in the server's default language.
    /// </summary>
    public string DefaultTitle
    {
      get
      {
        if(Titles == null)
          return "[unknown Quest]";
        return Titles.LocalizeWithDefaultLocale();
      }
    }

    /// <summary>
    /// Objective of the quest to be shown in <see cref="T:WCell.RealmServer.Quests.QuestLog" /> in the server's default language.
    /// </summary>
    [NotPersistent]
    public string DefaultObjective
    {
      get { return Instructions.LocalizeWithDefaultLocale(); }
    }

    [NotPersistent]
    public string DefaultDetailText
    {
      get { return Details.LocalizeWithDefaultLocale(); }
    }

    [NotPersistent]
    public string DefaultEndText
    {
      get { return EndTexts.LocalizeWithDefaultLocale(); }
    }

    [NotPersistent]
    public string DefaultCompletedText
    {
      get { return CompletedTexts.LocalizeWithDefaultLocale(); }
    }

    public bool HasGOEvent
    {
      get
      {
        if(GOInteractions == null)
          return GOInteraction != null;
        return true;
      }
    }

    public bool HasObjectOrSpellInteractions { get; private set; }

    public bool RequiresSpellCasts { get; private set; }

    public bool HasNPCInteractionEvent
    {
      get
      {
        if(NPCInteracted != null)
          return true;
        if(NPCInteractions != null)
          return NPCInteractions.Length > 0;
        return false;
      }
    }

    [NotPersistent]
    public string DefaultOfferRewardText
    {
      get { return OfferRewardTexts.LocalizeWithDefaultLocale(); }
    }

    [NotPersistent]
    public string DefaultProgressText
    {
      get { return ProgressTexts.LocalizeWithDefaultLocale(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool ShouldBeConnectedInGraph
    {
      get
      {
        return (PreviousQuestId | NextQuestId | ExclusiveGroup |
                FollowupQuestId) != 0L;
      }
    }

    /// <summary>
    /// Value indicating whether this <see cref="T:WCell.RealmServer.Quests.QuestTemplate" /> is shareable.
    /// </summary>
    public bool Sharable
    {
      get { return Flags.HasFlag(QuestFlags.Sharable); }
    }

    /// <summary>
    /// Value indicating whether this <see cref="T:WCell.RealmServer.Quests.QuestTemplate" /> is daily.
    /// </summary>
    public bool IsDaily
    {
      get { return Flags.HasFlag(QuestFlags.Daily); }
    }

    /// <summary>
    /// To finish this Quest the Character has to interact with the given
    /// kind of GO the given amount of times. This is a unique interaction.
    /// To Add more than one GO for a particular objective add the first with
    /// this method then use <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddLinkedGOInteraction(System.UInt32,WCell.Constants.GameObjects.GOEntryId)" />
    /// </summary>
    /// <param name="goId">The entry id of the GO that must be interacted with</param>
    /// <param name="amount">The number of times this GO must be interacted with</param>
    /// <returns>The index of this template in the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.GOInteractions" /> array</returns>
    public int AddGOInteraction(GOEntryId goId, int amount, SpellId requiredSpell = SpellId.None)
    {
      int index;
      if(GOInteractions == null)
      {
        index = 0;
        GOInteractions = new QuestInteractionTemplate[1];
      }
      else
      {
        index = GOInteractions.Length;
        Array.Resize(ref GOInteractions, index + 1);
      }

      uint freeIndex = ObjectOrSpellInteractions.GetFreeIndex();
      QuestInteractionTemplate val = new QuestInteractionTemplate
      {
        Index = freeIndex,
        Amount = amount,
        RequiredSpellId = requiredSpell,
        ObjectType = ObjectTypeId.GameObject
      };
      val.TemplateId[0] = (uint) goId;
      ArrayUtil.Set(ref ObjectOrSpellInteractions, freeIndex, val);
      GOInteractions[index] = val;
      return index;
    }

    /// <summary>
    /// Adds an alternative GO to the interaction template that may also
    /// be interacted with for this quest objective <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.GOInteractions" />
    /// array where the first GO was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="goEntry">The entry id of the alternative GO that can be interacted with</param>
    public void AddLinkedGOInteraction(uint index, GOEntryId goEntry)
    {
      QuestInteractionTemplate interactionTemplate = GOInteractions.Get(index);
      if(interactionTemplate.TemplateId.Contains((uint) goEntry))
        return;
      Array.Resize(ref interactionTemplate.TemplateId, interactionTemplate.TemplateId.Length + 1);
      int num = (int) ArrayUtil.Add(ref interactionTemplate.TemplateId, (uint) goEntry);
    }

    /// <summary>
    /// Adds alternative GOs to the interaction template that may also
    /// be interacted with for this quest objective. <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.GOInteractions" /> array
    /// where the first GO was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="goids">The entry ids of the alternative GOs that can be interacted with</param>
    public void AddLinkedGOInteractions(uint index, IEnumerable<GOEntryId> goids)
    {
      QuestInteractionTemplate templ = NPCInteractions.Get(index);
      Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + goids.Count());
      foreach(GOEntryId goEntryId in goids.Where(npcId =>
        !templ.TemplateId.Contains((uint) npcId)))
      {
        int num = (int) ArrayUtil.Add(ref templ.TemplateId, (uint) goEntryId);
      }
    }

    /// <summary>
    /// Adds alternative GOs to the interaction template that may also
    /// be interacted with for this quest objective. <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.GOInteractions" /> array
    /// where the first GO was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddGOInteraction(WCell.Constants.GameObjects.GOEntryId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="goids">The entry ids of the alternative GOs that can be interacted with</param>
    public void AddLinkedGOInteractions(uint index, params GOEntryId[] goids)
    {
      QuestInteractionTemplate templ = NPCInteractions.Get(index);
      Array.Resize(ref templ.TemplateId,
        templ.TemplateId.Length + goids.Count());
      foreach(GOEntryId goEntryId in goids.Where(
        npcId =>
          !templ.TemplateId.Contains((uint) npcId)))
      {
        int num = (int) ArrayUtil.Add(ref templ.TemplateId, (uint) goEntryId);
      }
    }

    /// <summary>
    /// Returns the first QuestInteractionTemplate that requires the given NPC to be interacted with
    /// </summary>
    public QuestInteractionTemplate GetInteractionTemplateFor(GOEntryId goEntryId)
    {
      return GOInteractions
        .FirstOrDefault(interaction =>
          interaction.TemplateId.Contains((uint) goEntryId));
    }

    public void AddProvidedItem(Asda2ItemId id, int amount = 1)
    {
      ProvidedItems.Add(new Asda2ItemStackDescription(id, amount));
    }

    public void AddAreaTriggerObjective(uint id)
    {
      int num = (int) ArrayUtil.AddOnlyOne(ref AreaTriggerObjectives, id);
    }

    /// <summary>
    /// To finish this Quest the Character has to interact with the given
    /// kind of NPC the given amount of times. This is a unique interaction.
    /// To Add more than one NPC for a particular objective add the first with
    /// this method then use <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddLinkedNPCInteraction(System.UInt32,WCell.Constants.NPCs.NPCId)" />
    /// </summary>
    /// <param name="npcid">The entry id of the NPC that must be interacted with</param>
    /// <param name="amount">The number of times this NPC must be interacted with</param>
    /// <returns>The index of this template in the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.NPCInteractions" /> array</returns>
    public int AddNPCInteraction(NPCId npcid, int amount, SpellId requiredSpell = SpellId.None)
    {
      int index;
      if(NPCInteractions == null)
      {
        index = 0;
        NPCInteractions = new QuestInteractionTemplate[1];
      }
      else
      {
        index = NPCInteractions.Length;
        Array.Resize(ref NPCInteractions, index + 1);
      }

      uint freeIndex = ObjectOrSpellInteractions.GetFreeIndex();
      QuestInteractionTemplate val = new QuestInteractionTemplate
      {
        Index = freeIndex,
        Amount = amount,
        RequiredSpellId = requiredSpell,
        ObjectType = ObjectTypeId.GameObject
      };
      val.TemplateId[0] = (uint) npcid;
      ArrayUtil.Set(ref ObjectOrSpellInteractions, freeIndex, val);
      NPCInteractions[index] = val;
      return index;
    }

    /// <summary>
    /// Adds an alternative NPC to the interaction template that may also
    /// be interacted with for this quest objective. <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.NPCInteractions" /> array
    /// where the first NPC was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="npcid">The entry id of the alternative NPC that can be interacted with</param>
    public void AddLinkedNPCInteraction(uint index, NPCId npcid)
    {
      QuestInteractionTemplate interactionTemplate = NPCInteractions.Get(index);
      if(interactionTemplate.TemplateId.Contains((uint) npcid))
        return;
      Array.Resize(ref interactionTemplate.TemplateId, interactionTemplate.TemplateId.Length + 1);
      int num = (int) ArrayUtil.Add(ref interactionTemplate.TemplateId, (uint) npcid);
    }

    /// <summary>
    /// Adds alternative NPCs to the interaction template that may also
    /// be interacted with for this quest objective. <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.NPCInteractions" /> array
    /// where the first NPC was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="npcids">The entry ids of the alternative NPCs that can be interacted with</param>
    public void AddLinkedNPCInteractions(uint index, IEnumerable<NPCId> npcids)
    {
      QuestInteractionTemplate templ = NPCInteractions.Get(index);
      Array.Resize(ref templ.TemplateId, templ.TemplateId.Length + npcids.Count());
      foreach(NPCId npcId in npcids.Where(npcId =>
        !templ.TemplateId.Contains((uint) npcId)))
      {
        int num = (int) ArrayUtil.Add(ref templ.TemplateId, (uint) npcId);
      }
    }

    /// <summary>
    /// Adds alternative NPCs to the interaction template that may also
    /// be interacted with for this quest objective. <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" />
    /// </summary>
    /// <param name="index">The index into the <see cref="F:WCell.RealmServer.Quests.QuestTemplate.NPCInteractions" /> array
    /// where the first NPC was added with <seealso cref="M:WCell.RealmServer.Quests.QuestTemplate.AddNPCInteraction(WCell.Constants.NPCs.NPCId,System.Int32,WCell.Constants.Spells.SpellId)" /></param>
    /// <param name="npcids">The entry ids of the alternative NPCs that can be interacted with</param>
    public void AddLinkedNPCInteractions(uint index, params NPCId[] npcids)
    {
      QuestInteractionTemplate templ = NPCInteractions.Get(index);
      Array.Resize(ref templ.TemplateId,
        templ.TemplateId.Length + npcids.Count());
      foreach(NPCId npcId in npcids.Where(npcId =>
        !templ.TemplateId.Contains((uint) npcId)))
      {
        int num = (int) ArrayUtil.Add(ref templ.TemplateId, (uint) npcId);
      }
    }

    /// <summary>
    /// Returns the first QuestInteractionTemplate that requires the given NPC to be interacted with
    /// </summary>
    public QuestInteractionTemplate GetInteractionTemplateFor(NPCId npcId)
    {
      return NPCInteractions
        .FirstOrDefault(interaction =>
          interaction.TemplateId.Contains((uint) npcId));
    }

    /// <summary>Checks whether the given Character may do this Quest</summary>
    public QuestInvalidReason CheckBasicRequirements(Character chr)
    {
      if(RequiredRaces != ~RaceMask.AllRaces1 && !RequiredRaces.HasAnyFlag(chr.RaceMask))
        return QuestInvalidReason.WrongRace;
      if(RequiredClass != ClassMask.None && !RequiredClass.HasAnyFlag(chr.ClassMask))
        return QuestInvalidReason.WrongClass;
      if(RequiredSkill != SkillId.None && RequiredSkill > SkillId.None &&
         !chr.Skills.CheckSkill(RequiredSkill, (int) RequiredSkillValue))
        return QuestInvalidReason.NoRequirements;
      QuestInvalidReason questInvalidReason1 = CheckRequiredActiveQuests(chr.QuestLog);
      if(questInvalidReason1 != QuestInvalidReason.Ok)
        return questInvalidReason1;
      QuestInvalidReason questInvalidReason2 = CheckRequiredFinishedQuests(chr.QuestLog);
      if(questInvalidReason2 != QuestInvalidReason.Ok)
        return questInvalidReason2;
      if(IsDaily && chr.QuestLog.CurrentDailyCount >= 25U)
        return QuestInvalidReason.TooManyDailys;
      if(chr.Account.ClientId < RequiredClientId)
        return QuestInvalidReason.NoExpansionAccount;
      if(chr.QuestLog.TimedQuestSlot != null)
        return QuestInvalidReason.AlreadyOnTimedQuest;
      if(chr.Level < RequiredLevel)
        return QuestInvalidReason.LowLevel;
      if(RewMoney < 0 && chr.Money < -RewMoney)
        return QuestInvalidReason.NotEnoughMoney;
      return EventIds.Count != 0 &&
             !EventIds.Where(WorldEventMgr.IsEventActive).Any()
        ? QuestInvalidReason.NoRequirements
        : QuestInvalidReason.Ok;
    }

    /// <summary>Check quest-relation requirements of active quests</summary>
    private QuestInvalidReason CheckRequiredActiveQuests(QuestLog log)
    {
      for(int index = 0; index < ReqAllActiveQuests.Count; ++index)
      {
        uint reqAllActiveQuest = ReqAllActiveQuests[index];
        if(!log.HasActiveQuest(reqAllActiveQuest))
          return QuestInvalidReason.NoRequirements;
      }

      if(ReqAnyActiveQuests.Count > 0)
      {
        bool flag = false;
        for(int index = 0; index < ReqAnyActiveQuests.Count; ++index)
        {
          uint reqAnyActiveQuest = ReqAnyActiveQuests[index];
          if(log.HasActiveQuest(reqAnyActiveQuest))
          {
            flag = true;
            break;
          }
        }

        if(!flag)
          return QuestInvalidReason.NoRequirements;
      }

      return QuestInvalidReason.Ok;
    }

    /// <summary>
    /// Check quest-relation requirements of quests that need to be finished for this one to start
    /// </summary>
    private QuestInvalidReason CheckRequiredFinishedQuests(QuestLog log)
    {
      for(int index = 0; index < ReqAllFinishedQuests.Count; ++index)
      {
        uint allFinishedQuest = ReqAllFinishedQuests[index];
        if(!log.FinishedQuests.Contains(allFinishedQuest))
          return QuestInvalidReason.NoRequirements;
      }

      if(ReqAnyActiveQuests.Count > 0)
      {
        bool flag = false;
        for(int index = 0; index < ReqAnyFinishedQuests.Count; ++index)
        {
          uint anyFinishedQuest = ReqAnyFinishedQuests[index];
          if(log.FinishedQuests.Contains(anyFinishedQuest))
          {
            flag = true;
            break;
          }
        }

        if(!flag)
          return QuestInvalidReason.NoRequirements;
      }

      if(ReqUndoneQuests.Count > 0)
      {
        for(int index = 0; index < ReqUndoneQuests.Count; ++index)
        {
          uint reqUndoneQuest = ReqUndoneQuests[index];
          if(log.FinishedQuests.Contains(reqUndoneQuest) || log.HasActiveQuest(reqUndoneQuest))
            return QuestInvalidReason.NoRequirements;
        }
      }

      return QuestInvalidReason.Ok;
    }

    /// <summary>
    /// Determines whether is quest obsolete for given character.
    /// </summary>
    /// <param name="chr">The character.</param>
    /// <returns>
    /// 	<c>true</c> if [is quest obsolete] [the specified qt]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsObsolete(Character chr)
    {
      return chr.Level >= RequiredLevel + QuestMgr.LevelObsoleteOffset;
    }

    /// <summary>
    /// Determines whether [is quest too high level] [the specified qt].
    /// </summary>
    /// <param name="chr">The CHR.</param>
    /// <returns>
    /// 	<c>true</c> if [is quest too high level] [the specified qt]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTooHighLevel(Character chr)
    {
      return chr.Level + QuestMgr.LevelRequirementOffset < RequiredLevel;
    }

    /// <summary>
    /// Checks the requirements and returns the QuestStatus for ending a Quest.
    /// </summary>
    public QuestStatus GetStartStatus(QuestHolderInfo qh, Character chr)
    {
      if(chr.QuestLog.GetActiveQuest(Id) != null ||
         !Repeatable && chr.QuestLog.FinishedQuests.Contains(Id))
        return QuestStatus.NotAvailable;
      switch(CheckBasicRequirements(chr))
      {
        case QuestInvalidReason.LowLevel:
          return QuestStatus.TooHighLevel;
        case QuestInvalidReason.Ok:
          if(Repeatable)
            return QuestStatus.Repeatable;
          return !IsObsolete(chr) ? QuestStatus.Available : QuestStatus.Obsolete;
        default:
          return QuestStatus.NotAvailable;
      }
    }

    /// <summary>
    /// </summary>
    public QuestStatus GetAvailability(Character chr)
    {
      if(CheckBasicRequirements(chr) == QuestInvalidReason.LowLevel)
        return QuestStatus.TooHighLevel;
      if(IsObsolete(chr))
        return !Repeatable ? QuestStatus.Obsolete : QuestStatus.Repeatable;
      return !Repeatable ? QuestStatus.Available : QuestStatus.Repeatable;
    }

    /// <summary>
    /// Checks the requirements and returns the QuestStatus for ending a Quest.
    /// </summary>
    /// <param name="chr">The client.</param>
    /// <returns></returns>
    public QuestStatus GetEndStatus(Character chr)
    {
      Quest activeQuest = chr.QuestLog.GetActiveQuest(Id);
      if(activeQuest == null)
        return QuestStatus.NotAvailable;
      return activeQuest.Status;
    }

    /// <summary>Returns the GOEntry with the given id or null</summary>
    public GOEntry GetStarter(GOEntryId id)
    {
      for(int index = 0; index < Starters.Count; ++index)
      {
        IQuestHolderEntry starter = Starters[index];
        if(starter is GOEntry && (GOEntryId) starter.Id == id)
          return (GOEntry) starter;
      }

      return null;
    }

    /// <summary>Returns the NPCEntry with the given id or null</summary>
    public NPCEntry GetStarter(NPCId id)
    {
      for(int index = 0; index < Starters.Count; ++index)
      {
        IQuestHolderEntry starter = Starters[index];
        if(starter is NPCEntry && (NPCId) starter.Id == id)
          return (NPCEntry) starter;
      }

      return null;
    }

    /// <summary>Returns the ItemTemplate with the given id or null</summary>
    public ItemTemplate GetStarter(Asda2ItemId id)
    {
      for(int index = 0; index < Starters.Count; ++index)
      {
        IQuestHolderEntry starter = Starters[index];
        if(starter is ItemTemplate && (Asda2ItemId) starter.Id == id)
          return (ItemTemplate) starter;
      }

      return null;
    }

    /// <summary>
    /// Returns the Starter of the given Type which has the given Id
    /// </summary>
    public T GetStarter<T>(uint id) where T : IQuestHolderEntry
    {
      for(int index = 0; index < Starters.Count; ++index)
      {
        IQuestHolderEntry starter = Starters[index];
        if(starter is T && (int) starter.Id == (int) id)
          return (T) starter;
      }

      return default(T);
    }

    /// <summary>Returns the GOEntry with the given id or null</summary>
    public GOEntry GetFinisher(GOEntryId id)
    {
      for(int index = 0; index < Finishers.Count; ++index)
      {
        IQuestHolderEntry finisher = Finishers[index];
        if(finisher is GOEntry && (GOEntryId) finisher.Id == id)
          return (GOEntry) finisher;
      }

      return null;
    }

    /// <summary>Returns the NPCEntry with the given id or null</summary>
    public NPCEntry GetFinisher(NPCId id)
    {
      for(int index = 0; index < Finishers.Count; ++index)
      {
        IQuestHolderEntry finisher = Finishers[index];
        if(finisher is NPCEntry && (NPCId) finisher.Id == id)
          return (NPCEntry) finisher;
      }

      return null;
    }

    /// <summary>
    /// Returns the Finisher of the given Type which has the given Id
    /// </summary>
    public T GetFinisher<T>(uint id) where T : IQuestHolderEntry
    {
      for(int index = 0; index < Finishers.Count; ++index)
      {
        IQuestHolderEntry finisher = Finishers[index];
        if(finisher is T && (int) finisher.Id == (int) id)
          return (T) finisher;
      }

      return default(T);
    }

    /// <summary>Tries to give all Initial Items (or none at all).</summary>
    /// <remarks>If not all Initial Items could be given, the Quest cannot be started.</remarks>
    /// <param name="receiver"></param>
    /// <returns>Whether initial Items were given.</returns>
    public bool GiveInitialItems(Character receiver)
    {
      return true;
    }

    /// <summary>Tries to give all Rewards (or none at all).</summary>
    /// <remarks>If not all Rewards could be given, the Quest remains completable.</remarks>
    /// <param name="receiver"></param>
    /// <param name="qHolder"></param>
    /// <param name="rewardSlot">The slot of choosable items</param>
    /// <returns>Whether Rewards were given.</returns>
    public bool TryGiveRewards(Character receiver, IQuestHolder qHolder, uint rewardSlot)
    {
      if(RewMoney >= 0 || receiver.Money - RewMoney >= 0L)
        return GiveRewards(receiver, rewardSlot);
      QuestHandler.SendRequestItems(qHolder, this, receiver, true);
      return false;
    }

    public bool GiveRewards(Character receiver, uint rewardSlot)
    {
      return true;
    }

    public int CalcRewRep(int valueId, int value)
    {
      if(value != 0)
        return value * 100;
      int index = valueId > 0 ? 0 : 1;
      return QuestMgr.QuestRewRepInfos[index].RewRep[valueId - 1];
    }

    public int CalcRewardHonor(Character character)
    {
      int num = 0;
      if(RewHonorAddition > 0U || RewHonorMultiplier > 0.0)
      {
        QuestHonorInfo questHonorInfo = QuestMgr.QuestHonorInfos.Get(Level);
        if(questHonorInfo != null)
          num = (int) (questHonorInfo.RewHonor * (double) RewHonorMultiplier *
                       0.100000001490116) + (int) RewHonorAddition;
      }

      return num;
    }

    public int CalcRewardXp(Character character)
    {
      QuestXPInfo questXpInfo = QuestMgr.QuestXpInfos.Get(Level);
      int num = (questXpInfo == null
                  ? (int) MinLevel * 100
                  : questXpInfo.RewXP.Get((uint) (RewXPId - 1))) *
                character.QuestExperienceGainModifierPercent / 100;
      int level = character.Level;
      if(level <= Level + 5U)
        return num;
      if(level == Level + 6U)
        return num * 8 / 10;
      if(level == Level + 7U)
        return num * 6 / 10;
      if(level == Level + 8U)
        return num * 4 / 10;
      if(level == Level + 9U)
        return num / 5;
      return num / 10;
    }

    public void Dump(IndentTextWriter writer)
    {
      writer.WriteLineNotDefault(QuestType, "Type: " + QuestType);
      writer.WriteLineNotDefault(Flags, "Flags: " + Flags);
      writer.WriteLineNotDefault(RequiredLevel, "RequiredLevel: " + RequiredLevel);
      writer.WriteLineNotDefault(RequiredRaces, "Races: " + RequiredRaces);
      writer.WriteLineNotDefault(RequiredClass, "Class: " + RequiredClass);
      writer.WriteLineNotDefault(ProvidedItems.Count,
        "ProvidedItems: " + ProvidedItems.ToString(", "));
      writer.WriteLineNotDefault(Starters.Count,
        "Starters: " + Starters.ToString(", "));
      writer.WriteLineNotDefault(Finishers.Count,
        "Finishers: " + Finishers.ToString(", "));
      List<QuestInteractionTemplate> list =
        ObjectOrSpellInteractions
          .Where(action =>
          {
            if(action != null)
              return action.TemplateId[0] != 0U;
            return false;
          }).ToList();
      writer.WriteLineNotDefault(list.Count(),
        "Interactions: " + list.ToString(", "));
      if(CollectableItems != null && CollectableItems.Length > 0)
        writer.WriteLine("Collectables: " + CollectableItems
                           .ToString(", "));
      writer.WriteLineNotDefault(AreaTriggerObjectives.Length,
        "Req AreaTriggers: " +
        AreaTriggerObjectives
          .TransformArray(id => AreaTriggerMgr.GetTrigger(id))
          .ToString(", "));
      if(Instructions != null)
      {
        IEnumerable<string> strings =
          Instructions.Where(
            obj => !string.IsNullOrEmpty(obj));
        writer.WriteLineNotDefault(strings.Count(),
          "Instructions: " + strings.ToString(" / "));
      }

      if(!ShouldBeConnectedInGraph)
        return;
      writer.WriteLine();
      writer.WriteLine("PreviousQuestId: {0}, NextQuestId: {1}, ExclusiveGroup: {2}, FollowupQuestId: {3} ",
        (object) PreviousQuestId, (object) NextQuestId, (object) ExclusiveGroup,
        (object) FollowupQuestId);
      writer.WriteLineNotDefault(ReqAllActiveQuests.Count,
        "ReqAllActiveQuests: " + MakeQuestString(ReqAllActiveQuests));
      writer.WriteLineNotDefault(ReqAllFinishedQuests.Count,
        "ReqAllFinishedQuests: " + MakeQuestString(ReqAllFinishedQuests));
      writer.WriteLineNotDefault(ReqAnyActiveQuests.Count,
        "ReqAnyActiveQuests: " + MakeQuestString(ReqAnyActiveQuests));
      writer.WriteLineNotDefault(ReqAnyFinishedQuests.Count,
        "ReqAnyFinishedQuests: " + MakeQuestString(ReqAnyFinishedQuests));
      writer.WriteLineNotDefault(ReqUndoneQuests.Count,
        "ReqUndoneQuests: " + MakeQuestString(ReqUndoneQuests));
    }

    private string MakeQuestString(IEnumerable<uint> questIds)
    {
      return Utility.GetStringRepresentation(
        questIds.Select(QuestMgr.GetTemplate));
    }

    public override string ToString()
    {
      return DefaultTitle + " (Id: " + Id + ")";
    }

    internal void NotifyStarted(Quest quest)
    {
      Action<Quest> questStarted = QuestStarted;
      if(questStarted == null)
        return;
      questStarted(quest);
    }

    internal void NotifyFinished(Quest quest)
    {
      Action<Quest> questFinished = QuestFinished;
      if(questFinished == null)
        return;
      questFinished(quest);
    }

    internal void NotifyCancelled(Quest quest, bool failed)
    {
      QuestCancelHandler questCancelled = QuestCancelled;
      if(questCancelled == null)
        return;
      questCancelled(quest, failed);
    }

    internal void NotifyNPCInteracted(Quest quest, NPC npc)
    {
      QuestNPCHandler npcInteracted = NPCInteracted;
      if(npcInteracted == null)
        return;
      npcInteracted(quest, npc);
    }

    internal void NotifyGOUsed(Quest quest, GameObject go)
    {
      QuestGOHandler goInteraction = GOInteraction;
      if(goInteraction == null)
        return;
      goInteraction(quest, go);
    }

    public static IEnumerable<QuestTemplate> GetAllDataHolders()
    {
      return QuestMgr.Templates;
    }

    public void FinalizeDataHolder()
    {
      if(ReqSkillOrClass > 0)
        RequiredSkill = (SkillId) ReqSkillOrClass;
      else if(ReqSkillOrClass < 0)
        RequiredClass = (ClassMask) (-ReqSkillOrClass);
      if(Category >= 0 && Category > 0)
        ZoneTemplate = World.GetZoneInfo((ZoneId) Category);
      List<QuestInteractionTemplate> list1 = null;
      List<QuestInteractionTemplate> list2 = null;
      for(uint index = 0; (long) index < (long) ObjectOrSpellInteractions.Length; ++index)
      {
        QuestInteractionTemplate spellInteraction = ObjectOrSpellInteractions[index];
        if(spellInteraction != null && spellInteraction.IsValid)
        {
          HasObjectOrSpellInteractions = true;
          if(spellInteraction.RequiredSpellId != SpellId.None)
          {
            RequiresSpellCasts = true;
            if(SpellInteractions == null)
            {
              SpellInteractions = new QuestInteractionTemplate[1]
              {
                spellInteraction
              };
            }
            else
            {
              int num = (int) ArrayUtil.AddOnlyOne(ref SpellInteractions,
                spellInteraction);
            }
          }
          else if(spellInteraction.ObjectType == ObjectTypeId.GameObject)
            (list1 = list1.NotNull()).Add(spellInteraction);
          else
            (list2 = list2.NotNull()).Add(spellInteraction);

          spellInteraction.Index = index;
        }
      }

      List<Asda2ItemStackDescription> stackDescriptionList = new List<Asda2ItemStackDescription>(4);
      for(int index = 0; index < CollectableItems.Length; ++index)
      {
        Asda2ItemStackDescription item = CollectableItems[index];
        if(item.ItemId != 0 &&
           ProvidedItems
             .Find(stack => stack.ItemId == item.ItemId).ItemId ==
           0)
          stackDescriptionList.Add(item);
      }

      CollectableItems = stackDescriptionList.ToArray();
      for(int index = 0; index < CollectableSourceItems.Length; ++index)
      {
        if(CollectableSourceItems[index].ItemId != Asda2ItemId.None &&
           CollectableSourceItems[index].Amount == 0)
          CollectableSourceItems[index].Amount = 1;
      }

      if(list1 != null)
        GOInteractions = list1.ToArray();
      if(list2 != null)
        NPCInteractions = list2.ToArray();
      foreach(WorldEventQuest worldEventQuest in WorldEventMgr.WorldEventQuests.Where(
        worldEventQuest => (int) worldEventQuest.QuestId == (int) Id))
        EventIds.Add(worldEventQuest.EventId);
      ArrayUtil.PruneVals(ref AreaTriggerObjectives);
      ArrayUtil.PruneVals(ref RewardChoiceItems);
      ArrayUtil.PruneVals(ref RewardItems);
      QuestMgr.AddQuest(this);
    }

    public delegate void QuestNPCHandler(Quest quest, NPC npc);

    public delegate void QuestGOHandler(Quest quest, GameObject go);

    public delegate void QuestCancelHandler(Quest quest, bool failed);
  }
}