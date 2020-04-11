using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Quests;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// Sequence in Quest packets upon completion:
    /// 
    /// CMSG_QUESTGIVER_COMPLETE_QUEST
    /// SMSG_QUESTGIVER_OFFER_REWARD
    /// CMSG_QUESTGIVER_REQUEST_REWARD
    /// SMSG_QUESTGIVER_QUEST_COMPLETE
    /// SMSG_QUESTGIVER_QUEST_DETAILS
    /// CMSG_QUESTGIVER_CHOOSE_REWARD
    /// CMSG_QUESTGIVER_STATUS_MULTIPLE_QUERY
    /// 
    /// or:
    /// CMSG_QUESTGIVER_COMPLETE_QUEST
    /// SMSG_QUESTGIVER_QUEST_COMPLETE
    /// CMSG_QUESTGIVER_CHOOSE_REWARD
    /// CMSG_QUESTGIVER_STATUS_MULTIPLE_QUERY
    /// 
    /// </summary>
    public static class QuestHandler
    {
        /// <summary>Handles the quest confirm accept.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestConfirmAccept(IRealmClient client, RealmPacketIn packet)
        {
            QuestHandler.SendQuestConfirmAccept(client);
        }

        public static void SendQuestConfirmAccept(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_CONFIRM_ACCEPT))
            {
                packet.Write(0);
                client.Send(packet, false);
            }
        }

        /// <summary>Handles the quest position of interest query.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestPOIQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint count = packet.ReadUInt32();
            List<uint> uintList = new List<uint>();
            for (int index = 0; (long) index < (long) count; ++index)
                uintList.Add(packet.ReadUInt32());
            QuestHandler.SendQuestPOIResponse(client, count, (IEnumerable<uint>) uintList);
        }

        public static void SendQuestPOIResponse(IRealmClient client, uint count, IEnumerable<uint> questIds)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_POI_QUERY_RESPONSE))
            {
                packet.Write(count);
                foreach (uint questId in questIds)
                {
                    List<QuestPOI> questPoiList;
                    QuestMgr.POIs.TryGetValue(questId, out questPoiList);
                    if (questPoiList != null)
                    {
                        packet.Write(questId);
                        packet.Write((uint) questPoiList.Count);
                        foreach (QuestPOI questPoi in questPoiList)
                        {
                            packet.Write(questPoi.PoiId);
                            packet.Write(questPoi.ObjectiveIndex);
                            packet.Write((uint) questPoi.MapID);
                            packet.Write((uint) questPoi.ZoneId);
                            packet.Write(questPoi.FloorId);
                            packet.Write(questPoi.Unk3);
                            packet.Write(questPoi.Unk4);
                            packet.Write((uint) questPoi.Points.Count);
                            foreach (QuestPOIPoints point in questPoi.Points)
                            {
                                packet.Write(point.X);
                                packet.Write(point.Y);
                            }
                        }
                    }
                    else
                    {
                        packet.Write(questId);
                        packet.Write(0U);
                    }
                }

                client.Send(packet, false);
            }
        }

        /// <summary>Handles the quest giver cancel.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverCancel(IRealmClient client, RealmPacketIn packet)
        {
            GossipHandler.SendConversationComplete((IPacketReceiver) client.ActiveCharacter);
        }

        /// <summary>Handles the quest query.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestQuery(IRealmClient client, RealmPacketIn packet)
        {
            QuestTemplate template = QuestMgr.GetTemplate(packet.ReadUInt32());
            if (template == null)
                return;
            QuestHandler.SendQuestQueryResponse(template, client.ActiveCharacter);
        }

        /// <summary>Handles the quest giver hello.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverHello(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            if (questGiver == null)
                return;
            questGiver.StartQuestDialog(activeCharacter);
        }

        /// <summary>Handles the quest giver request reward.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverRequestReward(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            IQuestHolder questGiver = client.ActiveCharacter.QuestLog.GetQuestGiver(guid);
            QuestMgr.GetTemplate(packet.ReadUInt32());
            if (questGiver == null)
                ;
        }

        /// <summary>Handles the quest giver status query.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverStatusQuery(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            if (questGiver == null)
                return;
            QuestStatus questGiverStatus = questGiver.QuestHolderInfo.GetHighestQuestGiverStatus(activeCharacter);
            QuestHandler.SendQuestGiverStatus(questGiver, questGiverStatus, activeCharacter);
        }

        /// <summary>Handles the quest giver accept quest.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverAcceptQuest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            QuestTemplate template = QuestMgr.GetTemplate(packet.ReadUInt32());
            if (template == null || questGiver == null || !questGiver.QuestHolderInfo.QuestStarts.Contains(template))
                return;
            activeCharacter.QuestLog.TryAddQuest(template, questGiver);
        }

        public static void HandleQuestCompletedQuery(IRealmClient client, RealmPacketIn packet)
        {
            QuestHandler.SendQuestCompletedQueryResponse(client.ActiveCharacter);
        }

        public static void SendQuestCompletedQueryResponse(Character chr)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_QUERY_QUESTS_COMPLETED_RESPONSE, 4))
            {
                packet.Write(chr.QuestLog.FinishedQuests.Count);
                foreach (uint finishedQuest in chr.QuestLog.FinishedQuests)
                    packet.Write(finishedQuest);
                chr.Send(packet, false);
            }
        }

        /// <summary>Sends the quest invalid.</summary>
        /// <param name="chr">The character.</param>
        /// <param name="reason">The reason.</param>
        public static void SendQuestInvalid(Character chr, QuestInvalidReason reason)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_QUESTGIVER_QUEST_INVALID, 4))
            {
                packet.Write((int) reason);
                chr.Send(packet, false);
            }
        }

        /// <summary>Sends the quest update complete.</summary>
        /// <param name="chr">The character.</param>
        public static void SendQuestUpdateComplete(Character chr, uint questId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_COMPLETE))
            {
                packet.Write(questId);
                chr.Send(packet, false);
            }
        }

        /// <summary>Sends the quest update failed.</summary>
        /// <param name="qst">The quest.</param>
        /// <param name="chr">The client.</param>
        public static void SendQuestUpdateFailed(Character chr, Quest qst)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_FAILED))
            {
                packet.Write(qst.Template.Id);
                chr.Send(packet, false);
            }
        }

        /// <summary>Sends the quest update failed timer.</summary>
        /// <param name="qst">The QST.</param>
        /// <param name="chr">The character</param>
        public static void SendQuestUpdateFailedTimer(Character chr, Quest qst)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_FAILEDTIMER))
            {
                packet.Write(qst.Template.Id);
                chr.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends the quest update add kill, this should actually cover both GameObject interaction
        /// together with killing the objectBase.
        /// </summary>
        /// <param name="quest">The QST.</param>
        /// <param name="chr">The client.</param>
        /// <param name="obj">The unit.</param>
        public static void SendUpdateInteractionCount(Quest quest, ObjectBase obj, QuestInteractionTemplate interaction,
            uint currentCount, Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTUPDATE_ADD_KILL))
            {
                packet.Write(quest.Template.Id);
                packet.Write(interaction.RawId);
                packet.Write(currentCount);
                packet.Write(interaction.Amount);
                packet.Write((ulong) (obj != null ? obj.EntityId : EntityId.Zero));
                chr.Client.Send(packet, false);
            }
        }

        /// <summary>Sends the quest update add item.</summary>
        /// <param name="chr">The client.</param>
        public static void SendUpdateItems(Asda2ItemId item, int diff, Character chr)
        {
        }

        /// <summary>Sends the quest query response.</summary>
        /// <param name="qt">The quest id.</param>
        /// <param name="chr">The client.</param>
        public static void SendQuestQueryResponse(QuestTemplate qt, Character chr)
        {
            ClientLocale locale = chr.Client.Info.Locale;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUEST_QUERY_RESPONSE))
            {
                packet.Write(qt.Id);
                packet.Write((uint) qt.IsActive);
                packet.Write(qt.Level);
                packet.Write(qt.MinLevel);
                packet.Write(qt.Category);
                packet.Write((uint) qt.QuestType);
                packet.Write(qt.SuggestedPlayers);
                packet.Write((uint) qt.ObjectiveMinReputation.ReputationIndex);
                packet.Write(qt.ObjectiveMinReputation.Value);
                packet.Write((uint) qt.ObjectiveMaxReputation.ReputationIndex);
                packet.Write(qt.ObjectiveMaxReputation.Value);
                packet.Write(qt.FollowupQuestId);
                packet.Write(qt.CalcRewardXp(chr));
                if (qt.Flags.HasFlag((Enum) QuestFlags.HiddenRewards))
                    packet.Write(0);
                else
                    packet.Write(qt.RewMoney);
                packet.Write(qt.MoneyAtMaxLevel);
                packet.Write((uint) qt.CastSpell);
                packet.Write((uint) qt.RewSpell);
                packet.Write(qt.RewHonorAddition);
                packet.WriteFloat(qt.RewHonorMultiplier);
                packet.Write((uint) qt.SrcItemId);
                packet.Write((uint) qt.Flags);
                packet.Write((uint) qt.RewardTitleId);
                packet.Write(qt.PlayersSlain);
                packet.Write(qt.RewardTalents);
                packet.Write(0);
                packet.Write(0);
                if (qt.Flags.HasFlag((Enum) QuestFlags.HiddenRewards))
                {
                    for (int index = 0; index < 4; ++index)
                    {
                        packet.WriteUInt(0U);
                        packet.WriteUInt(0U);
                    }

                    for (int index = 0; index < 6; ++index)
                    {
                        packet.WriteUInt(0U);
                        packet.WriteUInt(0U);
                    }
                }
                else
                {
                    for (int index = 0; index < 4; ++index)
                    {
                        if (index < qt.RewardItems.Length)
                        {
                            packet.Write((uint) qt.RewardItems[index].ItemId);
                            packet.Write(qt.RewardItems[index].Amount);
                        }
                        else
                        {
                            packet.WriteUInt(0U);
                            packet.WriteUInt(0U);
                        }
                    }

                    for (int index = 0; index < 6; ++index)
                    {
                        if (index < qt.RewardChoiceItems.Length)
                        {
                            packet.Write((uint) qt.RewardChoiceItems[index].ItemId);
                            packet.Write(qt.RewardChoiceItems[index].Amount);
                        }
                        else
                        {
                            packet.WriteUInt(0U);
                            packet.WriteUInt(0U);
                        }
                    }
                }

                for (int index = 0; index < 5; ++index)
                    packet.Write((uint) qt.RewardReputations[index].Faction);
                for (int index = 0; index < 5; ++index)
                    packet.Write(qt.RewardReputations[index].ValueId);
                for (int index = 0; index < 5; ++index)
                    packet.Write(qt.RewardReputations[index].Value);
                packet.Write((uint) qt.MapId);
                packet.Write(qt.PointX);
                packet.Write(qt.PointY);
                packet.Write(qt.PointOpt);
                packet.WriteCString(qt.Titles.Localize(locale));
                packet.WriteCString(qt.Instructions.Localize(locale));
                packet.WriteCString(qt.Details.Localize(locale));
                packet.WriteCString(qt.EndTexts.Localize(locale));
                packet.WriteCString(qt.CompletedTexts.Localize(locale));
                for (int index = 0; index < 4; ++index)
                {
                    packet.Write(qt.ObjectOrSpellInteractions[index].RawId);
                    packet.Write(qt.ObjectOrSpellInteractions[index].Amount);
                    packet.Write((uint) qt.CollectableSourceItems[index].ItemId);
                    packet.Write(qt.CollectableSourceItems[index].Amount);
                }

                for (int index = 0; index < 6; ++index)
                {
                    if (index < qt.CollectableItems.Length)
                    {
                        packet.Write((uint) qt.CollectableItems[index].ItemId);
                        packet.Write(qt.CollectableItems[index].Amount);
                    }
                    else
                    {
                        packet.WriteUInt(0U);
                        packet.WriteUInt(0U);
                    }
                }

                for (int index = 0; index < 4; ++index)
                {
                    QuestObjectiveSet objectiveText = qt.ObjectiveTexts[(int) locale];
                    if (objectiveText != null)
                        packet.Write(objectiveText.Texts[index]);
                    else
                        packet.Write("");
                }

                chr.Client.Send(packet, false);
            }
        }

        public static void SendQuestLogFull(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTLOG_FULL))
                chr.Send(packet, false);
        }

        /// <summary>Sends the quest giver quest detail.</summary>
        /// <param name="questGiver">The qg.</param>
        /// <param name="qt">The quest id.</param>
        /// <param name="chr">The client.</param>
        /// <param name="acceptable">if set to <c>true</c> [acceptable].</param>
        public static void SendDetails(IEntity questGiver, QuestTemplate qt, Character chr, bool acceptable)
        {
            ClientLocale locale = chr.Locale;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_DETAILS))
            {
                packet.Write((ulong) (questGiver != null ? questGiver.EntityId : EntityId.Zero));
                packet.Write((ulong) EntityId.Zero);
                packet.Write(qt.Id);
                packet.WriteCString(qt.Titles.Localize(locale));
                packet.WriteCString(qt.Details.Localize(locale));
                packet.WriteCString(qt.Instructions.Localize(locale));
                packet.Write(acceptable ? (byte) 1 : (byte) 0);
                packet.WriteUInt((uint) qt.Flags);
                packet.WriteUInt(qt.SuggestedPlayers);
                packet.Write((byte) 0);
                if (qt.Flags.HasFlag((Enum) QuestFlags.HiddenRewards))
                {
                    packet.WriteUInt(0U);
                    packet.WriteUInt(0U);
                    packet.WriteUInt(0U);
                    packet.WriteUInt(0U);
                }
                else
                {
                    packet.Write(qt.RewardChoiceItems.Length);
                    for (uint index = 0; (long) index < (long) qt.RewardChoiceItems.Length; ++index)
                    {
                        packet.Write((uint) qt.RewardChoiceItems[index].ItemId);
                        packet.Write(qt.RewardChoiceItems[index].Amount);
                        ItemTemplate template = qt.RewardChoiceItems[index].Template;
                        if (template != null)
                            packet.Write(template.DisplayId);
                        else
                            packet.Write(0);
                    }

                    packet.Write(qt.RewardItems.Length);
                    for (uint index = 0; (long) index < (long) qt.RewardItems.Length; ++index)
                    {
                        packet.Write((uint) qt.RewardItems[index].ItemId);
                        packet.Write(qt.RewardItems[index].Amount);
                        ItemTemplate template = qt.RewardItems[index].Template;
                        if (template != null)
                            packet.Write(template.DisplayId);
                        else
                            packet.Write(0);
                    }

                    if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
                        packet.Write(qt.MoneyAtMaxLevel);
                    else
                        packet.Write(qt.RewMoney);
                    packet.Write(qt.CalcRewardXp(chr));
                }

                packet.Write(qt.RewHonorAddition);
                packet.Write(qt.RewHonorMultiplier);
                packet.Write((uint) qt.RewSpell);
                packet.Write((uint) qt.CastSpell);
                packet.Write((uint) qt.RewardTitleId);
                packet.Write(qt.RewardTalents);
                packet.Write(0);
                packet.Write(0);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write((uint) qt.RewardReputations[index].Faction);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write(qt.RewardReputations[index].ValueId);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write(qt.RewardReputations[index].Value);
                packet.Write(4);
                for (int index = 0; index < 4; ++index)
                {
                    EmoteTemplate questDetailedEmote = qt.QuestDetailedEmotes[index];
                    packet.Write((int) questDetailedEmote.Type);
                    packet.Write(questDetailedEmote.Delay);
                }

                chr.Client.Send(packet, false);
            }
        }

        /// <summary>
        /// Offers the reward of the given Quest to the given Character.
        /// When replying the Quest will be complete.
        /// </summary>
        /// <param name="questGiver">The qg.</param>
        /// <param name="qt">The quest id.</param>
        /// <param name="chr">The client.</param>
        public static void SendQuestGiverOfferReward(IEntity questGiver, QuestTemplate qt, Character chr)
        {
            ClientLocale locale = chr.Locale;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_OFFER_REWARD))
            {
                packet.Write((ulong) questGiver.EntityId);
                packet.WriteUInt(qt.Id);
                packet.WriteCString(qt.Titles.Localize(locale));
                packet.WriteCString(qt.OfferRewardTexts.Localize(locale));
                packet.WriteByte(qt.FollowupQuestId > 0U ? (byte) 1 : (byte) 0);
                packet.WriteUInt((uint) qt.Flags);
                packet.WriteUInt(qt.SuggestedPlayers);
                packet.Write(qt.OfferRewardEmotes.Length);
                for (uint index = 0; (long) index < (long) qt.OfferRewardEmotes.Length; ++index)
                {
                    packet.Write(qt.OfferRewardEmotes[index].Delay);
                    packet.Write((uint) qt.OfferRewardEmotes[index].Type);
                }

                packet.Write(qt.RewardChoiceItems.Length);
                for (int index = 0; index < qt.RewardChoiceItems.Length; ++index)
                {
                    packet.Write((uint) qt.RewardChoiceItems[index].ItemId);
                    packet.Write(qt.RewardChoiceItems[index].Amount);
                    ItemTemplate template = qt.RewardChoiceItems[index].Template;
                    if (template != null)
                        packet.Write(template.DisplayId);
                    else
                        packet.Write(0);
                }

                packet.Write(qt.RewardItems.Length);
                for (int index = 0; index < qt.RewardItems.Length; ++index)
                {
                    packet.Write((uint) qt.RewardItems[index].ItemId);
                    packet.Write(qt.RewardItems[index].Amount);
                    ItemTemplate template = qt.RewardItems[index].Template;
                    if (template != null)
                        packet.WriteUInt(template.DisplayId);
                    else
                        packet.Write(0);
                }

                if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
                    packet.Write(qt.MoneyAtMaxLevel);
                else
                    packet.Write(qt.RewMoney);
                packet.Write(qt.CalcRewardXp(chr));
                packet.Write(qt.CalcRewardHonor(chr));
                packet.Write(qt.RewHonorMultiplier);
                packet.Write(8U);
                packet.Write((uint) qt.RewSpell);
                packet.Write((uint) qt.CastSpell);
                packet.Write((uint) qt.RewardTitleId);
                packet.Write(qt.RewardTalents);
                packet.Write(0);
                packet.Write(0);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write((uint) qt.RewardReputations[index].Faction);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write(qt.RewardReputations[index].ValueId);
                for (uint index = 0; index < 5U; ++index)
                    packet.Write(qt.RewardReputations[index].Value);
                chr.Client.Send(packet, false);
            }
        }

        /// <summary>Sends SMSG_QUESTGIVER_REQUEST_ITEMS</summary>
        /// <param name="qg">The qg.</param>
        /// <param name="qt">The qt.</param>
        /// <param name="chr">The client.</param>
        public static void SendRequestItems(IQuestHolder qg, QuestTemplate qt, Character chr, bool closeOnCancel)
        {
        }

        /// <summary>
        /// Sends packet, which informs client about IQuestHolder's status.
        /// </summary>
        /// <param name="qg">The qg.</param>
        /// <param name="status">The status.</param>
        /// <param name="chr">The client.</param>
        public static void SendQuestGiverStatus(IQuestHolder qg, QuestStatus status, Character chr)
        {
        }

        /// <summary>Sends the quest giver quest complete.</summary>
        /// <param name="qt">The quest id.</param>
        /// <param name="chr">The client.</param>
        public static void SendComplete(QuestTemplate qt, Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_COMPLETE))
            {
                packet.Write(qt.Id);
                if (chr.Level >= RealmServerConfiguration.MaxCharacterLevel)
                {
                    packet.Write(0U);
                    packet.Write(qt.MoneyAtMaxLevel);
                }
                else
                {
                    packet.Write(qt.CalcRewardXp(chr));
                    packet.Write(qt.RewMoney);
                }

                packet.Write(qt.CalcRewardHonor(chr));
                packet.Write(qt.RewardTalents);
                packet.Write(0);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendQuestPushResult(Character receiver, QuestPushResponse qpr, Character giver)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_QUEST_PUSH_RESULT))
            {
                packet.Write((ulong) receiver.EntityId);
                packet.Write((byte) qpr);
                giver.Send(packet, false);
            }
        }

        /// <summary>Sends the quest giver quest list.</summary>
        /// <param name="qHolder">The quest giver.</param>
        /// <param name="list">The list.</param>
        /// <param name="chr">The character.</param>
        public static void SendQuestList(IQuestHolder qHolder, List<QuestTemplate> list, Character chr)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut(new PacketId(RealmServerOpCode.SMSG_QUESTGIVER_QUEST_LIST)))
            {
                packet.Write((ulong) qHolder.EntityId);
                if (qHolder.QuestHolderInfo == null)
                    return;
                packet.Write("Stay a while and listen...");
                packet.Write(0U);
                packet.Write(1U);
                int num = Math.Min(20, list.Count);
                packet.Write((byte) num);
                foreach (QuestTemplate questTemplate in list)
                {
                    packet.Write(questTemplate.Id);
                    Quest activeQuest = chr.QuestLog.GetActiveQuest(questTemplate.Id);
                    if (activeQuest != null)
                    {
                        if (activeQuest.CompleteStatus == QuestCompleteStatus.Completed)
                            packet.Write(4);
                        else
                            packet.Write(5U);
                    }
                    else
                    {
                        uint availability = (uint) questTemplate.GetAvailability(chr);
                        packet.Write(availability);
                    }

                    packet.WriteUInt(questTemplate.Level);
                    packet.WriteUInt((uint) questTemplate.Flags);
                    packet.Write((byte) 0);
                    packet.WriteCString(questTemplate.DefaultTitle);
                }

                chr.Client.Send(packet, false);
            }
        }

        /// <summary>Handles CMSG_QUESTGIVER_CHOOSE_REWARD.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverChooseReward(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId guid = packet.ReadEntityId();
            uint qid = packet.ReadUInt32();
            Quest questById = activeCharacter.QuestLog.GetQuestById(qid);
            if (questById == null)
                return;
            uint rewardSlot = packet.ReadUInt32();
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            if (questGiver == null || questById.CompleteStatus != QuestCompleteStatus.Completed)
                return;
            questById.TryFinish(questGiver, rewardSlot);
        }

        /// <summary>Handles the quest giver complete quest.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverCompleteQuest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            if (questGiver == null)
                return;
            uint qid = packet.ReadUInt32();
            Quest questById = activeCharacter.QuestLog.GetQuestById(qid);
            if (questById == null || !questGiver.QuestHolderInfo.QuestEnds.Contains(questById.Template))
                return;
            if (questById.CompleteStatus != QuestCompleteStatus.Completed)
                QuestHandler.SendRequestItems(questGiver, questById.Template, activeCharacter, false);
            else
                questById.OfferQuestReward(questGiver);
        }

        /// <summary>Handles the quest giver query quest.</summary>
        public static void HandleQuestGiverQueryQuest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId guid = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            uint num = packet.ReadUInt32();
            IQuestHolder questGiver = activeCharacter.QuestLog.GetQuestGiver(guid);
            QuestTemplate template = QuestMgr.GetTemplate(num);
            if (questGiver == null || template == null)
                return;
            if (!activeCharacter.QuestLog.HasActiveQuest(num))
            {
                bool flag = template.Flags.HasFlag((Enum) QuestFlags.AutoAccept);
                QuestHandler.SendDetails((IEntity) questGiver, template, activeCharacter, !flag);
                if (!flag)
                    return;
                activeCharacter.QuestLog.TryAddQuest(template, questGiver);
            }
            else
                QuestHandler.SendRequestItems(questGiver, template, activeCharacter, false);
        }

        /// <summary>Handles the quest log remove quest.</summary>
        public static void HandleQuestLogRemoveQuest(IRealmClient client, RealmPacketIn packet)
        {
            byte slot = packet.ReadByte();
            Quest questBySlot = client.ActiveCharacter.QuestLog.GetQuestBySlot(slot);
            if (questBySlot == null)
                return;
            questBySlot.Cancel(false);
        }

        /// <summary>Handles the questgiver status multiple query.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestgiverStatusMultipleQuery(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.FindAndSendAllNearbyQuestGiverStatuses();
        }

        public static void HandlePushQuestToParty(IRealmClient client, RealmPacketIn packet)
        {
            QuestTemplate qt = QuestMgr.GetTemplate(packet.ReadUInt32());
            if (qt == null || !qt.Sharable || client.ActiveCharacter.QuestLog.GetActiveQuest(qt.Id) == null)
                return;
            Group group = client.ActiveCharacter.Group;
            if (group == null)
                return;
            group.ForeachCharacter((Action<Character>) (chr =>
            {
                if (chr == null)
                    return;
                if (chr.QuestLog.ActiveQuestCount >= 25)
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.QuestlogFull, client.ActiveCharacter);
                else if (chr.QuestLog.GetActiveQuest(qt.Id) != null)
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.AlreadyHave, client.ActiveCharacter);
                else if (chr.QuestLog.FinishedQuests.Contains(qt.Id) && !qt.Repeatable)
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.AlreadyFinished, client.ActiveCharacter);
                else if (qt.CheckBasicRequirements(chr) != QuestInvalidReason.Ok || !chr.IsAlive)
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.CannotTake, client.ActiveCharacter);
                else if (!chr.IsInRadius((WorldObject) client.ActiveCharacter, 30f))
                {
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.TooFar, client.ActiveCharacter);
                }
                else
                {
                    QuestHandler.SendQuestPushResult(chr, QuestPushResponse.Sharing, client.ActiveCharacter);
                    QuestHandler.SendDetails((IEntity) client.ActiveCharacter, qt, chr, true);
                }
            }));
        }

        public static void HandleQuestPushResult(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            byte num = packet.ReadByte();
            Character giver = client.ActiveCharacter.Map.GetObject(id) as Character;
            if (giver == null || client.ActiveCharacter.Group == null || client.ActiveCharacter.Group != giver.Group)
                return;
            QuestHandler.SendQuestPushResult(client.ActiveCharacter, (QuestPushResponse) num, giver);
        }

        /// <summary>Handles the quest log swap quest.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestLogSwapQuest(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles the quest giver query autolaunch.</summary>
        /// <param name="client">The client.</param>
        /// <param name="packet">The packet.</param>
        public static void HandleQuestGiverQueryAutoLaunch(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleFlagQuest(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleFlagQuestFinish(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleClearQuest(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void SendQuestForceRemoved(IRealmClient client, QuestTemplate quest)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_QUEST_FORCE_REMOVE, 4))
            {
                packet.Write(quest.Id);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Finds and sends all surrounding QuestGiver's current Quest-Status to the given Character
        /// </summary>
        /// <param name="chr">The <see cref="T:WCell.RealmServer.Entities.Character" />.</param>
        public static void FindAndSendAllNearbyQuestGiverStatuses(this Character chr)
        {
        }
    }
}