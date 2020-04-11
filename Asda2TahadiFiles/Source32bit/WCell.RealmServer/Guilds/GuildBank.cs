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
      Guild = guild;
      BankLog = new GuildBankLog(this);
      if(isNew)
      {
        bankTabs = new GuildBankTab[1]
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
        bankTabs = ActiveRecordBase<GuildBankTab>.FindAllByProperty("_guildId", (int) guild.Id);
        BankLog.LoadLogs();
      }
    }

    public Guild Guild { get; private set; }

    public GuildBankLog BankLog { get; private set; }

    public GuildBankTab this[int tabId]
    {
      get
      {
        GuildBankTab[] bankTabs = this.bankTabs;
        if(tabId >= bankTabs.Length)
          return null;
        return bankTabs[tabId];
      }
    }

    public void DepositMoney(Character depositer, GameObject bankObj, uint deposit)
    {
      if(!CheckBankObj(depositer, bankObj) || deposit == 0U || depositer.Money < deposit)
        return;
      Guild.Money += deposit;
      depositer.SubtractMoney(deposit);
      BankLog.LogEvent(GuildBankLogEntryType.DepositMoney, depositer, deposit, null, 0,
        null);
      GuildHandler.SendGuildBankTabNames(depositer, bankObj);
      GuildHandler.SendGuildBankTabContents(depositer, bankObj, 0);
      GuildHandler.SendGuildBankMoneyUpdate(depositer, bankObj);
    }

    public void WithdrawMoney(Character withdrawer, GameObject bankObj, uint withdrawl)
    {
      if(!CheckBankObj(withdrawer, bankObj) || withdrawl == 0U || Guild.Money < withdrawl)
        return;
      GuildMember guildMember = withdrawer.GuildMember;
      if(guildMember == null)
        return;
      Guild.Money -= withdrawl;
      withdrawer.AddMoney(withdrawl);
      guildMember.BankMoneyWithdrawlAllowance -= withdrawl;
      BankLog.LogEvent(GuildBankLogEntryType.WithdrawMoney, withdrawer, withdrawl, null, 0,
        null);
      GuildHandler.SendMemberRemainingDailyWithdrawlAllowance(withdrawer,
        guildMember.BankMoneyWithdrawlAllowance);
      GuildHandler.SendGuildBankTabNames(withdrawer, bankObj);
      GuildHandler.SendGuildBankTabContents(withdrawer, bankObj, 0);
      GuildHandler.SendGuildBankMoneyUpdate(withdrawer, bankObj);
    }

    public void SwapItemsManualBankToBank(Character chr, GameObject bankObj, byte fromBankTabId, byte fromTabSlot,
      byte toBankTabId, byte toTabSlot, uint itemEntryId, byte amount)
    {
      if(!CheckBankObj(chr, bankObj))
        return;
      GuildMember guildMember = chr.GuildMember;
      if(guildMember == null)
        return;
      GuildRank rank = guildMember.Rank;
      if(rank == null)
        return;
      GuildBankTab intoTab1 = this[fromBankTabId];
      if(intoTab1 == null)
        return;
      GuildBankTab intoTab2 = this[toBankTabId];
      if(intoTab2 == null || fromTabSlot >= 98 || toTabSlot >= 98)
        return;
      GuildBankTabRights[] bankTabRights = rank.BankTabRights;
      if(!bankTabRights[fromBankTabId].Privileges.HasFlag(GuildBankTabPrivileges.ViewTab) ||
         bankTabRights[fromBankTabId].WithdrawlAllowance <= 0U || !bankTabRights[toBankTabId]
           .Privileges.HasFlag(GuildBankTabPrivileges.DepositItem))
        return;
      ItemRecord itemRecord1 = intoTab1[fromTabSlot];
      if(itemRecord1 == null || (int) itemRecord1.EntryId != (int) itemEntryId ||
         itemRecord1.Amount < amount)
        return;
      if(amount == 0)
        amount = (byte) itemRecord1.Amount;
      bool flag = fromBankTabId == toBankTabId;
      if(flag)
      {
        if(fromTabSlot == toTabSlot)
          return;
        ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, amount, toTabSlot, true);
        intoTab1[fromTabSlot] = itemRecord2;
      }
      else
      {
        if(intoTab2.CheckStoreItemInSlot(itemRecord1, amount, toTabSlot, true))
        {
          ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, amount, toTabSlot, true);
          intoTab1[fromTabSlot] = itemRecord2;
        }
        else
        {
          if(!bankTabRights[fromBankTabId].Privileges
               .HasFlag(GuildBankTabPrivileges.DepositItem) ||
             bankTabRights[toBankTabId].WithdrawlAllowance <= 0U)
            return;
          ItemRecord itemRecord2 = intoTab2.StoreItemInSlot(itemRecord1, amount, toTabSlot, true);
          intoTab1[fromTabSlot] = itemRecord2;
          if(itemRecord2 != itemRecord1)
          {
            --bankTabRights[toTabSlot].WithdrawlAllowance;
            --bankTabRights[fromBankTabId].WithdrawlAllowance;
            BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord1, amount, intoTab2);
            BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord2, intoTab1);
            Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot);
            Guild.SendGuildBankTabContentUpdateToAll(toBankTabId, toTabSlot);
            return;
          }
        }

        --bankTabRights[fromBankTabId].WithdrawlAllowance;
      }

      BankLog.LogEvent(GuildBankLogEntryType.MoveItem, chr, itemRecord1, amount, intoTab2);
      Guild.SendGuildBankTabContentUpdateToAll(fromBankTabId, fromTabSlot,
        flag ? toTabSlot : -1);
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
      if(!CheckBankObj(chr, bank))
        return;
      GuildMember guildMember = chr.GuildMember;
      if(guildMember == null)
        return;
      GuildRank rank = guildMember.Rank;
      if(rank == null || !guildMember.IsLeader || (tabId >= 6 || Guild.PurchasedBankTabCount >= 6) ||
         tabId != Guild.PurchasedBankTabCount)
        return;
      int bankTabPrice = BankTabPrices[tabId];
      if(chr.Money < bankTabPrice || !AddNewBankTab(tabId))
        return;
      rank.BankTabRights[tabId].Privileges = GuildBankTabPrivileges.Full;
      rank.BankTabRights[tabId].WithdrawlAllowance = uint.MaxValue;
      GuildHandler.SendGuildRosterToGuildMembers(Guild);
      GuildHandler.SendGuildBankTabNames(chr, bank);
    }

    public void ModifyTabInfo(Character chr, GameObject bank, byte tabId, string newName, string newIcon)
    {
      if(!CheckBankObj(chr, bank))
        return;
      GuildMember guildMember = chr.GuildMember;
      if(guildMember == null || !guildMember.IsLeader ||
         (tabId < 0 || tabId > Guild.PurchasedBankTabCount))
        return;
      GuildBankTab record = this[tabId];
      if(record == null)
        return;
      record.Name = newName;
      record.Icon = newIcon;
      record.UpdateLater();
      GuildHandler.SendGuildBankTabNames(chr, bank);
      GuildHandler.SendGuildBankTabContents(chr, bank, tabId);
    }

    public void GetBankTabText(Character chr, byte tabId)
    {
      if(tabId < 0 || tabId >= 6 || tabId > Guild.PurchasedBankTabCount)
        return;
      GuildBankTab guildBankTab = this[tabId];
      if(guildBankTab == null)
        return;
      GuildHandler.SendGuildBankTabText(chr, tabId, guildBankTab.Text);
    }

    public void SetBankTabText(Character chr, byte tabId, string newText)
    {
      GuildMember guildMember = chr.GuildMember;
      if(guildMember == null)
        return;
      GuildRank rank = guildMember.Rank;
      if(rank == null || tabId < 0 ||
         (tabId >= 6 || tabId > Guild.PurchasedBankTabCount))
        return;
      GuildBankTab record = this[tabId];
      if(record == null || !rank.BankTabRights[tabId].Privileges
           .HasFlag(GuildBankTabPrivileges.UpdateText))
        return;
      record.Text = newText.Length < 501 ? newText : newText.Substring(0, 500);
      record.UpdateLater();
      Guild.Broadcast(GuildHandler.CreateBankTabTextPacket(tabId, newText));
    }

    public void QueryBankLog(Character chr, byte tabId)
    {
      if(tabId < 0 || tabId >= 6 ||
         (tabId > Guild.PurchasedBankTabCount || this[tabId] == null))
        return;
      GuildHandler.SendGuildBankLog(chr, BankLog, tabId);
    }

    private static bool CheckBankObj(Character chr, GameObject bankObj)
    {
      chr.EnsureContext();
      if(bankObj == null)
        return false;
      bankObj.EnsureContext();
      return bankObj.CanBeUsedBy(chr);
    }

    private bool AddNewBankTab(int tabId)
    {
      if(tabId < 0 || tabId >= 6 || this[tabId] != null)
        return false;
      ++Guild.PurchasedBankTabCount;
      ServerApp<RealmServer>.IOQueue.AddMessage(() =>
      {
        GuildBankTab guildBankTab = new GuildBankTab
        {
          Bank = this,
          BankSlot = tabId,
          Icon = "",
          Name = "Slot " + (tabId + 1),
          Text = ""
        };
        int num = (int) ArrayUtil.AddOnlyOne(ref bankTabs, guildBankTab);
        guildBankTab.CreateLater();
      });
      return true;
    }
  }
}