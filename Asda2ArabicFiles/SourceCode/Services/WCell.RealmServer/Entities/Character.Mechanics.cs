using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Trade;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public partial class Character
    {
        protected int[] m_dmgBonusVsCreatureTypePct;
        internal int[] m_MeleeAPModByStat;
        internal int[] m_RangedAPModByStat;

        #region Creature Type Damage

        /// <summary>
        /// Damage bonus vs creature type in %
        /// </summary>
        public void ModDmgBonusVsCreatureTypePct(CreatureType type, int delta)
        {
            if (m_dmgBonusVsCreatureTypePct == null)
            {
                m_dmgBonusVsCreatureTypePct = new int[(int)CreatureType.End];
            }
            var val = m_dmgBonusVsCreatureTypePct[(int)type] + delta;
            m_dmgBonusVsCreatureTypePct[(int)type] = val;
        }

        /// <summary>
        /// Damage bonus vs creature type in %
        /// </summary>
        public void ModDmgBonusVsCreatureTypePct(uint[] creatureTypes, int delta)
        {
            foreach (var type in creatureTypes)
            {
                ModDmgBonusVsCreatureTypePct((CreatureType)type, delta);
            }
        }
        #endregion

        #region AP Bonus By Stat
        public int GetMeleeAPModByStat(StatType stat)
        {
            if (m_MeleeAPModByStat == null)
            {
                return 0;
            }
            return m_MeleeAPModByStat[(int)stat];
        }

        public void SetMeleeAPModByStat(StatType stat, int value)
        {
            if (m_MeleeAPModByStat == null)
            {
                m_MeleeAPModByStat = new int[(int)StatType.End];
            }
            m_baseStats[(int)stat] = value;
            this.UpdateMeleeAttackPower();
        }

        public void ModMeleeAPModByStat(StatType stat, int delta)
        {
            SetMeleeAPModByStat(stat, (GetMeleeAPModByStat(stat) + delta));
        }

        public int GetRangedAPModByStat(StatType stat)
        {
            if (m_RangedAPModByStat == null)
            {
                return 0;
            }
            return m_RangedAPModByStat[(int)stat];
        }

        public void SetRangedAPModByStat(StatType stat, int value)
        {
            if (m_RangedAPModByStat == null)
            {
                m_RangedAPModByStat = new int[(int)StatType.End];
            }
            m_baseStats[(int)stat] = value;
            this.UpdateRangedAttackPower();
        }

        public void ModRangedAPModByStat(StatType stat, int delta)
        {
            SetRangedAPModByStat(stat, (GetRangedAPModByStat(stat) + delta));
        }
        #endregion

        #region Movement Handling
        /// <summary>
        /// Is called whenever the Character moves up or down in water or while flying.
        /// </summary>
        internal protected void MovePitch(float moveAngle)
        {
        }

        /// <summary>
        /// Is called whenever the Character falls
        /// </summary>
        internal protected void OnFalling()
        {
            if (m_fallStart == 0)
            {
                m_fallStart = Environment.TickCount;
                m_fallStartHeight = m_position.Z;
            }


            if (IsFlying || !IsAlive || GodMode)
            {
                return;
            }
            // TODO Immunity against environmental damage

        }

        public bool IsSwimming
        {
            get { return MovementFlags.HasFlag(MovementFlags.Swimming); }
        }

        public bool IsUnderwater
        {
            get { return m_position.Z < m_swimSurfaceHeight - 0.5f; }
        }

        internal protected void OnSwim()
        {
            // TODO: Lookup liquid type and verify heights
            if (!IsSwimming)
            {
                m_swimStart = DateTime.Now;
            }
            else
            {

            }
        }

        internal protected void OnStopSwimming()
        {
            m_swimSurfaceHeight = -2048;
        }

        /// <summary>
        /// Is called whenever the Character is moved while on Taxi, Ship, elevator etc
        /// </summary>
        internal protected void MoveTransport(ref Vector4 transportLocation)
        {
            SendSystemMessage("You have been identified as cheater: Faking transport movement!");
        }

        public int EndMoveCount;
        /// <summary>
        /// Is called whenever a Character moves
        /// </summary>
        public override void OnMove()
        {
            base.OnMove();
            IsFighting = false;

            if (m_standState != StandState.Stand)
            {
                StandState = StandState.Stand;
            }

            if (m_currentRitual != null)
            {
                m_currentRitual.Remove(this);
            }

            if (IsTrading && !IsInRadius(m_tradeWindow.OtherWindow.Owner, TradeMgr.MaxTradeRadius))
            {
                m_tradeWindow.Cancel(TradeStatus.TooFarAway);
            }

            if (CurrentCapturingPoint != null)
            {
                CurrentCapturingPoint.StopCapture();
            }
            //var now = Environment.TickCount;
            /*if (m_fallStart > 0 && now - m_fallStart > 3000 && m_position.Z == LastPosition.Z)
            {
                if (IsAlive && Flying == 0 && Hovering == 0 && FeatherFalling == 0 && !IsImmune(DamageSchool.Physical))
                {
                    var fallDamage = FallDamageGenerator.GetFallDmg(this, m_fallStartHeight - m_position.Z);
					
                    if (fallDamage > 0)
                    {
                        // If the character current health is higher then the fall damage, the player survived the fall.
                        if (fallDamage < Health)
                        {
                            Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.FallWithoutDying, (uint)(m_fallStartHeight - m_position.Z));
                        }
                    //	DoEnvironmentalDamage(EnviromentalDamageType.Fall, fallDamage);
                    }

                    m_fallStart = 0;
                    m_fallStartHeight = 0;
                }
            }

            // TODO: Change speedhack detection
            // TODO: Check whether the character is really in Taxi
            if (SpeedHackCheck)
            {
                var msg = "You have been identified as a SpeedHacker. - Byebye!";

                // simple SpeedHack protection
                int latency = Client.Latency;
                int delay = now - m_lastMoveTime + Math.Max(1000, latency);

                float speed = Flying > 0 ? FlightSpeed : RunSpeed;
                float maxDistance = (speed / 1000f) * delay * SpeedHackToleranceFactor;
                if (!IsInRadius(ref LastPosition, maxDistance))
                {
                    // most certainly a speed hacker
                    log.Warn("WARNING: Possible speedhacker [{0}] moved {1} yards in {2} milliseconds (Latency: {3}, Tolerance: {4})",
                             this, GetDistance(ref LastPosition), delay, latency, SpeedHackToleranceFactor);
                }

                Kick(msg);
            }*/

            LastPosition = MoveControl.Mover.Position;
        }

        public void SetMover(WorldObject mover, bool canControl)
        {
            MoveControl.Mover = mover ?? this;
            MoveControl.CanControl = canControl;

        }

        public void ResetMover()
        {
            MoveControl.Mover = this;
            MoveControl.CanControl = true;
        }

        /// <summary>
        /// Is called whenever a new object appears within vision range of this Character
        /// </summary>
        public void OnEncountered(WorldObject obj)
        {
            if (obj != this)
            {
                obj.OnEncounteredBy(this);
            }
            KnownObjects.Add(obj);
        }


        /// <summary>
        /// Is called whenever an object leaves this Character's sight
        /// </summary>
        public void OnOutOfRange(WorldObject obj)
        {
            obj.AreaCharCount--;
            if (obj == Asda2DuelingOponent)
            {
                if (Asda2Duel != null)
                    Asda2Duel.StopPvp();
            }
            if (obj == m_target)
            {
                // unset current Target
                ClearTarget();
            }

            if (obj == m_activePet)
            {
                ActivePet = null;
            }

            if (GossipConversation != null && obj == GossipConversation.Speaker && GossipConversation.Character == this)
            {
                // stop conversation with a vanished object
                GossipConversation.Dispose();
            }

            if (!(obj is Transport))
            {
                KnownObjects.Remove(obj);
            }
            var chr = obj as Character;
            if (chr != null)
            {
                if (EnemyCharacters.Contains(chr))
                {
                    EnemyCharacters.Remove(chr);
                    CheckEnemysCount();
                }
                GlobalHandler.SendCharacterDeleteResponse(chr, Client);
            }
            else
            {
                var loot = obj as Asda2Loot;
                if (loot != null)
                    GlobalHandler.SendRemoveLootResponse(this, loot);
            }
        }

        public void CheckEnemysCount()
        {
            if (EnemyCharacters.Count == 0 && !IsAsda2BattlegroundInProgress)
                GlobalHandler.SendFightingModeChangedResponse(Client, SessionId, (int)AccId, -1);
        }

        /// <summary>
        /// Is called whenever this Character was added to a new map
        /// </summary>
        internal protected override void OnEnterMap()
        {
            base.OnEnterMap();
            if (!_saveTaskRunning)
            {
                _saveTaskRunning = true;
                RealmServer.IOQueue.CallDelayed(CharacterFormulas.SaveChateterInterval, SaveCharacter);
            }
            // when removed from map, make sure the Character forgets everything and gets everything re-sent
            ClearSelfKnowledge();

            m_lastMoveTime = Environment.TickCount;
            LastPosition = m_position;

            AddPostUpdateMessage(() =>
            {
                // Add Honorless Target buff
                if (m_zone != null && m_zone.Template.IsPvP)
                {
                    //SpellCast.TriggerSelf(SpellId.HonorlessTarget);
                }
            });

            if (IsPetActive)
            {
                // actually spawn pet
                IsPetActive = true;
            }
        }

        protected internal override void OnLeavingMap()
        {
            if (m_activePet != null && m_activePet.IsInWorld)
            {
                m_activePet.Map.RemoveObject(m_activePet);
            }

            if (m_minions != null)
            {
                foreach (var minion in m_minions)
                {
                    minion.Delete();
                }
            }

            base.OnLeavingMap();
        }

        private StandState m_standState;
        private bool _isMoving;

        /// <summary>
        /// Changes the character's stand state and notifies the client.
        /// </summary>
        public override StandState StandState
        {
            get { return m_standState; }
            set
            {
                if (value != StandState)
                {
                    m_standState = value;
                    base.StandState = value;

                    if (m_looterEntry != null &&
                        m_looterEntry.Loot != null &&
                        value != StandState.Kneeling &&
                        m_looterEntry.Loot.MustKneelWhileLooting)
                    {
                        CancelLooting();
                    }

                    if (value == StandState.Stand)
                    {
                        m_auras.RemoveByFlag(AuraInterruptFlags.OnStandUp);
                    }

                }
            }
        }
        #endregion

        #region Overrides
        protected override void OnResistanceChanged(DamageSchool school)
        {
            base.OnResistanceChanged(school);
            if (m_activePet != null && m_activePet.IsHunterPet)
            {
                m_activePet.UpdatePetResistance(school);
            }
        }

        public override void ModSpellHitChance(DamageSchool school, int delta)
        {
            base.ModSpellHitChance(school, delta);

            // also modify pet's hit chance
            if (m_activePet != null)
            {
                m_activePet.ModSpellHitChance(school, delta);
            }
        }

        public override float GetResiliencePct()
        {
            return 0;
        }

        public override void DealEnvironmentalDamage(EnviromentalDamageType dmgType, int amount)
        {
            base.DealEnvironmentalDamage(dmgType, amount);
            if (!IsAlive)
            {
                Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathsFrom, (uint)dmgType, 1);
            }
        }
        #endregion

        public new bool IsMoving
        {
            get { return _isMoving; }
            set
            {
                _isMoving = value;
                if (value)
                    OnMove();
            }
        }
        public BaseRelation GetRelationTo(Character chr, CharacterRelationType type)
        {
            return Singleton<RelationMgr>.Instance.GetRelation(EntityId.Low, chr.EntityId.Low, type);
        }

        /// <summary>
        /// Returns whether this Character ignores the Character with the given low EntityId.
        /// </summary>
        /// <returns></returns>
        public bool IsIgnoring(IUser user)
        {
            return Singleton<RelationMgr>.Instance.HasRelation(EntityId.Low, user.EntityId.Low, CharacterRelationType.Ignored);
        }

        /// <summary>
        /// Indicates whether the two Characters are in the same <see cref="Group"/>
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool IsAlliedWith(Character chr)
        {
            return m_groupMember != null && chr.m_groupMember != null && m_groupMember.Group == chr.m_groupMember.Group;
        }

        /// <summary>
        /// Binds Character to start position if none other is set
        /// </summary>
        void CheckBindLocation()
        {
            if (!m_bindLocation.IsValid())
            {
                BindTo(this, m_archetype.StartLocation);
            }
        }

        public void TeleportToBindLocation()
        {
            TeleportTo(BindLocation);
        }

        public bool CanFly
        {
            get
            {
                return (m_Map.CanFly && (m_zone == null ||
                    m_zone.Flags.HasFlag(ZoneFlags.CanFly) && !m_zone.Flags.HasFlag(ZoneFlags.CannotFly))
                    || Role.IsStaff);
            }
        }

        #region Mounts

        public override void Mount(uint displayId)
        {
            if (m_activePet != null)
            {
                // remove active pet
                m_activePet.RemoveFromMap();
            }

            base.Mount(displayId);
        }

        protected internal override void DoDismount()
        {
            if (IsPetActive)
            {
                // put pet into world
                PlaceOnTop(ActivePet);
            }
            base.DoDismount();
        }
        #endregion

        public int GetRandomMagicDamage()
        {
            return Utility.Random(MinMagicDamage, MaxMagicDamage);
        }

        public float GetRandomPhysicalDamage()
        {
            return Utility.Random(MinDamage, MaxDamage);
        }
        public byte RealProffLevel
        {
            get
            {
                if (Class == ClassId.THS || Class == ClassId.OHS || Class == ClassId.Spear || Class == ClassId.NoClass)
                    return ProfessionLevel;
                if (Class == ClassId.AtackMage || Class == ClassId.SupportMage || Class == ClassId.HealMage)
                    return (byte)(ProfessionLevel - 22);
                if (Class == ClassId.Bow || Class == ClassId.Crossbow || Class == ClassId.Balista)
                    return (byte)(ProfessionLevel - 11);
                return 0;
            }
        }

        public Asda2PetRecord AddAsda2Pet(PetTemplate petTemplate, bool silent = false)
        {
            var newPet = new Asda2PetRecord(petTemplate, this);
            newPet.Create();
            OwnedPets.Add(newPet.Guid, newPet);
            Asda2TitleChecker.OnPetCountChanged(OwnedPets.Count,this);
            if (!silent)
                Asda2PetHandler.SendInitPetInfoOnLoginResponse(Client, newPet);
            return newPet;
        }

        Locale IPacketReceiver.Locale
        {
            get;
            set;
        }
    }
}
