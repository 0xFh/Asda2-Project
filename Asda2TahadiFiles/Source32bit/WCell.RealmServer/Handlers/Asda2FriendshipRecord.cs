using Castle.ActiveRecord;
using System.Collections.Generic;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
  [ActiveRecord("Asda2FriendshipRecord", Access = PropertyAccess.Property)]
  public class Asda2FriendshipRecord : WCellRecord<Asda2FriendshipRecord>
  {
    private static readonly NHIdGenerator _idGenerator =
      new NHIdGenerator(typeof(Asda2FriendshipRecord), nameof(Guid), 1L);

    [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
    public long Guid { get; set; }

    [Property]
    public uint FirstCharacterAccId { get; set; }

    [Property]
    public uint SecondCharacterAccId { get; set; }

    public static List<Asda2FriendshipRecord> LoadAll(uint characterId)
    {
      Asda2FriendshipRecord[] allByProperty1 =
        FindAllByProperty("FirstCharacterAccId", characterId);
      Asda2FriendshipRecord[] allByProperty2 =
        FindAllByProperty("SecondCharacterAccId", characterId);
      List<Asda2FriendshipRecord> friendshipRecordList = new List<Asda2FriendshipRecord>();
      friendshipRecordList.AddRange(allByProperty1);
      friendshipRecordList.AddRange(allByProperty2);
      return friendshipRecordList;
    }

    public Asda2FriendshipRecord()
    {
    }

    public Asda2FriendshipRecord(Character firstCharacter, Character secondCharacter)
    {
      Guid = _idGenerator.Next();
      FirstCharacterAccId = firstCharacter.EntityId.Low;
      SecondCharacterAccId = secondCharacter.EntityId.Low;
    }

    public uint GetFriendId(uint low)
    {
      if((int) FirstCharacterAccId == (int) low)
        return SecondCharacterAccId;
      return FirstCharacterAccId;
    }
  }
}