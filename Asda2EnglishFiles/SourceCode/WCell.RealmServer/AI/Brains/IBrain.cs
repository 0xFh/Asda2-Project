﻿using System;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Groups;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Brains
{
    /// <summary>
    /// The interface to any brain (usually belonging to an NPC)
    /// A brain is a finite automaton with a queue of actions
    /// </summary>
    public interface IBrain : IUpdatable, IAICombatEventHandler, IDisposable
    {
        /// <summary>Current state of the brain</summary>
        BrainState State { get; set; }

        /// <summary>Default state of the brain</summary>
        BrainState DefaultState { get; set; }

        /// <summary>Aggressive brains actively seek for combat Action</summary>
        bool IsAggressive { get; set; }

        UpdatePriority UpdatePriority { get; }

        /// <summary>Current Running state</summary>
        /// <value>if false, Brain will not update</value>
        bool IsRunning { get; set; }

        /// <summary>The collection of all actions the IBrain can execute</summary>
        IAIActionCollection Actions { get; }

        /// <summary>The AIAction that is currently being executed</summary>
        IAIAction CurrentAction { get; set; }

        /// <summary>
        /// The origin location to which this Brain will always want to go back to (if any)
        /// </summary>
        Vector3 SourcePoint { get; set; }

        List<Vector3> MovingPoints { get; set; }

        void EnterDefaultState();

        void StopCurrentAction();

        /// <summary>Executes a brain cycle</summary>
        void Perform();

        bool ScanAndAttack();

        bool CheckCombat();

        /// <summary>
        /// Used to get the owner of this brain out of combat and leave all fighting behind
        /// </summary>
        void ClearCombat(BrainState newState);

        /// <summary>Called when the AIGroup of an NPC is about to change</summary>
        void OnGroupChange(AIGroup newGroup);
    }
}