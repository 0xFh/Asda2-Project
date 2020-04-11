using WCell.Core;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    /// <summary>Defines an entity with a name.</summary>
    public class NamedEntity : IEntity, INamed
    {
        private readonly EntityId m_EntityId;
        private string m_Name;

        public static EntityId CreateId()
        {
            return EntityId.GetPlayerId(CharacterRecord.NextId());
        }

        public NamedEntity(string name)
        {
            this.m_Name = name;
            this.m_EntityId = NamedEntity.CreateId();
        }

        public EntityId EntityId
        {
            get { return this.m_EntityId; }
        }

        public string Name
        {
            get { return this.m_Name; }
        }
    }
}