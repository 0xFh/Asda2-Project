using System;

namespace WCell.RealmServer.Spells
{
    public interface ICooldown
    {
        DateTime Until { get; set; }

        IConsistentCooldown AsConsistent();
    }
}