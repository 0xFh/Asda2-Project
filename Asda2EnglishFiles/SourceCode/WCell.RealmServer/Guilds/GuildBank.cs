using Castle.ActiveRecord;
using System;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
    public class GuildBank
    {
        private static readonly int[] BankTabPrices = new int[6]
        {
            1000000,
            2500000,
            5000000,
            10000000,
            25000000,
            50000000
        };

        private GuildBankTab[] bankTabs;

        /// <summary>
        /// 
        /// </summary>
        internal GuildBank(Guild guild, bool isNew)
        {
            this.Guild = guild;
            this.BankLog = new GuildBankLog(this);
            if (isNew)
            {
                this.bankTabs = new GuildBankTab[1]
                {
                    new GuildBankTab(this)
                    {
                        BankSlot = 0,
                        Icon = "",
                        Name = "Slot 0",
                        Text = ""
                    }
                };
            }
            else
            {
                this.bankTabs = ActiveRecordBase<GuildBankTab>.FindAllByProperty("_guildId", (object) (int) guild.Id);
                this.BankLog.LoadLogs();
            }
        }

        public Guild Guild { get; private set; }

        public GuildBankLog BankLog { get; private set; }

        public GuildBankTab this[int tabId]
        {
            get
            {
                GuildBankTab[] bankTabs = this.bankTabs;
                if (tabId >= bankTabs.Length)
                    return (GuildBankTab) null;
                return bankTabs[tabId];
            }
        }

        public void DepositMoney(Character depositer, GameObject bankObj, uint deposit)
        {
            if (!GuildBank.CheckBankObj(depositer, bankObj) || deposit == 0U || depositer.Money < deposit)
                return;
            this.Guild.Money += (long) deposit;
            depositer.SubtractMoney(deposit);
            this.BankLog.LogEvent(GuildBankLogEntryType.DepositMoney, depositer, deposit, (ItemRecord) null, 0,
                (GuildBankTab) null);
            GuildHandler.SendGuildBankTabNames(depositer, bankObj);
            GuildHandler.SendGuildBankTabContents(depositer, bankObj, (byte) 0);
            GuildHandler.SendGuildBankMoneyUpdate(depositer, bankObj);
        }

        public void WithdrawMoney(Character withdrawer, GameObject bankObj, uint withdrawl)
        {
            if (!GuildBank.CheckBankObj(withdrawer, bankObj) || withdrawl == 0U || this.Guild.Money < (long) withdrawl)
                return;
            GuildMember guildMember = withdrawer.GuildMember;
            if (guildMember == null)
                return;
            this.Guild.Money -= (long) withdrawl;
            withdrawer.AddMoney(withdrawl);
            guildMember.BankMoneyWithdrawlAllowance -= withdrawl;
            this.BankLog.LogEvent(GuildBankLogEntryType.WithdrawMoney, withdrawer, withdrawl, (ItemRecord) null, 0,
                (GuildBankTab) null);
            GuildHandler.SendMemberRemainingDailyWithdrawlAllowance((IPacketReceiver) withdrawer,
                guildMember.BankMoneyWithdrawlAllowance);
            GuildHandler.SendGuildBankTabNames(withdrawer, bankObj);
            GuildHandler.SendGuildBankTabContents(withdrawer, bankObj, (byte) 0);
            GuildHandler.SendGuildBankMoneyUpdate(withdrawer, bankObj);
        }

        public void SwapItemsManualBankToBank(Character chr, GameObject bankObj, byte fromBankTabId, byte fromTabSlot,
            byte toBankTabId, byte toTabSlot, uint itemEntryId, byte amount)
        {
            if (!GuildBank.CheckBankObj(chr, bankObj))
                return;
            GuildMember guildMember = chr.GuildMember;
            if (guildMember == null)
                return;
            GuildRank rank = guildMember.Rank;
            if (rank == null)
                return;
            GuildBankTab intoTab1 = this[(int) fromBankTabId];
            if (intoTab1 == null)
                return;
            GuildBankTab intoTab2 = this[(int) toBankTabId];
            if (intoTab2 == null || fromTabSlot >= (byte) 98 || toTabSlot >= (byte) 98)
                return;
            GuildBankTabRights[] bankTabRights = rank.BankTabRights;
            if (!bankTabRights[(int) fromBankTabId].Privileges.HasFlag((Enum) GuildBankTabPrivileges.ViewTab) ||
                bankTabRights[(int) fromBankTabId].WithdrawlAllowance <= 0U || !bankTabRights[(int) toBankTabId]
                    .Privileges.HasFlag((Enum) GuildBankTabPrivileges.DepositItem))
                return;
            ItemRecord itemRecord1 = intoTab1[(int) fromTabSlot];
            if (itemRecord1 == null || (int) itemRecord1.EntryId != (int) itemEntryId ||
                itemRecord1.Amount < (int) amount)
                return;
            if (amount == (byte) 0)
                amount = (byte) itemRecord1.Amount;
            bool flag = (int) fromBankTabId == (int) toBankTabId;
            if (flag)
            {
                if ((int) fromTabSlot == (int) toTabSlot)
                    return;
                ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, (int) amount, (int) toTabSlot, true);
                intoTab1[(int) fromTabSlot] = itemRecord2;
            }
            else
            {
                if (intoTab2.CheckStoreItemInSlot(itemRecord1, (int) amount, (int) toTabSlot, true))
                {
                    ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, (int) amount, (int) toTabSlot, true);
                    intoTab1[(int) fromTabSlot] = itemRecord2;
                }
                else
                {
                    if (!bankTabRights[(int) fromBankTabId].Privileges
                            .HasFlag((Enum) GuildBankTabPrivileges.DepositItem) ||
                        bankTabRights[(int) toBankTabId].WithdrawlAllowance <= 0U)
                        return;
                    ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, (int) amount, (int) toTabSlot, true);
                    intoTab1[(int) fromTabSlot] = itemRecord2;
                    if (itemRecord2 != itemRecord1)
                    {
                        --bankTabRights[(int) toTabSlot].WithdrawlAllowance;
                        --bankTabRights[(int) fromBankTabId].WithdrawlAllowance;
                        this.BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord1, (int) amount, intoTab2);
                        this.BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord2, intoTab1);
                        this.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, (int) fromTabSlot);
                        this.Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, (int) toTabSlot);
                        return;
                    }
                }

                --bankTabRights[(int) fromBankTabId].WithdrawlAllowance;
            }

            this.BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord1, (int) amount, intoTab2);
            this.Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, (int) fromTabSlot,
                flag ? (int) toTabSlot : -1);
        }

        public void SwapItemsAutoStoreBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot,
            uint itemEntryId, byte autoStoreCount)
        {
        }

        public void SwapItemsManualBankToChar(Character chr, GameObject bank, byte fromBankTabId, byte fromTabSlot,
            byte bagSlot, byte slot, uint itemEntryId, byte amount)
        {
        }

        public void SwapItemsAutoStoreCharToBank(Character chr, GameObject bank, byte toBankTabId, byte bagSlot,
            byte slot, uint itemEntryId, byte autoStoreCount)
        {
        }

        public void SwapItemsManualCharToBank(Character chr, GameObject bank, byte bagSlot, byte slot, uint itemEntryId,
            byte toBankTabId, byte toTabSlot, byte amount)
        {
        }

        public void BuyTab(Character chr, GameObject bank, byte tabId)
        {
            if (!GuildBank.CheckBankObj(chr, bank))
                return;
            GuildMember guildMember = chr.GuildMember;
            if (guildMember == null)
                return;
            GuildRank rank = guildMember.Rank;
            if (rank == null || !guildMember.IsLeader || (tabId >= (byte) 6 || this.Guild.PurchasedBankTabCount >= 6) ||
                (int) tabId != this.Guild.PurchasedBankTabCount)
                return;
            int bankTabPrice = GuildBank.BankTabPrices[(int) tabId];
            if ((long) chr.Money < (long) bankTabPrice || !this.AddNewBankTab((int) tabId))
                return;
            rank.BankTabRights[(int) tabId].Privileges = GuildBankTabPrivileges.Full;
            rank.BankTabRights[(int) tabId].WithdrawlAllowance = uint.MaxValue;
            GuildHandler.SendGuildRosterToGuildMembers(this.Guild);
            GuildHandler.SendGuildBankTabNames(chr, bank);
        }

        public void ModifyTabInfo(Character chr, GameObject bank, byte tabId, string newName, string newIcon)
        {
            if (!GuildBank.CheckBankObj(chr, bank))
                return;
            GuildMember guildMember = chr.GuildMember;
            if (guildMember == null || !guildMember.IsLeader ||
                (tabId < (byte) 0 || (int) tabId > this.Guild.PurchasedBankTabCount))
                return;
            GuildBankTab record = this[(int) tabId];
            if (record == null)
                return;
            record.Name = newName;
            record.Icon = newIcon;
            record.UpdateLater();
            GuildHandler.SendGuildBankTabNames(chr, bank);
            GuildHandler.SendGuildBankTabContents(chr, bank, tabId);
        }

        public void GetBankTabText(Character chr, byte tabId)
        {
            if (tabId < (byte) 0 || tabId >= (byte) 6 || (int) tabId > this.Guild.PurchasedBankTabCount)
                return;
            GuildBankTab guildBankTab = this[(int) tabId];
            if (guildBankTab == null)
                return;
            GuildHandler.SendGuildBankTabText(chr, tabId, guildBankTab.Text);
        }

        public void SetBankTabText(Character chr, byte tabId, string newText)
        {
            GuildMember guildMember = chr.GuildMember;
            if (guildMember == null)
                return;
            GuildRank rank = guildMember.Rank;
            if (rank == null || tabId < (byte) 0 ||
                (tabId >= (byte) 6 || (int) tabId > this.Guild.PurchasedBankTabCount))
                return;
            GuildBankTab record = this[(int) tabId];
            if (record == null || !rank.BankTabRights[(int) tabId].Privileges
                    .HasFlag((Enum) GuildBankTabPrivileges.UpdateText))
                return;
            record.Text = newText.Length < 501 ? newText : newText.Substring(0, 500);
            record.UpdateLater();
            this.Guild.Broadcast(GuildHandler.CreateBankTabTextPacket(tabId, newText));
        }

        public void QueryBankLog(Character chr, byte tabId)
        {
            if (tabId < (byte) 0 || tabId >= (byte) 6 ||
                ((int) tabId > this.Guild.PurchasedBankTabCount || this[(int) tabId] == null))
                return;
            GuildHandler.SendGuildBankLog(chr, this.BankLog, tabId);
        }

        private static bool CheckBankObj(Character chr, GameObject bankObj)
        {
            chr.EnsureContext();
            if (bankObj == null)
                return false;
            bankObj.EnsureContext();
            return bankObj.CanBeUsedBy(chr);
        }

        private bool AddNewBankTab(int tabId)
        {
            if (tabId < 0 || tabId >= 6 || this[tabId] != null)
                return false;
            ++this.Guild.PurchasedBankTabCount;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                GuildBankTab guildBankTab = new GuildBankTab()
                {
                    Bank = this,
                    BankSlot = tabId,
                    Icon = "",
                    Name = "Slot " + (object) (tabId + 1),
                    Text = ""
                };
                int num = (int) ArrayUtil.AddOnlyOne<GuildBankTab>(ref this.bankTabs, guildBankTab);
                guildBankTab.CreateLater();
            }));
            return true;
        }
    }
}