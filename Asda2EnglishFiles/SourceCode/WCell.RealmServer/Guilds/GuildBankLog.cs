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
            this.Bank = bank;
            this.itemLogEntries =
                new StaticCircularList<GuildBankLogEntry>(24,
                    new Action<GuildBankLogEntry>(GuildBankLog.OnEntryDeleted));
            this.moneyLogEntries =
                new StaticCircularList<GuildBankLogEntry>(24,
                    new Action<GuildBankLogEntry>(GuildBankLog.OnEntryDeleted));
        }

        private static void OnEntryDeleted(GuildBankLogEntry obj)
        {
            obj.DeleteLater();
        }

        public GuildBank Bank { get; internal set; }

        internal void LoadLogs()
        {
            foreach (GuildBankLogEntry guildBankLogEntry in GuildBankLogEntry.LoadAll(this.Bank.Guild.Id))
            {
                switch (guildBankLogEntry.Type)
                {
                    case GuildBankLogEntryType.DepositItem:
                        this.itemLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.WithdrawItem:
                        this.itemLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.MoveItem:
                        this.itemLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.DepositMoney:
                        this.moneyLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.WithdrawMoney:
                        this.moneyLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.MoneyUsedForRepairs:
                        this.moneyLogEntries.Insert(guildBankLogEntry);
                        break;
                    case GuildBankLogEntryType.MoveItem_2:
                        this.itemLogEntries.Insert(guildBankLogEntry);
                        break;
                }
            }
        }

        public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, GuildBankTab intoTab)
        {
            this.LogEvent(type, chr, item, item.Amount, intoTab);
        }

        public void LogEvent(GuildBankLogEntryType type, Character chr, ItemRecord item, int amount,
            GuildBankTab intoTab)
        {
            this.LogEvent(type, chr, 0U, item, amount, intoTab);
        }

        public void LogEvent(GuildBankLogEntryType type, Character member, uint money, ItemRecord item, int amount,
            GuildBankTab intoTab)
        {
            switch (type)
            {
                case GuildBankLogEntryType.DepositItem:
                    this.LogItemEvent(type, member, item, amount, intoTab);
                    break;
                case GuildBankLogEntryType.WithdrawItem:
                    this.LogItemEvent(type, member, item, amount, intoTab);
                    break;
                case GuildBankLogEntryType.MoveItem:
                    this.LogItemEvent(type, member, item, amount, intoTab);
                    break;
                case GuildBankLogEntryType.DepositMoney:
                    this.LogMoneyEvent(type, member, money);
                    break;
                case GuildBankLogEntryType.WithdrawMoney:
                    this.LogMoneyEvent(type, member, money);
                    break;
                case GuildBankLogEntryType.MoneyUsedForRepairs:
                    this.LogMoneyEvent(type, member, money);
                    break;
                case GuildBankLogEntryType.MoveItem_2:
                    this.LogItemEvent(type, member, item, amount, intoTab);
                    break;
            }
        }

        private void LogMoneyEvent(GuildBankLogEntryType type, Character actor, uint money)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                GuildBankLogEntry record = new GuildBankLogEntry(this.Bank.Guild.Id)
                {
                    Type = type,
                    Actor = actor,
                    BankLog = this,
                    Money = (int) money,
                    Created = DateTime.Now
                };
                record.CreateLater();
                lock (this.itemLogEntries)
                    this.itemLogEntries.Insert(record);
            }));
        }

        private void LogItemEvent(GuildBankLogEntryType type, Character actor, ItemRecord record, int amount,
            GuildBankTab intoTab)
        {
            GuildBankLogEntry guildBankLogEntry = new GuildBankLogEntry(this.Bank.Guild.Id)
            {
                Type = type,
                Actor = actor,
                BankLog = this,
                DestinationTab = intoTab,
                ItemEntryId = (int) record.EntryId,
                ItemStackCount = amount,
                Created = DateTime.Now
            };
            lock (this.moneyLogEntries)
                this.moneyLogEntries.Insert(guildBankLogEntry);
        }

        public IEnumerable<GuildBankLogEntry> GetBankLogEntries(byte tabId)
        {
            if (tabId == (byte) 6)
            {
                lock (this.moneyLogEntries)
                {
                    foreach (GuildBankLogEntry moneyLogEntry in this.moneyLogEntries)
                    {
                        if (moneyLogEntry.DestinationTabId == (int) tabId)
                            yield return moneyLogEntry;
                    }
                }
            }

            lock (this.itemLogEntries)
            {
                foreach (GuildBankLogEntry itemLogEntry in this.itemLogEntries)
                {
                    if (itemLogEntry.DestinationTabId == (int) tabId)
                        yield return itemLogEntry;
                }
            }
        }
    }
}