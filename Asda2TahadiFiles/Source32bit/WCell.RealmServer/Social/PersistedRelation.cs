using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Interaction
{
  /// <summary>
  /// Represents a relationship between two <see cref="T:WCell.RealmServer.Entities.Character" /> entities, persisted in the db.
  /// </summary>
  public abstract class PersistedRelation : BaseRelation
  {
    private readonly CharacterRelationRecord m_charRelationRecord;

    public override uint CharacterId
    {
      get { return m_charRelationRecord.CharacterId; }
      set { m_charRelationRecord.CharacterId = value; }
    }

    public override uint RelatedCharacterId
    {
      get { return m_charRelationRecord.RelatedCharacterId; }
      set { m_charRelationRecord.RelatedCharacterId = value; }
    }

    public override string Note
    {
      get { return m_charRelationRecord.Note; }
      set { m_charRelationRecord.Note = value; }
    }

    /// <summary>Default constructor</summary>
    public PersistedRelation()
    {
      m_charRelationRecord = new CharacterRelationRecord();
    }

    /// <summary>
    /// Creates a new character relation based on the chars EntityId
    /// </summary>
    public PersistedRelation(uint charId, uint relatedCharId)
    {
      m_charRelationRecord = new CharacterRelationRecord(charId, relatedCharId, Type);
    }

    /// <summary>
    /// Creates a new character relation based on a <see cref="T:WCell.RealmServer.Database.CharacterRelationRecord" />
    /// </summary>
    protected PersistedRelation(CharacterRelationRecord relation)
    {
      m_charRelationRecord = relation;
    }

    /// <summary>Saves this instance to the DB</summary>
    public virtual void SaveToDB()
    {
      m_charRelationRecord.Save();
    }

    /// <summary>Delete this instance from the database</summary>
    public virtual void Delete()
    {
      m_charRelationRecord.Delete();
    }

    /// <summary>Retrieves the list of character relations</summary>
    /// <returns>The list of all characters relations.</returns>
    public static BaseRelation[] GetAll()
    {
      return ActiveRecordBase<CharacterRelationRecord>.FindAll()
        .Select(
          crr => RelationMgr.CreateRelation(crr))
        .ToArray();
    }

    /// <summary>
    /// Retrieves the list of character relations of a character
    /// </summary>
    /// <param name="charLowId">The character Id</param>
    /// <returns>The list of relations of the character.</returns>
    public static BaseRelation[] GetByCharacterId(uint charLowId)
    {
      return ActiveRecordBase<CharacterRelationRecord>.FindAllByProperty(
          "_characterId", (long) charLowId)
        .Select(
          crr => RelationMgr.CreateRelation(crr))
        .ToArray();
    }
  }
}