using System;
using System.Collections.Generic;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
  public class GuildBankLog
  {
    public const int MAX_ENTRIES = 24;
    private readonly StaticCircularList<GuildBankLogEntry> itemLogEntries;
    private readonly StaticCircularList<GuildBankLogEntry> moneyLogEntries;

    public GuildBankLog(GuildBank bank)
    {
      Bank = bank;
      itemLogEntries =
        new StaticCircularList<GuildBankLogEntry>(24,
          OnEntryDeleted);
      moneyLogEntries =
        new StaticCircularList<GuildBankLogEntry>(24,
          OnEntryDeleted);
    }

    private static void OnEntryDeleted(GuildBankLogEntry obj)
    {
      obj.DeleteLater();
    }

    public GuildBank Bank { get; internal set; }

    internal void LoadLogs()
    {
      foreach(GuildBankLogEntry guildBankLogEntry in GuildBankLogEntry.LoadAll(Bank.Guild.Id))
      {
        switch(guildBankLogEntry.Type)
        {
          case GuildBankLogEntryType.DepositItem:
            itemLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.WithdrawItem:
            itemLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.MoveItem:
            itemLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.DepositMoney:
            moneyLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.WithdrawMoney:
            moneyLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.MoneyUsedForRepairs:
            moneyLogEntries.Insert(guildBankLogEntry);
            break;
          case GuildBankLogEntryType.MoveItem_2:
            itemLogEntries.Insert(guildBankLogEntry);
            break;
        }
      }
    }

    public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, GuildBankTab intoTab)
    {
      LogEvent(type, chr, item, item.Amount, intoTab);
    }

    public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, int amount,
      GuildBankTab intoTab)
    {
      LogEvent(type, chr, 0U, item, amount, intoTab);
    }

    public void LogEvent(GuildBankLogEntryType type, Character member, uint money, ItemRecord item, int amount,
      GuildBankTab intoTab)
    {
      switch(type)
      {
        case GuildBankLogEntryType.DepositItem:
          LogItemEvent(type, member, item, amount, intoTab);
          break;
        case GuildBankLogEntryType.WithdrawItem:
          LogItemEvent(type, member, item, amount, intoTab);
          break;
        case GuildBankLogEntryType.MoveItem:
          LogItemEvent(type, member, item, amount, intoTab);
          break;
        case GuildBankLogEntryType.DepositMoney:
          LogMoneyEvent(type, member, money);
          break;
        case GuildBankLogEntryType.WithdrawMoney:
          LogMoneyEvent(type, member, money);
          break;
        case GuildBankLogEntryType.MoneyUsedForRepairs:
          LogMoneyEvent(type, member, money);
          break;
        case GuildBankLogEntryType.MoveItem_2:
          LogItemEvent(type, member, item, amount, intoTab);
          break;
      }
    }

    private void LogMoneyEvent(GuildBankLogEntryType type, Character actor, uint money)
    {
      ServerApp<RealmServer>.IOQueue.AddMessage(() =>
      {
        GuildBankLogEntry record = new GuildBankLogEntry(Bank.Guild.Id)
        {
          Type = type,
          Actor = actor,
          BankLog = this,
          Money = (int) money,
          Created = DateTime.Now
        };
        record.CreateLater();
        lock(itemLogEntries)
          itemLogEntries.Insert(record);
      });
    }

    private void LogItemEvent(GuildBankLogEntryType type, Character actor, ItemRecord record, int amount,
      GuildBankTab intoTab)
    {
      GuildBankLogEntry guildBankLogEntry = new GuildBankLogEntry(Bank.Guild.Id)
      {
        Type = type,
        Actor = actor,
        BankLog = this,
        DestinationTab = intoTab,
        ItemEntryId = (int) record.EntryId,
        ItemStackCount = amount,
        Created = DateTime.Now
      };
      lock(moneyLogEntries)
        moneyLogEntries.Insert(guildBankLogEntry);
    }

    public IEnumerable<GuildBankLogEntry> GetBankLogEntries(byte tabId)
    {
      if(tabId == 6)
      {
        lock(moneyLogEntries)
        {
          foreach(GuildBankLogEntry moneyLogEntry in moneyLogEntries)
          {
            if(moneyLogEntry.DestinationTabId == tabId)
              yield return moneyLogEntry;
          }
        }
      }

      lock(itemLogEntries)
      {
        foreach(GuildBankLogEntry itemLogEntry in itemLogEntries)
        {
          if(itemLogEntry.DestinationTabId == tabId)
            yield return itemLogEntry;
        }
      }
    }
  }
}