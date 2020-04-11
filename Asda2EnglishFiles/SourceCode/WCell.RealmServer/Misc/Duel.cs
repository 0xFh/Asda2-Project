using System;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.Threading;

namespace WCell.RealmServer.Misc
{
    /// <summary>
    /// Represents the progress of a duel between 2 Characters.
    /// Most methods require the context of the Flag's map
    /// </summary>
    public class Duel : IDisposable, IUpdatable
    {
        /// <summary>The delay until a duel starts in milliseconds</summary>
        public static int DefaultStartDelayMillis = 3000;

        /// <summary>
        /// If Duelist leaves the area around the flag for longer than this delay, he/she will loose the duel
        /// </summary>
        public static int DefaultCancelDelayMillis = 10000;

        private static float s_duelRadius;
        private static float s_duelRadiusSq;
        private Character m_challenger;
        private Character m_rival;
        private int m_startDelay;
        private int m_challengerCountdown;
        private int m_rivalCountdown;
        private int m_cancelDelay;
        private bool m_active;
        private bool m_accepted;
        private bool m_challengerInRange;
        private bool m_rivalInRange;
        private GameObject m_flag;
        private Map m_Map;

        /// <summary>
        /// If duelists are further away than the DuelRadius (in yards), the cancel-timer will be started.
        /// </summary>
        public static float DuelRadius
        {
            get { return Duel.s_duelRadius; }
            set
            {
                Duel.s_duelRadius = value;
                Duel.s_duelRadiusSq = value * value;
            }
        }

        public static float DuelRadiusSquare
        {
            get { return Duel.s_duelRadiusSq; }
        }

        static Duel()
        {
            Duel.DuelRadius = 25f;
        }

        /// <summary>Checks several requirements for a new Duel to start.</summary>
        /// <param name="challenger"></param>
        /// <param name="rival"></param>
        /// <returns></returns>
        public static SpellFailedReason CheckRequirements(Character challenger, Character rival)
        {
            if (challenger.IsDueling)
                return SpellFailedReason.CantDuelWhileStealthed;
            if (challenger.Zone != null && !challenger.Zone.Flags.HasFlag((Enum) ZoneFlags.Duel))
                return SpellFailedReason.NotHere;
            if (!challenger.KnowsOf((WorldObject) rival))
                return SpellFailedReason.NoValidTargets;
            if (!rival.KnowsOf((WorldObject) challenger))
                return SpellFailedReason.CantDuelWhileInvisible;
            if (rival.IsInCombat)
                return SpellFailedReason.TargetInCombat;
            if (rival.DuelOpponent != null)
                return SpellFailedReason.TargetDueling;
            return challenger.Map is Battleground ? SpellFailedReason.NotHere : SpellFailedReason.Ok;
        }

        /// <summary>
        /// Make sure that the 2 parties may actual duel, by calling <see cref="M:WCell.RealmServer.Misc.Duel.CheckRequirements(WCell.RealmServer.Entities.Character,WCell.RealmServer.Entities.Character)" /> before.
        /// </summary>
        /// <param name="challenger"></param>
        /// <param name="rival"></param>
        /// <returns></returns>
        public static Duel InitializeDuel(Character challenger, Character rival)
        {
            challenger.EnsureContext();
            rival.EnsureContext();
            return new Duel(challenger, rival, Duel.DefaultStartDelayMillis, Duel.DefaultCancelDelayMillis);
        }

        /// <summary>Creates a new duel between the 2 parties.</summary>
        /// <param name="challenger"></param>
        /// <param name="rival"></param>
        /// <param name="startDelay"></param>
        /// <param name="cancelDelay"></param>
        internal Duel(Character challenger, Character rival, int startDelay, int cancelDelay)
        {
            this.m_challenger = challenger;
            this.m_rival = rival;
            this.m_Map = challenger.Map;
            this.m_startDelay = startDelay;
            this.m_cancelDelay = cancelDelay;
            this.m_challenger.Duel = this;
            this.m_challenger.DuelOpponent = rival;
            this.m_rival.Duel = this;
            this.m_rival.DuelOpponent = challenger;
            this.Initialize();
        }

        /// <summary>The Character who challenged the Rival for a Duel</summary>
        public Character Challenger
        {
            get { return this.m_challenger; }
        }

        /// <summary>The Character who has been challenged for a Duel</summary>
        public Character Rival
        {
            get { return this.m_rival; }
        }

        /// <summary>The Duel Flag</summary>
        public GameObject Flag
        {
            get { return this.m_flag; }
        }

        public int CancelDelay
        {
            get { return this.m_cancelDelay; }
            set { this.m_cancelDelay = value; }
        }

