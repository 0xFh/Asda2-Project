using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
    /// <summary>Customizable ProcHandler</summary>
    public interface IProcHandler : IDisposable
    {
        /// <summary>The one who the proc handler is applied to</summary>
        Unit Owner { get; }

        ProcTriggerFlags ProcTriggerFlags { get; }

        ProcHitFlags ProcHitFlags { get; }

        /// <summary>Probability to proc in percent (0-100)</summary>
        uint ProcChance { get; }

        /// <summary>The Spell to be triggered (if any)</summary>
        Spell ProcSpell { get; }

        int StackCount { get; }

        int MinProcDelay { get; }

        /// <summary>
        /// Time when this proc may be triggered again (or small value, if always)
        /// </summary>
        DateTime NextProcTime { get; set; }

        /// <summary>Whether this handler can trigger the given Proc</summary>
        /// <param name="target"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active);

        void TriggerProc(Unit triggerer, IUnitAction action);
    }
}