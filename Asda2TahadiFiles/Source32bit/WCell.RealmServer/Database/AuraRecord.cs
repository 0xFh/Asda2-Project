using Castle.ActiveRecord;
using NLog;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Database
{
  [ActiveRecord(Access = PropertyAccess.Property)]
  public class AuraRecord : WCellRecord<AuraRecord>
  {
    internal static readonly ObjectPool<AuraRecord> AuraRecordPool =
      new ObjectPool<AuraRecord>(() => new AuraRecord());

    private static readonly NHIdGenerator
      _idGenerator = new NHIdGenerator(typeof(AuraRecord), nameof(RecordId), 1L);

    [Field("OwnerId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
    private int m_OwnerId;

    private Spell m_spell;

    /// <summary>Returns the next unique Id for a new SpellRecord</summary>
    public static long NextId()
    {
      return _idGenerator.Next();
    }

    public static AuraRecord ObtainAuraRecord(Aura aura)
    {
      AuraRecord auraRecord = AuraRecordPool.Obtain();
      auraRecord.State = RecordState.New;
      auraRecord.RecordId = NextId();
      auraRecord.SyncData(aura);
      return auraRecord;
    }

    public static AuraRecord[] LoadAuraRecords(uint lowId)
    {
      return FindAllByProperty("m_OwnerId", (int) lowId);
    }

    public AuraRecord(Aura aura)
    {
      State = RecordState.New;
      RecordId = NextId();
      SyncData(aura);
    }

    public AuraRecord()
    {
    }

    public void SyncData(Aura aura)
    {
      OwnerId = aura.Auras.Owner.EntityId.Low;
      CasterId = (long) aura.CasterReference.EntityId.Full;
      Level = aura.Level;
      m_spell = aura.Spell;
      MillisLeft = !aura.HasTimeout ? -1 : aura.TimeLeft;
      StackCount = aura.StackCount;
      IsBeneficial = aura.IsBeneficial;
    }

    [PrimaryKey(PrimaryKeyType.Assigned)]
    public long RecordId { get; set; }

    public uint OwnerId
    {
      get { return (uint) m_OwnerId; }
      set { m_OwnerId = (int) value; }
    }

    [Property]
    public long CasterId { get; set; }

    [Property]
    public int Level { get; set; }

    [Property]
    public int SpellId
    {
      get { return (int) m_spell.Id; }
      set
      {
        m_spell = SpellHandler.Get((uint) value);
        if(m_spell != null)
          return;
        LogManager.GetCurrentClassLogger().Warn("Aura record {0} has invalid SpellId {1}",
          RecordId, value);
      }
    }

    public Spell Spell
    {
      get { return m_spell; }
    }

    [Property]
    public int MillisLeft { get; set; }

    [Property]
    public int StackCount { get; set; }

    [Property]
    public bool IsBeneficial { get; set; }

    public ObjectReference GetCasterInfo(Map map)
    {
      EntityId entityId = new EntityId((ulong) CasterId);
      WorldObject worldObject = map.GetObject(entityId);
      if(worldObject != null)
        return worldObject.SharedReference;
      return new ObjectReference(entityId, Level);
    }

    public override void Delete()
    {
      base.Delete();
      AuraRecordPool.Recycle(this);
    }

    internal void Recycle()
    {
      AuraRecordPool.Recycle(this);
    }
  }
}