/*************************************************************************
 *
 *   file		: Owner.Update.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 10:02:00 +0100 (lø, 30 jan 2010) $
 
 *   revision		: $Rev: 1234 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Mounts;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.UpdateFields;
using WCell.Util.Collections;
using WCell.Util.NLog;

namespace WCell.RealmServer.Entities
{

    /// <summary>
    /// TODO: Move Update and BroadcastValueUpdate for Character together, since else we sometimes 
    /// have to fetch everything in our environment twice in a single map update
    /// </summary>
    public partial class Character
    {
        public static new readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Player);

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return UpdateFieldInfos; }
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicPlayerHandlers; }
        }



        private HashSet<Item> m_itemsRequiringUpdates = new HashSet<Item>();

        /// <summary>
        /// All Characters that recently were inspecting our inventory
        /// </summary>
        private HashSet<Character> m_observers = new HashSet<Character>();

        /// <summary>
        /// Messages to be processed by the map after updating of the environment (sending of Update deltas etc).
        /// </summary>
        private readonly LockfreeQueue<Action> m_environmentQueue = new LockfreeQueue<Action>();

        protected bool m_initialized;

        private Unit observing;

        public Unit Observing
        {
            get { return observing ?? this; }
            set { observing = value; }
        }

        #region Messages
        /// <summary>
        /// Will be executed by the current map we are currently in or enqueued and executed,
        /// once we re-enter a map
        /// </summary>
        public void AddPostUpdateMessage(Action action)
        {
            m_environmentQueue.Enqueue(action);
        }
        #endregion


        public HashSet<Character> Observers
        {
            get { return m_observers; }
        }

        #region Owned objects
        internal void AddItemToUpdate(Item item)
        {
            m_itemsRequiringUpdates.Add(item);
        }

        /// <summary>
        /// Removes the given item visually from the Client.
        /// Do not call this method - but use Item.Remove instead.
        /// </summary>
        internal void RemoveOwnedItem(Item item)
        {
            //if (m_itemsRequiringUpdates.Remove(item))
            m_itemsRequiringUpdates.Remove(item);
            m_environmentQueue.Enqueue(() =>
            {
                item.SendDestroyToPlayer(this);
                if (m_observers == null)
                {
                    return;
                }

                foreach (var observer in m_observers)
                {
                    item.SendDestroyToPlayer(observer);
                }
            });
        }

        #endregion

        #region World Knowledge
        /// <summary>
        /// Resends all updates of everything
        /// </summary>
        public void ResetOwnWorld()
        {
            MovementHandler.SendNewWorld(Client, MapId, ref m_position, Orientation);
            ClearSelfKnowledge();
        }

        /// <summary>
        /// Clears known objects and leads to resending of the creation packet
        /// during the next Map-Update.
        /// This is only needed for teleporting or body-transfer.
        /// Requires map context.
        /// </summary>
        internal void ClearSelfKnowledge()
        {
            KnownObjects.Clear();
            NearbyObjects.Clear();
            if (m_observers != null) m_observers.Clear();

            /*foreach (var item in m_inventory.GetAllItems(true))
            {
                item.m_unknown = true;
                m_itemsRequiringUpdates.Add(item);
            }*/
        }

        /// <summary>
        /// Will resend update packet of the given object
        /// </summary>
        public void InvalidateKnowledgeOf(WorldObject obj)
        {
            KnownObjects.Remove(obj);
            NearbyObjects.Remove(obj);

            obj.SendDestroyToPlayer(this);
        }

        /// <summary>
        /// Whether the given Object is visible to (and thus in broadcast-range of) this Character
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool KnowsOf(WorldObject obj)
        {
            return KnownObjects.Contains(obj);
        }


        /// <summary>
        /// Collects all update-masks from nearby objects
        /// </summary>
        internal void UpdateEnvironment(HashSet<WorldObject> updatedObjects)
        {
            var toRemove = WorldObjectSetPool.Obtain();
            toRemove.AddRange(KnownObjects);
            toRemove.Remove(this);

            NearbyObjects.Clear();

            if (m_initialized)
            {
                Observing.IterateEnvironment(BroadcastRange, (obj) =>
                {
                    if (Client == null || Client.ActiveCharacter == null)
                    {
                        if (Client == null)
                            LogUtil.WarnException("Client is null. removeing from map and world? {0}[{1}]", Name, AccId);
                        else
                        {
                            if (Client.ActiveCharacter == null)
                                LogUtil.WarnException("Client.ActiveCharacter is null. removeing from map and world? {0}[{1}]", Name, AccId);
                        }
                        if (Map != null)
                        {
                            Map.AddMessage(() =>
                            {
                                Map.RemoveObject(this);
                                World.RemoveCharacter(this);
                            });
                        }
                        return false;
                    }
                    if (!Observing.IsInPhase(obj))
                    {
                        return true;
                    }
                    if (obj is GameObject && obj.GetDistance(this) > BroadcastRangeNpc)
                        return true;

                    NearbyObjects.Add(obj);


                    //ensure "this" never goes out of range
                    //if we are observing another units broadcasts
                    if (!Observing.CanSee(obj) && !ReferenceEquals(obj, this))
                    {
                        return true;
                    }

                    if (!KnownObjects.Contains(obj))
                    {
                        // encountered new object
                        //TODO Send upadte Packets here ASDA
                        var visibleChr = obj as Character;
                        if (visibleChr != null && visibleChr != this)
                        {
                            GlobalHandler.SendCharacterVisibleNowResponse(Client, visibleChr);
                           
                                if (visibleChr.Asda2Pet != null)
                                    GlobalHandler.SendCharacterInfoPetResponse(Client, visibleChr);
                                if (visibleChr.IsAsda2TradeDescriptionEnabled)
                                    Asda2PrivateShopHandler.SendtradeStatusTextWindowResponseToOne(
                                        visibleChr, Client);
                                GlobalHandler.SendCharacterPlaceInTitleRatingResponse(Client, visibleChr);
                                GlobalHandler.SendBuffsOnCharacterInfoResponse(Client, visibleChr);
                                if (visibleChr.IsInGuild)
                                    GlobalHandler.SendCharacterInfoClanNameResponse(Client, visibleChr);
                                GlobalHandler.SendCharacterFactionAndFactionRankResponse(Client, visibleChr);
                                GlobalHandler.SendCharacterFriendShipResponse(Client, visibleChr);
                                if (visibleChr.ChatRoom != null)
                                    ChatMgr.SendChatRoomVisibleResponse(visibleChr, ChatRoomVisibilityStatus.Visible, visibleChr.ChatRoom, this);
                                CheckAtackStateWithCharacter(visibleChr);
                                if (visibleChr.Asda2WingsItemId != -1)
                                    FunctionalItemsHandler.SendWingsInfoResponse(visibleChr, Client);
                                if (visibleChr.TransformationId != -1)
                                {
                                    GlobalHandler.SendTransformToPetResponse(visibleChr, true, Client);
                                }

                            if (visibleChr.IsOnTransport)
                                FunctionalItemsHandler.SendShopItemUsedResponse(Client, visibleChr, int.MaxValue);
                            if (visibleChr.IsOnMount)
                                Asda2MountHandler.SendCharacterOnMountStatusChangedToPneClientResponse(Client,
                                    visibleChr);
                        }
                        else
                        {
                            var visibleMonstr = obj as NPC;
                            if (visibleMonstr != null && visibleMonstr.IsAlive)
                                GlobalHandler.SendMonstrVisibleNowResponse(Client, visibleMonstr);
                            else
                            {
                                var npc = obj as GameObject;
                                if (npc != null && npc.GoId != GOEntryId.Portal)
                                {
                                    if (!IsAsda2BattlegroundInProgress ||
                                        CurrentBattleGround.WarType != Asda2BattlegroundType.Deathmatch ||
                                        MapId != MapId.BatleField)
                                        GlobalHandler.SendNpcVisiableNowResponse(Client, npc);
                                }
                                else
                                {
                                    var loot = obj as Asda2Loot;
                                    if (loot != null)
                                    {
                                        GlobalHandler.SendItemVisible(this, loot);
                                    }
                                }
                            }
                        }
                        OnEncountered(obj);
                    }

                    toRemove.Remove(obj);	// still in range, no need to remove it
                    return true;
                });

                //update group member stats for out of range players
                if (m_groupMember != null)
                {
                    m_groupMember.Group.UpdateOutOfRangeMembers(m_groupMember);
                }

                // delete objects that are not in range anymore
                foreach (var obj in toRemove)
                {
                    OnOutOfRange(obj);
                }
            }

            // init player, delete Items etc
            Action action;
            while (m_environmentQueue.TryDequeue(out action))
            {
                var ac = action;
                // need to Add a message because Update state will be reset after method call
                AddMessage(ac);
            }

            // check rest state
            if (m_restTrigger != null)
            {
                UpdateRestState();
            }

            toRemove.Clear();
            WorldObjectSetPool.Recycle(toRemove);
        }

        private void CheckAtackStateWithCharacter(Character visibleChr)
        {

            if (MayAttack(visibleChr))
            {
                EnemyCharacters.Add(visibleChr);
                if (IsAsda2BattlegroundInProgress)
                  GlobalHandler.SendFightingModeChangedOnWarResponse(Client, visibleChr.SessionId, (int)visibleChr.AccId, visibleChr.Asda2FactionId);
                  else
                   GlobalHandler.SendFightingModeChangedResponse(Client, SessionId, (int)AccId, visibleChr.SessionId);
                                       
            }
            else
            {
                if (EnemyCharacters.Contains(visibleChr))
                {
                    EnemyCharacters.Remove(visibleChr);
                    CheckEnemysCount();
                }
            }
        }

        public List<Character> EnemyCharacters = new List<Character>();
        /// <summary>
        /// Check if this Character is still resting (if it was resting before)
        /// </summary>
        void UpdateRestState()
        {
            if (!m_restTrigger.IsInArea(this))
            {
                RestTrigger = null;
            }
        }

        /// <summary>
        /// Sends Item-information and Talents to the given Character and keeps them updated until they
        /// are out of range.
        /// </summary>
        /// <param name="chr"></param>
        /*public void AddObserver(Character chr)
        {
            if (m_observers == null)
            {
                m_observers = new HashSet<Character>();
            }

            if (!m_observers.Contains(chr))
            {
                // only send item creation if Character wasn't already observing
                for (var i = InventorySlot.Bag1; i < InventorySlot.Bank1; i++)
                {
                    var item = m_inventory[i];
                    if (item != null)
                    {
                        item.WriteObjectCreationUpdate(chr);
                    }
                }

                m_observers.Add(chr);
            }

            TalentHandler.SendInspectTalents(chr);
        }*/
        #endregion



        //protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
        //{
        //    base.WriteMovementUpdate(packet, relation);
        //}


        public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
        {
            if (chr == this)
            {
                return UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly | UpdateFieldFlags.GroupOnly | UpdateFieldFlags.Public;
            }
            return base.GetUpdateFieldVisibilityFor(chr);
        }

        protected override UpdateType GetCreationUpdateType(UpdateFieldFlags flags)
        {
            return flags.HasAnyFlag(UpdateFieldFlags.Private) ? UpdateType.CreateSelf : UpdateType.Create;
        }

        public void PushFieldUpdate(UpdateFieldId field, uint value)
        {
            if (!IsInWorld)
            {
                // set the value and don't push, we aren't in game so we'll get it on the next self full update
                SetUInt32(field, value);

                return;
            }

            using (var packet = GetFieldUpdatePacket(field, value))
            {
                SendUpdatePacket(this, packet);
            }
        }

        public void PushFieldUpdate(UpdateFieldId field, EntityId value)
        {

        }

        #region IUpdatable
        public List<Asda2ItemCategory> CategoryBuffsToDelete = new List<Asda2ItemCategory>();
        public uint LastPetExpGainTime;
        public uint LastPetEatingTime;
        public uint LastSendIamNotMoving;
        public override void Update(int dt)
        {
            base.Update(dt);

            if (m_isLoggingOut)
            {
                m_logoutTimer.Update(dt);
            }

            if (!IsMoving && LastSendIamNotMoving < (uint)Environment.TickCount )
            {
                LastSendIamNotMoving = (uint)(Environment.TickCount + CharacterFormulas.TimeBetweenImNotMovingPacketSendMillis);
                Asda2MovmentHandler.SendStartMoveCommonToAreaResponse(this,true,false);
            }
            Asda2MovmentHandler.CalculateAndSetRealPos(this, dt);
            if (Asda2Pet != null)
            {
                if (LastPetExpGainTime < (uint)Environment.TickCount)
                {
                    Asda2Pet.GainXp(1);
                    LastPetExpGainTime = (uint)Environment.TickCount + CharacterFormulas.TimeBetweenPetExpGainSecs * 1000;
                }
                if (!PetNotHungerEnabled && LastPetEatingTime < (uint)Environment.TickCount)
                {
                    if (Asda2Pet.HungerPrc == 1)
                    {
                        Asda2TitleChecker.OnPetStarve(this);
                        //Stop pet
                        Asda2PetHandler.SendPetGoesSleepDueStarvationResponse(Client, Asda2Pet);
                        Asda2Pet.RemoveStatsFromOwner();
                        Asda2Pet.HungerPrc = 0;
                        Asda2Pet = null;
                        GlobalHandler.UpdateCharacterPetInfoToArea(this);
                    }
                    else
                    {
                        Asda2Pet.HungerPrc--;
                        LastPetEatingTime = (uint)Environment.TickCount + CharacterFormulas.TimeBetweenPetEatingsSecs * 1000;
                    }
                }
            }
            if (PremiumBuffs.Count > 0)
            {
                foreach (var functionItemBuff in PremiumBuffs.Values)
                {
                    if (functionItemBuff.Duration < dt)
                    {
                        ProcessFunctionalItemEffect(functionItemBuff, false);
                        CategoryBuffsToDelete.Add(functionItemBuff.Template.Category);
                        functionItemBuff.DeleteLater();
                    }
                    else
                    {
                        functionItemBuff.Duration -= dt;
                    }
                }
            }
            foreach (var functionItemBuff in LongTimePremiumBuffs)
            {
                if (functionItemBuff == null) continue;
                if (functionItemBuff.EndsDate < DateTime.Now)
                {
                    ProcessFunctionalItemEffect(functionItemBuff, false);
                    CategoryBuffsToDelete.Add(functionItemBuff.Template.Category);
                    functionItemBuff.DeleteLater();
                }
            }
            if (CategoryBuffsToDelete.Count > 0)
            {
                foreach (var asda2ItemCategory in CategoryBuffsToDelete)
                {
                    PremiumBuffs.Remove(asda2ItemCategory);
                    for (int i = 0; i < LongTimePremiumBuffs.Length; i++)
                    {
                        if (LongTimePremiumBuffs[i] == null || LongTimePremiumBuffs[i].Template.Category != asda2ItemCategory)
                            continue;
                        LongTimePremiumBuffs[i] = null;
                        break;
                    }
                }
                CategoryBuffsToDelete.Clear();
            }
            var toDelete = new List<Asda2PereodicActionType>();
            foreach (var pereodicAction in PereodicActions)
            {
                pereodicAction.Value.Update(dt);
                if (pereodicAction.Value.CallsNum <= 0)
                    toDelete.Add(pereodicAction.Key);
            }
            foreach (var t in toDelete)
            {
                PereodicActions.Remove(t);
            }
            if (SoulmateRecord != null)
            {
                SoulmateRecord.OnUpdateTick();
            }
            if (BanChatTill < DateTime.Now)
            {
                BanChatTill = null;
                ChatBanned = false;
                SendInfoMsg("Chat is unbanned.");
            }
        }

        public override UpdatePriority UpdatePriority
        {
            get
            {
                return UpdatePriority.HighPriority;
            }
        }
        #endregion

        private void UpdateSettings()
        {
            if (SettingsFlags == null)
                return;
            for (int i = 0; i < SettingsFlags.Length; i++)
            {
                var enabled = SettingsFlags[i] == 1;
                switch ((CharacterSettingsFlag)i)
                {
                    case CharacterSettingsFlag.DisplayHemlet:
                        break;
                    case CharacterSettingsFlag.DisplayMonstrHelath:
                        break;
                    case CharacterSettingsFlag.EnableFriendRequest:
                        EnableFriendRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnableGearTradeRequest:
                        EnableGearTradeRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnableGeneralTradeRequest:
                        EnableGeneralTradeRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnableGuildRequest:
                        EnableGuildRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnablePartyRequest:
                        EnablePartyRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnableSoulmateRequest:
                        EnableSoulmateRequest = enabled;
                        break;
                    case CharacterSettingsFlag.EnableWishpers:
                        EnableWishpers = enabled;
                        break;
                    case CharacterSettingsFlag.ShowSelfNameAndHealth:
                        break;
                }
            }
        }
    }
}