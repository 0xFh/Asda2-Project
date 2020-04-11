using Castle.ActiveRecord;
using WCell.Constants.Spells;
using WCell.Core.Database;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database
{
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class SpellRecord : WCellRecord<SpellRecord>
  {
    private static readonly NHIdGenerator _idGenerator =
      new NHIdGenerator(typeof(SpellRecord), nameof(RecordId), 1L);

    public const int NoSpecIndex = -1;

    [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_spellId;

    [Field("OwnerId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_ownerId;

    /// <summary>Returns the next unique Id for a new SpellRecord</summary>
    public static long NextId()
    {
      return _idGenerator.Next();
    }

    public static SpellRecord[] LoadAllRecordsFor(uint lowId)
    {
      return FindAllByProperty("m_ownerId", (int) lowId);
    }

    private SpellRecord()
    {
    }

    public SpellRecord(SpellId id, uint ownerId, int specIndex)
    {
      SpellId = id;
      OwnerId = ownerId;
      SpecIndex = specIndex;
      RecordId = NextId();
      State = RecordState.New;
    }

    [PrimaryKey(PrimaryKeyType.Assigned, "SpellRecordId")]
    public long RecordId { get; set; }

    public uint OwnerId
    {
      get { return (uint) m_ownerId; }
      set { m_ownerId = (int) value; }
    }

    public SpellId SpellId
    {
      get { return (SpellId) m_spellId; }
      set { m_spellId = (int) value; }
    }

    public Spell Spell
    {
      get { return SpellHandler.Get(SpellId); }
    }

    [Property]
    public int SpecIndex { get; set; }

    public bool MatchesSpec(int index)
    {
      if(SpecIndex != index)
        return index == -1;
      return true;
    }

    public override string ToString()
    {
      return m_spellId + " (" + SpellId + ")";
    }
  }
}