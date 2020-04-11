using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    public interface IBattlegroundRelation
    {
        bool IsEnqueued { get; }

        int Count { get; }

        BattlegroundTeamQueue Queue { get; }

        ICharacterSet Characters { get; }
    }
}