        /// <summary>
        /// Delay left in milliseconds until the Duel starts.
        /// If countdown did not start yet, will be set to the total delay.
        /// If countdown is already over, this value is redundant.
        /// </summary>
        public int StartDelay
        {
            get { return this.m_startDelay; }
            set { this.m_startDelay = value; }
        }

        /// <summary>
        /// A duel is active if after the duel engaged, the given startDelay passed
        /// </summary>
        public bool IsActive
        {
            get { return this.m_active; }
        }

        /// <summary>
        /// whether the Challenger is in range of the duel-flag.
        /// When not in range, the cancel timer starts.
        /// </summary>
        public bool IsChallengerInRange
        {
            get { return this.m_challengerInRange; }
        }

        /// <summary>
        /// whether the Rival is in range of the duel-flag.
        /// When not in range, the cancel timer starts.
        /// </summary>
        public bool IsRivalInRange
        {
            get { return this.m_rivalInRange; }
        }

        /// <summary>
        /// Initializes the duel after a new Duel has been proposed
        /// </summary>
        private void Initialize()
        {
            this.m_flag = GameObject.Create(GOEntryId.DuelFlag,
                (IWorldLocation) new WorldLocationStruct(this.m_Map,
                    (this.m_challenger.Position + this.m_rival.Position) / 2f, 1U), (GOSpawnEntry) null,
                (GOSpawnPoint) null);
            if (this.m_flag == null)
            {
                ContentMgr.OnInvalidDBData("Cannot start Duel: DuelFlag-GameObject (ID: {0}) does not exist.",
                    (object) 336);
                this.Cancel();
            }
            else
            {
                this.m_flag.Phase = this.m_challenger.Phase;
                ((DuelFlagHandler) this.m_flag.Handler).Duel = this;
                this.m_flag.CreatedBy = this.m_challenger.EntityId;
                this.m_flag.Level = this.m_challenger.Level;
                this.m_flag.AnimationProgress = byte.MaxValue;
                this.m_flag.Position = this.m_challenger.Position;
                this.m_flag.Faction = this.m_challenger.Faction;
                this.m_flag.ScaleX = this.m_challenger.ScaleX;
                this.m_flag.ParentRotation4 = 1f;
                this.m_flag.Orientation = this.m_challenger.Orientation;
                this.m_Map.AddMessage((IMessage) new Message((Action) (() =>
                    DuelHandler.SendRequest(this.m_flag, this.m_challenger, this.m_rival))));
                this.m_challenger.SetEntityId((UpdateFieldId) PlayerFields.DUEL_ARBITER, this.m_flag.EntityId);
                this.m_rival.SetEntityId((UpdateFieldId) PlayerFields.DUEL_ARBITER, this.m_flag.EntityId);
            }
        }

        /// <summary>
        /// Starts the countdown (automatically called when the invited rival accepts)
        /// </summary>
        public void Accept(Character acceptingCharacter)
        {
            if (this.m_challenger == acceptingCharacter)
                return;
            uint startDelay = (uint) this.m_startDelay;
            DuelHandler.SendCountdown(this.m_challenger, startDelay);
            DuelHandler.SendCountdown(this.m_rival, startDelay);
            this.m_Map.RegisterUpdatableLater((IUpdatable) this);
            this.m_accepted = true;
        }

        /// <summary>
        /// Starts the Duel (automatically called after countdown)
        /// </summary>
        /// <remarks>Requires map context</remarks>
        public void Start()
        {
            this.m_active = true;
            this.m_challengerInRange = true;
            this.m_rivalInRange = true;
            this.m_challenger.SetUInt32((UpdateFieldId) PlayerFields.DUEL_TEAM, 1U);
            this.m_rival.SetUInt32((UpdateFieldId) PlayerFields.DUEL_TEAM, 2U);
            this.m_challenger.FirstAttacker = (Unit) this.m_rival;
            this.m_rival.FirstAttacker = (Unit) this.m_challenger;
        }

        /// <summary>
        /// Ends the duel with the given win-condition and the given loser
        /// </summary>
        /// <param name="loser">The opponent that lost the match or null if its a draw</param>
        /// <remarks>Requires map context</remarks>
        public void Finish(DuelWin win, Character loser)
        {
            if (this.IsActive)
            {
                if (loser != null)
                {
                    int num = (int) loser.SpellCast.Start(SpellId.NotDisplayedGrovel, false);
                    Character duelOpponent = loser.DuelOpponent;
                    duelOpponent.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.WinDuel, 1U, 0U,
                        (Unit) null);
                    loser.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.LoseDuel, 1U, 0U,
                        (Unit) null);
                    DuelHandler.SendWinner(win, (Unit) duelOpponent, (INamed) loser);
                }

