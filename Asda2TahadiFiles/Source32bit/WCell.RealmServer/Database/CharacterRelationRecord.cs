using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Database
{
  /// <summary>
  /// Represents a character relationship entry in the database
  /// </summary>
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class CharacterRelationRecord : WCellRecord<CharacterRelationRecord>
  {
    private static readonly NHIdGenerator _idGenerator =
      new NHIdGenerator(typeof(CharacterRelationRecord), nameof(CharacterRelationGuid), 1L);

    [Field("CharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private long _characterId;

    [Field("RelatedCharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private long _relatedCharacterId;

    /// <summary>Returns the next unique Id for a new SpellRecord</summary>
    public static long NextId()
    {
      return _idGenerator.Next();
    }

    public CharacterRelationRecord()
    {
    }

    public CharacterRelationRecord(uint charId, uint relatedCharId, CharacterRelationType type)
    {
      State = RecordState.New;
      CharacterId = charId;
      RelatedCharacterId = relatedCharId;
      RelationType = type;
      CharacterRelationGuid = NextId();
    }

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public long CharacterRelationGuid { get; set; }

    public uint CharacterId
    {
      get { return (uint) _characterId; }
      set { _characterId = value; }
    }

    public uint RelatedCharacterId
    {
      get { return (uint) _relatedCharacterId; }
      set { _relatedCharacterId = value; }
    }

    [Property(NotNull = true)]
    public CharacterRelationType RelationType { get; set; }

    [Property]
    public string Note { get; set; }
  }
}