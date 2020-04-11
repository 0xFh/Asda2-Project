using System;

namespace WCell.Intercommunication
{
    public interface IAccount : IAccountInfo
    {
        string Name { get; }

        bool IsActive { get; }

        DateTime? StatusUntil { get; }
    }
}