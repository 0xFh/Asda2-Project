using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.RacesClasses
{
  public static class ArchetypeMgr
  {
    private static readonly Func<BaseClass>[] ClassCreators = new Func<BaseClass>[WCellConstants.ClassTypeLength];

    /// <summary>Use Archetypes for any customizations.</summary>
    internal static readonly BaseClass[] BaseClasses = new BaseClass[(WCellConstants.ClassTypeLength + 20U)];

    /// <summary>Use Archetypes for any customizations.</summary>
    internal static readonly BaseRace[] BaseRaces = new BaseRace[(WCellConstants.RaceTypeLength + 20U)];

    /// <summary>
    /// Use Archetype objects to customize basic settings.
    /// Index: [class][race]
    /// </summary>
    public static readonly Archetype[][] Archetypes = new Archetype[WCellConstants.ClassTypeLength][];

    private static bool loaded;

    internal static BaseClass CreateClass(ClassId id)
    {
      return ClassCreators[(int) id]();
    }

    /// <summary>Returns the Class with the given type</summary>
    public static BaseClass GetClass(ClassId id)
    {
      if((long) id < BaseClasses.Length)
        return BaseClasses[(uint) id];
      return null;
    }

    /// <summary>Returns the Race with the given type</summary>
    public static BaseRace GetRace(RaceId id)
    {
      if((uint) id < BaseRaces.Length)
        return BaseRaces[(uint) id];
      return null;
    }

    /// <summary>
    /// Returns the corresponding <see cref="T:WCell.RealmServer.RacesClasses.Archetype" />.
    /// </summary>
    /// <exception cref="T:System.NullReferenceException">If Archetype does not exist</exception>
    public static Archetype GetArchetypeNotNull(RaceId race, ClassId clss)
    {
      Archetype archetype;
      if(clss >= (ClassId) WCellConstants.ClassTypeLength || (uint) race >= WCellConstants.RaceTypeLength ||
         (archetype = Archetypes[(uint) clss][(uint) race]) == null)
        throw new ArgumentException(string.Format("Archetype \"{0} {1}\" does not exist.", race,
          clss));
      return archetype;
    }

    public static Archetype GetArchetype(RaceId race, ClassId clssId)
    {
      if(clssId >= (ClassId) WCellConstants.ClassTypeLength || (uint) race >= WCellConstants.RaceTypeLength)
        return null;
      return Archetypes[(uint) clssId]?[(uint) race];
    }

    /// <summary>
    /// Returns all archetypes with the given race/class combination.
    /// 0 for race or class means all.
    /// </summary>
    /// <returns></returns>
    public static List<Archetype> GetArchetypes(RaceId race, ClassId clss)
    {
      if(clss >= (ClassId) WCellConstants.ClassTypeLength || (uint) race >= WCellConstants.RaceTypeLength)
        return null;
      List<Archetype> archetypeList = new List<Archetype>();
      if(clss == ClassId.NoClass)
      {
        foreach(Archetype[] archetype1 in Archetypes)
        {
          if(archetype1 != null)
          {
            if(race == RaceId.None)
            {
              foreach(Archetype archetype2 in archetype1)
              {
                if(archetype2 != null)
                  archetypeList.Add(archetype2);
              }
            }
            else if(archetype1[(uint) race] != null)
              archetypeList.Add(archetype1[(uint) race]);
          }
        }
      }
      else if(race == RaceId.None)
      {
        foreach(Archetype archetype in Archetypes[(uint) clss])
        {
          if(archetype != null)
            archetypeList.Add(archetype);
        }
      }
      else if(Archetypes[(uint) clss][(uint) race] != null)
        archetypeList.Add(Archetypes[(uint) clss][(uint) race]);

      if(archetypeList.Count == 0)
        return null;
      return archetypeList;
    }

    static ArchetypeMgr()
    {
      for(int index = 0; index < Archetypes.Length; ++index)
        Archetypes[index] = new Archetype[WCellConstants.RaceTypeLength];
    }

    public static void EnsureInitialize()
    {
      if(ClassCreators[1] != null)
        return;
      Initialize();
    }

    /// <summary>
    /// Note: This step is depending on Skills, Spells and WorldMgr
    /// </summary>
    [Initialization(InitializationPass.Seventh, "Initializing Races and Classes")]
    public static void Initialize()
    {
      if(loaded)
        return;
      InitClasses();
      InitRaces();
      ContentMgr.Load<Archetype>();
      if(ItemMgr.Loaded)
        LoadItems();
      for(int index = 0; index < SpellLines.SpellLinesByClass.Length; ++index)
      {
        SpellLine[] spellLineArray = SpellLines.SpellLinesByClass[index];
        if(spellLineArray != null)
        {
          BaseClass baseClass = GetClass((ClassId) index);
          if(baseClass != null)
            baseClass.SpellLines = spellLineArray;
        }
      }

      loaded = true;
    }

    private static void InitClasses()
    {
      AddClass(new OHSClass());
      AddClass(new SpearClass());
      AddClass(new THSClass());
      AddClass(new CrossbowClass());
      AddClass(new BowClass());
      AddClass(new BalistatClass());
      AddClass(new AtackMageClass());
      AddClass(new SupportMageClass());
      AddClass(new HealMageClass());
      AddClass(new NoviceClass());
    }

    private static void AddClass(BaseClass clss)
    {
      BaseClasses[(int) clss.Id] = clss;
    }

    private static void InitRaces()
    {
      new BaseRace(RaceId.Human).FinalizeAfterLoad();
    }

    public static bool Loaded
    {
      get { return loaded; }
    }

    public static void LoadItems()
    {
    }
  }
}