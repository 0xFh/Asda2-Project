using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.RealmServer.Res;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2LoginHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly byte[] unk51 = new byte[28];
        private static readonly byte[] stab31 = new byte[3];

        /// <summary>Triggered after an Account logs into the Realm-server</summary>
        public static event Action<RealmAccount> AccountLogin;

        /// <summary>Triggered before a client disconnects</summary>
        public static event Func<IRealmClient, CharacterRecord, CharacterRecord> BeforeLogin;

        /// <summary>Sends an auth session success response to the client.</summary>
        /// <param name="client">the client to send to</param>
        public static void InviteToRealm(IRealmClient client)
        {
            Action<RealmAccount> accountLogin = Asda2LoginHandler.AccountLogin;
            if (accountLogin != null)
                accountLogin(client.Account);
            ServerApp<WCell.RealmServer.RealmServer>.Instance.OnClientAccepted((object) null, (EventArgs) null);
        }

        [ClientPacketHandler(RealmServerOpCode.SelectServerRequest, IsGamePacket = false, RequiresLogin = false)]
        public static void SelectServerRequest(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Handles an incoming player login request.</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.EnterGameRequset, IsGamePacket = false, RequiresLogin = false)]
        public static void PlayerLoginRequestLS(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.IsConnected || client.AuthAccount == null || client.Account == null)
            {
                client.Disconnect(false);
            }
            else
            {
                if (client.ActiveCharacter != null && client.ActiveCharacter.IsConnected)
                    return;
                packet.Position += 5;
                byte num1 = packet.ReadByte();
                if (num1 < (byte) 10 || num1 > (byte) 12)
                {
                    client.Disconnect(false);
                }
                else
                {
                    uint num2 = (uint) (client.Account.AccountId + 1000000 * (int) num1);
                    if (client.Account.GetCharacterRecord(num2) == null)
                        client.Disconnect(false);
                    else
                        Asda2LoginHandler.PreLoginCharacter(client, num2, true);
                }
            }
        }

        public static void PreLoginCharacter(IRealmClient client, uint charLowId, bool isLoginStep)
        {
            try
            {
                Character chr = World.GetCharacter(charLowId);
                client.Info = new ClientInformation();
                if (chr != null)
                {
                    chr.Client.Disconnect(false);
                    client.ActiveCharacter = chr;
                    chr.Map.AddMessage((IMessage) new Message((Action) (() =>
                    {
                        if (!chr.IsInContext)
                        {
                            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                                (IMessage) new Message((Action) (() =>
                                    Asda2LoginHandler.LoginCharacter(client, charLowId, isLoginStep))));
                        }
                        else
                        {
                            if (isLoginStep)
                            {
                                chr.IsLoginServerStep = true;
                                chr.IsFirstGameConnection = true;
                            }

                            chr.ReconnectCharacter(client);
                            if (!isLoginStep)
                                return;
                            chr.Client.Disconnect(true);
                        }
                    })));
                }
                else
                    Asda2LoginHandler.LoginCharacter(client, charLowId, isLoginStep);
            }
            catch (Exception ex)
            {
                Asda2LoginHandler.log.Error((object) ex);
                Asda2LoginHandler.SendCharacterLoginFail((IPacketReceiver) client, LoginErrorCode.CHAR_LOGIN_FAILED);
            }
        }

        private static void LoginCharacter(IRealmClient client, uint charLowId, bool isLoginStep)
        {
            RealmAccount account = client.Account;
            if (account == null)
                return;
            CharacterRecord record = client.Account.GetCharacterRecord(charLowId);
            if (record == null)
            {
                Asda2LoginHandler.log.Error(string.Format(WCell_RealmServer.CharacterNotFound, (object) charLowId,
                    (object) account.Name));
                client.Disconnect(false);
            }
            else
            {
                if (client.ActiveCharacter != null)
                    return;
                Character character = (Character) null;
                try
                {
                    Func<IRealmClient, CharacterRecord, CharacterRecord> beforeLogin = Asda2LoginHandler.BeforeLogin;
                    if (beforeLogin != null)
                    {
                        record = beforeLogin(client, record);
                        if (record == null)
                            throw new ArgumentNullException("record", "BeforeLogin returned null");
                    }

                    character = record.CreateCharacter();
                    if (isLoginStep)
                    {
                        character.IsLoginServerStep = true;
                        character.IsFirstGameConnection = true;
                    }

                    character.Create(account, record, client);
                    character.LoadAndLogin();
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Failed to load Character from Record: " + (object) record,
                        new object[0]);
                    if (character == null)
                        return;
                    character.Dispose();
                    client.Disconnect(false);
                }
            }
        }

        /// <summary>
        /// Sends a "character login failed" error message to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="error">the actual login error</param>
        public static void SendCharacterLoginFail(IPacketReceiver client, LoginErrorCode error)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_CHARACTER_LOGIN_FAILED, 1))
            {
                packet.WriteByte((byte) error);
                client.Send(packet, false);
            }
        }

        public static void SendEnterGameResposeResponse(IRealmClient client)
        {
            if (client.Account == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EnterGameRespose))
            {
                RealmAccount account = client.Account;
                Character activeCharacter = account.ActiveCharacter;
                if (activeCharacter == null)
                    return;
                packet.WriteInt32(account.AccountId);
                packet.WriteFixedAsciiString(activeCharacter.Name, 20, Locale.Start);
                packet.WriteInt16(activeCharacter.Record.CharNum);
                packet.WriteByte(activeCharacter.Record.Zodiac);
                packet.WriteByte((byte) activeCharacter.Gender);
                packet.WriteByte(activeCharacter.ProfessionLevel);
                packet.WriteByte((byte) activeCharacter.Archetype.ClassId);
                packet.WriteByte(activeCharacter.Level);
                packet.WriteUInt32(activeCharacter.Experience);
                packet.WriteInt32(0);
                packet.WriteInt16(activeCharacter.Spells.AvalibleSkillPoints);
                packet.WriteInt16(0);
                packet.WriteByte(0);
                packet.WriteInt16(15000);
                packet.WriteInt16(1000);
                packet.WriteInt64(0L);
                packet.WriteByte(0);
                packet.WriteByte(3);
                packet.WriteInt16(((int) client.ActiveCharacter.Record.PremiumWarehouseBagsCount + 1) * 30);
                packet.WriteInt16(
                    ((IEnumerable<Asda2Item>) client.ActiveCharacter.Asda2Inventory.WarehouseItems).Count<Asda2Item>(
                        (Func<Asda2Item, bool>) (i => i != null)));
                packet.WriteInt16(((int) client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount + 1) * 30);
                packet.WriteInt16(
                    ((IEnumerable<Asda2Item>) client.ActiveCharacter.Asda2Inventory.AvatarWarehouseItems)
                    .Count<Asda2Item>((Func<Asda2Item, bool>) (i => i != null)));
                packet.WriteByte(0);
                packet.WriteByte(1);
                packet.WriteInt16(-1);
                packet.WriteInt16(0);
                packet.WriteByte(activeCharacter.Record.Zodiac);
                packet.WriteByte(activeCharacter.HairStyle);
                packet.WriteByte(activeCharacter.HairColor);
                packet.WriteByte(activeCharacter.Facial);
                packet.WriteInt32(activeCharacter.Health);
                packet.WriteInt16(activeCharacter.Power);
                packet.WriteInt32(activeCharacter.MaxHealth);
                packet.WriteInt16(activeCharacter.MaxPower);
                packet.WriteInt16((short) activeCharacter.MinDamage);
                packet.WriteInt16((short) activeCharacter.MaxDamage);
                packet.WriteInt16((short) activeCharacter.RangedAttackPower);
                packet.WriteInt16((short) activeCharacter.RangedAttackPower);
                packet.WriteInt16(activeCharacter.ArcaneResist);
                packet.WriteInt16(activeCharacter.Defense);
                packet.WriteInt16(activeCharacter.Defense);
                packet.WriteInt32(activeCharacter.BlockValue);
                packet.WriteInt32(activeCharacter.BlockValue);
                packet.WriteInt16(15);
                packet.WriteInt16(7);
                packet.WriteInt16(4);
                packet.WriteSkip(Asda2LoginHandler.unk51);
                packet.WriteSkip(activeCharacter.SettingsFlags);
                packet.WriteInt32(activeCharacter.AvatarMask);
                for (int index = 11; index < 20; ++index)
                {
                    Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, false);
            }
        }

        public static void SendEnterGameResponseItemsOnCharacterResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EnterGameResponseItemsOnCharacter))
            {
                packet.WriteByte(1);
                for (int index = 0; index < 12; ++index)
                {
                    Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(0);
                    packet.WriteInt32(asda2Item == null ? -1 : index);
                    packet.WriteInt32(0);
                    packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Durability);
                    packet.WriteInt16(0);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul1Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul2Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul3Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul4Id);
                    packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Enchant);
                    packet.WriteInt16(0);
                    packet.WriteByte(0);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr1Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr1Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr2Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr2Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr3Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr3Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr4Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr4Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr5Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr5Value);
                    packet.WriteByte(0);
                    packet.WriteByte(asda2Item == null ? 0 : 1);
                    packet.WriteInt32(0);
                    packet.WriteInt16(0);
                }

                client.Send(packet, false);
            }
        }

        public static void SendEnterWorldIpeResponseResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EnterWorldIpeResponse))
            {
                packet.WriteInt32(-1);
                if (client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 20, Locale.Start);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 20, Locale.Start);
                packet.WriteUInt16(RealmServerConfiguration.Port);
                packet.WriteInt16((short) client.ActiveCharacter.MapId);
                packet.WriteInt16(Convert.ToInt16(client.ActiveCharacter.Position.X));
                packet.WriteInt16(Convert.ToInt16(client.ActiveCharacter.Position.Y));
                Aura[] auraArray = new Aura[28];
                int num1 = 0;
                foreach (Aura activeAura in client.ActiveCharacter.Auras.ActiveAuras)
                {
                    if (activeAura.TicksLeft > 0)
                    {
                        auraArray[num1++] = activeAura;
                        if (auraArray.Length <= num1)
                            break;
                    }
                }

                for (int index = 0; index < 28; ++index)
                {
                    Aura aura = auraArray[index];
                    packet.WriteInt16(aura == null ? -1 : (int) aura.Spell.RealId);
                    packet.WriteInt16(aura == null ? -1 : (int) aura.Spell.RealId);
                    packet.WriteByte(aura == null ? 0 : 1);
                    packet.WriteByte(0);
                    packet.WriteByte(2);
                    packet.WriteInt16(aura == null ? 0 : aura.Duration / 1000);
                    packet.WriteByte(1);
                    packet.WriteInt16(1);
                }

                FunctionItemBuff[] functionItemBuffArray = new FunctionItemBuff[15];
                int num2 = 0;
                foreach (KeyValuePair<Asda2ItemCategory, FunctionItemBuff> premiumBuff in client.ActiveCharacter
                    .PremiumBuffs)
                {
                    if (!premiumBuff.Value.IsLongTime)
                        functionItemBuffArray[num2++] = premiumBuff.Value;
                }

                for (int index = 0; index < 15; ++index)
                {
                    FunctionItemBuff functionItemBuff = functionItemBuffArray[index];
                    packet.WriteInt32(-1);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(-1);
                    packet.WriteInt32(-1);
                    packet.WriteInt32(0);
                    packet.WriteInt16(-1);
                }

                client.Send(packet, false);
            }
        }

        public static void SendLongTimeBuffsInfoResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.LongTimeBuffsInfo))
            {
                for (int index = 0; index < 20; ++index)
                {
                    FunctionItemBuff longTimePremiumBuff = client.ActiveCharacter.LongTimePremiumBuffs[index];
                    packet.WriteInt16(longTimePremiumBuff == null ? -1 : longTimePremiumBuff.Template.PackageId);
                    packet.WriteInt16(longTimePremiumBuff == null ? -1 : longTimePremiumBuff.ItemId);
                    packet.WriteInt32(longTimePremiumBuff == null
                        ? -1
                        : (int) (long) (longTimePremiumBuff.EndsDate - DateTime.Now).TotalSeconds);
                }

                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.LocationInit, IsGamePacket = false, RequiresLogin = false)]
        public static void LocationInitRequest(IRealmClient client, RealmPacketIn packet)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                using (RealmPacketOut packet1 = new RealmPacketOut(RealmServerOpCode.ClientCanLoginToGS))
                {
                    packet1.WriteByte(2);
                    client.Send(packet1, false);
                }
            }));
        }

        [PacketHandler(RealmServerOpCode.CharacterInitOnLogin, IsGamePacket = false, RequiresLogin = false)]
        public static void CharacterInitOnLoginRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = packet.ReadInt32();
            packet.Position += 2;
            short num2 = packet.ReadInt16();
            if (num2 < (short) 10 || num2 > (short) 12)
            {
                client.TcpSocket.Close();
            }
            else
            {
                packet.Position += 4;
                uint num3 = (uint) (num1 + (int) num2 * 1000000);
                string str = client.ClientAddress.ToString();
                Account account = AccountMgr.GetAccount((long) num1);
                if (account == null || account.LastIPStr != str)
                {
                    if (account != null)
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) num1)
                            .AddAttribute("operation", 1.0, "login_game_server_bad_ip")
                            .AddAttribute("name", 0.0, account.Name)
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString())
                            .AddAttribute("old_ip", 0.0, account.LastIPStr).Write();
                    client.Disconnect(false);
                }
                else
                {
                    RealmAccount loggedInAccount =
                        ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(account.Name);
                    if (loggedInAccount == null || loggedInAccount.ActiveCharacter == null)
                    {
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) num1)
                            .AddAttribute("operation", 1.0, "login_game_server_no_character_selected")
                            .AddAttribute("name", 0.0, account.Name)
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString())
                            .AddAttribute("old_ip", 0.0, account.LastIPStr).Write();
                        client.Disconnect(false);
                    }
                    else
                    {
                        client.IsGameServerConnection = true;
                        client.Account = loggedInAccount;
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) num1)
                            .AddAttribute("operation", 1.0, "login_game_server").AddAttribute("name", 0.0, account.Name)
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString())
                            .AddAttribute("character", (double) client.Account.ActiveCharacter.EntryId,
                                client.Account.ActiveCharacter.Name).AddAttribute("chrLowId", (double) num3, "")
                            .Write();
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Character, num3)
                            .AddAttribute("operation", 1.0, "login_game_server")
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                        Asda2LoginHandler.PreLoginCharacter(client, num3, false);
                    }
                }
            }
        }

        [PacketHandler(RealmServerOpCode.CharacterInitOnChanelChange, IsGamePacket = false, RequiresLogin = false)]
        public static void CharacterInitOnChanelChangeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client == null || client.ClientAddress == null)
                return;
            int num = packet.ReadInt32();
            Account account = AccountMgr.GetAccount((long) num);
            if (account == null || account.LastIPStr != client.ClientAddress.ToString())
            {
                if (account != null)
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) num)
                        .AddAttribute("operation", 1.0, "login_on_map_change_bad_ip")
                        .AddAttribute("name", 0.0, account.Name)
                        .AddAttribute("ip", 0.0, client.ClientAddress.ToString())
                        .AddAttribute("old_ip", 0.0, account.LastIPStr).Write();
                client.Disconnect(false);
            }
            else
            {
                RealmAccount loggedInAccount =
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(account.Name);
                if (loggedInAccount == null || loggedInAccount.ActiveCharacter == null)
                {
                    client.Disconnect(false);
                }
                else
                {
                    client.IsGameServerConnection = true;
                    client.Account = loggedInAccount;
                    LogHelperEntry lgDelete = Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) num)
                        .AddAttribute("operation", 1.0, "login_on_map_change").AddAttribute("name", 0.0, account.Name)
                        .AddAttribute("chr", (double) loggedInAccount.ActiveCharacter.EntryId,
                            loggedInAccount.ActiveCharacter.Name)
                        .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Character,
                            loggedInAccount.ActiveCharacter.EntityId.Low)
                        .AddAttribute("operation", 1.0, "login_on_map_change")
                        .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).AddAttribute("chr",
                            (double) loggedInAccount.ActiveCharacter.EntryId, loggedInAccount.ActiveCharacter.Name)
                        .AddReference(lgDelete).Write();
                    Asda2LoginHandler.PreLoginCharacter(client, loggedInAccount.ActiveCharacter.EntityId.Low, false);
                }
            }
        }

        public static void SendInventoryInfoResponse(IRealmClient client)
        {
            Asda2PlayerInventory asda2Inventory = client.ActiveCharacter.Asda2Inventory;
            List<List<Asda2Item>> asda2ItemListList = new List<List<Asda2Item>>();
            int count1 = 0;
            Asda2Item[] array1 = ((IEnumerable<Asda2Item>) asda2Inventory.RegularItems)
                .Where<Asda2Item>((Func<Asda2Item, bool>) (it => it != null)).ToArray<Asda2Item>();
            while (count1 < array1.Length)
            {
                asda2ItemListList.Add(new List<Asda2Item>(((IEnumerable<Asda2Item>) array1).Skip<Asda2Item>(count1)
                    .Take<Asda2Item>(9)));
                count1 += 9;
            }

            foreach (List<Asda2Item> asda2ItemList in asda2ItemListList)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RegularInventoryInfo))
                {
                    for (int index = 0; index < asda2ItemList.Count; ++index)
                    {
                        Asda2Item asda2Item = asda2ItemList[index];
                        if (asda2Item != null)
                            Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                    }

                    client.Send(packet, false);
                }
            }

            asda2ItemListList.Clear();
            int count2 = 0;
            Asda2Item[] array2 = ((IEnumerable<Asda2Item>) asda2Inventory.ShopItems)
                .Where<Asda2Item>((Func<Asda2Item, bool>) (it => it != null)).ToArray<Asda2Item>();
            while (count2 < array2.Length)
            {
                asda2ItemListList.Add(new List<Asda2Item>(((IEnumerable<Asda2Item>) array2).Skip<Asda2Item>(count2)
                    .Take<Asda2Item>(9)));
                count2 += 9;
            }

            foreach (List<Asda2Item> asda2ItemList in asda2ItemListList)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShopInventoryInfo))
                {
                    for (int index = 0; index < asda2ItemList.Count; ++index)
                    {
                        Asda2Item asda2Item = asda2ItemList[index];
                        if (asda2Item != null)
                            Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                    }

                    client.Send(packet, false);
                }
            }
        }
    }
}