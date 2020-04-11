using WCell.RealmServer.Network;

namespace WCell.RealmServer.Entities
{
    public interface ICharacterCollection : ICharacterSet, IPacketReceiver
    {
        void AddCharacter(Character chr);

        void RemoveCharacter(Character chr);
    }
}