                this.m_challenger.FirstAttacker = (Unit) null;
                this.m_rival.FirstAttacker = (Unit) null;
                this.m_challenger.Auras.RemoveWhere((Predicate<Aura>) (aura =>
                {
                    if (!aura.IsBeneficial)
                        return aura.CasterReference.EntityId == this.m_rival.EntityId;
                    return false;
                }));
                this.m_rival.Auras.RemoveWhere((Predicate<Aura>) (aura =>
                {
                    if (!aura.IsBeneficial)
                        return aura.CasterReference.EntityId == this.m_challenger.EntityId;
                    return false;
                }));
                if (this.m_rival.ComboTarget == this.m_challenger)
                    this.m_rival.ResetComboPoints();
                if (this.m_challenger.ComboTarget == this.m_rival)
                    this.m_challenger.ResetComboPoints();
            }

            this.Dispose();
        }

        /// <summary>Updates the Duel</summary>
        /// <param name="dt">the time since the last update in milliseconds</param>
        public void Update(int dt)
        {
            if (this.m_challenger == null || this.m_rival == null)
                this.Dispose();
            else if (!this.m_challenger.IsInContext || !this.m_rival.IsInContext || !this.m_flag.IsInContext)
                this.Dispose();
            else if (!this.m_active)
            {
                if (!this.m_accepted)
                    return;
                this.m_startDelay -= dt;
                if (this.m_startDelay > 0)
                    return;
                this.Start();
            }
            else
            {
                if (this.m_challengerInRange !=
                    this.m_challenger.IsInRadiusSq((IHasPosition) this.m_flag, Duel.s_duelRadiusSq))
                {
                    this.m_challengerInRange = !this.m_challengerInRange;
                    if (this.m_challengerInRange)
                    {
                        this.m_challengerCountdown = 0;
                        DuelHandler.SendInBounds(this.m_challenger);
                    }
                    else
                        DuelHandler.SendOutOfBounds(this.m_challenger, this.m_cancelDelay);
                }
                else if (!this.m_challengerInRange)
                {
                    this.m_challengerCountdown += dt;
                    if (this.m_challengerCountdown >= this.m_cancelDelay)
                        this.Finish(DuelWin.OutOfRange, this.m_challenger);
                }

                if (this.m_rivalInRange != this.m_rival.IsInRadiusSq((IHasPosition) this.m_flag, Duel.s_duelRadiusSq))
                {
                    this.m_rivalInRange = !this.m_rivalInRange;
                    if (this.m_rivalInRange)
                    {
                        DuelHandler.SendOutOfBounds(this.m_rival, this.m_cancelDelay);
                    }
                    else
                    {
                        this.m_rivalCountdown = 0;
                        DuelHandler.SendInBounds(this.m_rival);
                    }
                }
                else
                {
                    if (this.m_rivalInRange)
                        return;
                    this.m_rivalCountdown += dt;
                    if (this.m_rivalCountdown < this.m_cancelDelay)
                        return;
                    this.Finish(DuelWin.OutOfRange, this.m_rival);
                }
            }
        }

        /// <summary>Duel is over due to death of one of the opponents</summary>
        internal void OnDeath(Character duelist)
        {
            duelist.Health = 1;
            duelist.Auras.RemoveWhere((Predicate<Aura>) (aura => aura.CasterUnit == duelist.DuelOpponent));
            this.Finish(DuelWin.Knockout, duelist);
        }

        internal void Cleanup()
        {
            DuelHandler.SendComplete(this.m_challenger, this.m_rival, this.m_active);
            this.m_active = false;
            Map map = this.m_Map;
            map.ExecuteInContext((Action) (() =>
            {
                map.UnregisterUpdatable((IUpdatable) this);
                if (this.m_challenger != null)
                {
                    this.m_challenger.SetEntityId((UpdateFieldId) PlayerFields.DUEL_ARBITER, EntityId.Zero);
                    this.m_challenger.SetUInt32((UpdateFieldId) PlayerFields.DUEL_TEAM, 0U);
                    this.m_challenger.Duel = (Duel) null;
                    this.m_challenger.DuelOpponent = (Character) null;
                }

                this.m_challenger = (Character) null;
                if (this.m_rival != null)
                {
                    this.m_rival.SetEntityId((UpdateFieldId) PlayerFields.DUEL_ARBITER, EntityId.Zero);
                    this.m_rival.SetUInt32((UpdateFieldId) PlayerFields.DUEL_TEAM, 0U);
                    this.m_rival.Duel = (Duel) null;
                    this.m_rival.DuelOpponent = (Character) null;
                }

                this.m_rival = (Character) null;
            }));
        }

        public void Cancel()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes the duel (called automatically when duel ends)
        /// </summary>
        /// <remarks>Requires map context</remarks>
        public void Dispose()
        {
            if (this.m_flag != null)
                this.m_flag.Delete();
            else
                this.Cleanup();
        }
    }
}