using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    public interface IAIAction : IDisposable
    {
        Unit Owner { get; }

        UpdatePriority Priority { get; }

        bool IsGroupAction { get; }

        ProcTriggerFlags InterruptFlags { get; }

        /// <summary>Start executing current action</summary>
        /// <returns></returns>
        void Start();

        /// <summary>Updates current action</summary>
        /// <returns></returns>
        void Update();

        /// <summary>Stops this Action</summary>
        /// <returns></returns>
        void Stop();
    }
}