using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Chat;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Events.Asda2;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Chat
{
  /// <summary>Manager for sending and receiving chat messages.</summary>
  public static class ChatMgr
  {
    /// <summary>
    /// The radius in which people can hear a someone else say something.
    /// </summary>
    public static float ListeningRadius = 50f;

    /// <summary>
    /// The radius in which people can hear a someone else say something.
    /// </summary>
    public static float YellRadius = 300f;

    /// <summary>
    /// Whether normal say/yell/emote is global. (sent to everyone in the world)
    /// </summary>
    public static bool GlobalChat = true;

    /// <summary>
    /// Whether chat messages of opposite factions will be scrambled
    /// </summary>
    public static bool ScrambleChat = true;

    public static readonly ChatParserDelegate[] ChatParsers = new ChatParserDelegate[51];

    /// <summary>
    /// The start of the lowercase alphabet in the ASCII table.
    /// </summary>
    private const int LowercaseAlphabetStart = 97;

    /// <summary>The end of the lowercase alphabet in the ASCII table.</summary>
    private const int LowercaseAlphabetEnd = 122;

    static ChatMgr()
    {
      ChatParsers[1] = SayYellEmoteParser;
      ChatParsers[6] = SayYellEmoteParser;
      ChatParsers[10] = SayYellEmoteParser;
      ChatParsers[2] = GroupParser;
      ChatParsers[3] = SubGroupParser;
      ChatParsers[39] = SubGroupParser;
      ChatParsers[40] = SubGroupParser;
      ChatParsers[4] = GuildParser;
      ChatParsers[5] = OfficerParser;
      ChatParsers[7] = WhisperParser;
      ChatParsers[17] = ChannelParser;
      ChatParsers[23] = AFKParser;
      ChatParsers[24] = AFKParser;
    }

    /// <summary>Parses any incoming say, yell, or emote messages.</summary>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void SayYellEmoteParser(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet)
    {
      string msg = ReadMessage(packet);
      if(msg.Length == 0)
        return;
      sender.SayYellEmote(type, language, msg,
        type == ChatMsgType.Yell ? YellRadius : ListeningRadius);
    }

    public static void SayYellEmote(this Character sender, ChatMsgType type, ChatLanguage language, string msg,
      float radius)
    {
      if(RealmCommandHandler.HandleCommand(sender, msg,
           sender.Target as Character) ||
         type != ChatMsgType.WhisperInform && msg.Length == 0)
        return;
      if(GlobalChat)
      {
        using(RealmPacketOut charChatMessage = CreateCharChatMessage(type, language, sender.EntityId,
          sender.EntityId, null, msg, sender.ChatTag))
        {
          foreach(Character allCharacter in World.GetAllCharacters())
            allCharacter.Send(charChatMessage, false);
        }
      }
      else
      {
        FactionGroup faction = sender.FactionGroup;
        RealmPacketOut pckt = null;
        RealmPacketOut scrambledPckt = null;
        bool scrambleDefault = ScrambleChat && sender.Role.ScrambleChat;
        Func<WorldObject, bool> predicate = obj =>
        {
          if(obj is Character)
          {
            Character character = (Character) obj;
            if(!scrambleDefault || character.FactionGroup == faction || !character.Role.ScrambleChat)
            {
              if(pckt == null)
                pckt = CreateCharChatMessage(type, language, sender.EntityId, sender.EntityId,
                  null, msg, sender.ChatTag);
              character.Send(pckt, false);
            }
            else
            {
              if(scrambledPckt == null)
                scrambledPckt = CreateCharChatMessage(type, language, sender.EntityId,
                  sender.EntityId, null, ScrambleMessage(msg), sender.ChatTag);
              character.Send(scrambledPckt, false);
            }
          }

          return true;
        };
        if(radius == (double) WorldObject.BroadcastRange)
          sender.NearbyObjects.Iterate(predicate);
        else
          sender.IterateEnvironment(radius, predicate);
        if(pckt != null)
          pckt.Close();
        if(scrambledPckt == null)
          return;
        scrambledPckt.Close();
      }
    }

    /// <summary>Parses any incoming party or raid messages.</summary>
    /// <param name="sender">The character sending the message</param>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void GroupParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
    {
      string msg = ReadMessage(packet);
      if(msg.Length == 0)
        return;
      sender.SayGroup(language, msg);
    }

    public static void SayGroup(this Character sender, ChatLanguage language, string msg)
    {
      if(RealmCommandHandler.HandleCommand(sender, msg,
        sender.Target as Character))
        return;
      Group group = sender.Group;
      if(group == null)
        return;
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Party,
        ChatLanguage.Universal, sender, sender, null, msg, sender.ChatTag))
        group.SendAll(charChatMessage);
    }

    /// <summary>Parses any incoming party or raid messages.</summary>
    /// <param name="sender">The character sending the message</param>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void SubGroupParser(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet)
    {
      string msg = ReadMessage(packet);
      if(msg.Length == 0 || RealmCommandHandler.HandleCommand(sender, msg,
           sender.Target as Character))
        return;
      SubGroup subGroup = sender.SubGroup;
      if(subGroup == null)
        return;
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(type, ChatLanguage.Universal,
        sender, sender, null, msg))
        subGroup.Send(charChatMessage, null);
    }

    /// <summary>Parses any incoming guild message.</summary>
    /// <param name="sender">The character sending the message</param>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void GuildParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
    {
      string msg = ReadMessage(packet);
      if(msg.Length == 0)
        return;
      RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character);
    }

    /// <summary>Parses any incoming officer message.</summary>
    /// <param name="sender">The character sending the message</param>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void OfficerParser(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet)
    {
      string msg = ReadMessage(packet);
      if(msg.Length == 0)
        return;
      RealmCommandHandler.HandleCommand(sender, msg, sender.Target as Character);
    }

    /// <summary>Parses any incoming whispers.</summary>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void WhisperParser(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet)
    {
      string str = packet.ReadCString();
      string msg = ReadMessage(packet);
      if(msg.Length == 0 || RealmCommandHandler.HandleCommand(sender, msg,
           sender.Target as Character))
        return;
      Character character = World.GetCharacter(str, false);
      if(character == null)
        SendChatPlayerNotFoundReply(sender.Client, str);
      else if(character.Faction.Group != sender.Faction.Group)
      {
        SendChatPlayerWrongTeamReply(sender.Client);
      }
      else
      {
        if(character.IsIgnoring(sender))
        {
          using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Ignored,
            ChatLanguage.Universal, character, sender, null, msg))
            sender.Send(charChatMessage, false);
        }
        else
        {
          using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Whisper,
            ChatLanguage.Universal, sender, character, null, msg))
            character.Send(charChatMessage, false);
        }

        using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.MsgReply,
          ChatLanguage.Universal, character, character, null, msg,
          sender.ChatTag))
          sender.Send(charChatMessage, false);
        if(character.IsAFK)
        {
          using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.AFK,
            ChatLanguage.Universal, character, sender, null,
            character.AFKReason, character.ChatTag))
            sender.Send(charChatMessage, false);
        }

        if(!character.IsDND)
          return;
        using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.DND,
          ChatLanguage.Universal, character, sender, null, string.Empty,
          character.ChatTag))
          sender.Send(charChatMessage, false);
      }
    }

    /// <summary>Parses any incoming channel messages.</summary>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    private static void ChannelParser(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet)
    {
      string channelName = packet.ReadCString();
      string str = packet.ReadCString();
      if(RealmCommandHandler.HandleCommand(sender, str,
        sender.Target as Character))
        return;
      ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel(sender, channelName);
      if(chatChannel == null)
        return;
      chatChannel.SendMessage(sender, str);
    }

    private static void AFKParser(Character sender, ChatMsgType type, ChatLanguage language, RealmPacketIn packet)
    {
      string str = packet.ReadCString();
      if(type == ChatMsgType.AFK)
      {
        sender.IsAFK = !sender.IsAFK;
        sender.AFKReason = sender.IsAFK ? str : "";
      }

      if(type != ChatMsgType.DND)
        return;
      sender.IsDND = !sender.IsDND;
      sender.DNDReason = sender.IsDND ? str : "";
    }

    /// <summary>Sends a message that the whisper target wasn't found.</summary>
    /// <param name="client">the client to send to</param>
    /// <param name="recipient">the name of the target player</param>
    public static void SendChatPlayerNotFoundReply(IPacketReceiver client, string recipient)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAT_PLAYER_NOT_FOUND))
      {
        packet.WriteCString(recipient);
        client.Send(packet, false);
      }
    }

    /// <summary>
    /// Sends a message that the whisper target isn't the same faction.
    /// </summary>
    /// <param name="client">the client to send to</param>
    /// <param name="recipient">the name of the target player</param>
    public static void SendChatPlayerWrongTeamReply(IPacketReceiver client)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAT_WRONG_FACTION))
        client.Send(packet, false);
    }

    private static RealmPacketOut CreateObjectChatMessage(ChatMsgType type, ChatLanguage language, INamedEntity obj)
    {
      string name = obj.Name;
      RealmPacketOut realmPacketOut =
        new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT, 31 + name.Length + 50);
      realmPacketOut.Write((byte) type);
      realmPacketOut.Write((uint) language);
      realmPacketOut.Write(obj.EntityId);
      realmPacketOut.Write(0);
      realmPacketOut.WriteUIntPascalString(name);
      realmPacketOut.Write(0L);
      return realmPacketOut;
    }

    /// <summary>
    /// Creates a chat message packet for a non-player object.
    /// </summary>
    /// <param name="type">the type of chat message</param>
    /// <param name="language">the language the message is in</param>
    /// <param name="obj">the object "saying" the message</param>
    /// <param name="msg">the message itself</param>
    /// <param name="tag">any chat tags for the object</param>
    /// <returns>the generated chat packet</returns>
    private static RealmPacketOut CreateObjectChatMessage(ChatMsgType type, ChatLanguage language, INamedEntity obj,
      string msg, ChatTag tag)
    {
      RealmPacketOut objectChatMessage = CreateObjectChatMessage(type, language, obj);
      objectChatMessage.WriteUIntPascalString(msg);
      objectChatMessage.Write((byte) tag);
      return objectChatMessage;
    }

    /// <summary>Creates a chat message packet for a player.</summary>
    /// <param name="type">the type of chat message</param>
    /// <param name="language">the language the message is in</param>
    /// <param name="id1">the ID of the chatter</param>
    /// <param name="id2">the ID of the receiver</param>
    /// <param name="target">the target or null (if its an area message)</param>
    /// <param name="msg">the message itself</param>
    /// <param name="tag">the chat tag of the chatter</param>
    private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, IEntity id1,
      IEntity id2, string target, string msg, ChatTag tag)
    {
      return CreateCharChatMessage(type, language, id1.EntityId, id2.EntityId, target, msg, tag);
    }

    /// <summary>Creates a chat message packet for a player.</summary>
    /// <param name="type">the type of chat message</param>
    /// <param name="language">the language the message is in</param>
    /// <param name="id1">the ID of the chatter</param>
    /// <param name="id2">the ID of the receiver</param>
    /// <param name="target">the target or null (if its an area message)</param>
    /// <param name="msg">the message itself</param>
    private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, IChatter id1,
      IChatter id2, string target, string msg)
    {
      return CreateCharChatMessage(type, language, id1.EntityId, id2.EntityId, target, msg, id1.ChatTag);
    }

    /// <summary>Creates a chat message packet for a player.</summary>
    /// <param name="type">the type of chat message</param>
    /// <param name="language">the language the message is in</param>
    /// <param name="id1">the ID of the chatter</param>
    /// <param name="id2">the ID of the receiver</param>
    /// <param name="target">the target or null (if its an area message)</param>
    /// <param name="msg">the message itself</param>
    /// <param name="tag">the chat tag of the chatter</param>
    /// <returns>Might return null</returns>
    private static RealmPacketOut CreateCharChatMessage(ChatMsgType type, ChatLanguage language, EntityId id1,
      EntityId id2, string target, string msg, ChatTag tag)
    {
      return CreateGlobalChatMessage("Server", msg, Color.Red, Locale.Start);
    }

    /// <summary>Sends a system message.</summary>
    /// <param name="target">the receiver of the message</param>
    /// <param name="message">the message to send</param>
    public static void SendSystemMessage(IPacketReceiver target, string message)
    {
      using(RealmPacketOut globalChatMessage =
        CreateGlobalChatMessage("~Server~", message, Color.DodgerBlue, target.Locale))
        target.Send(globalChatMessage, false);
    }

    public static void SendMessage(IPacketReceiver target, string sender, string message, Color c)
    {
      using(RealmPacketOut globalChatMessage =
        CreateGlobalChatMessage(sender, message, c, target.Locale))
        target.Send(globalChatMessage, false);
    }

    /// <summary>
    /// Sends a system message.
    /// TODO: Improve performance
    /// </summary>
    /// <param name="targets">an enumerable collection of players to send the message to</param>
    /// <param name="message">the message to send</param>
    public static void SendSystemMessage(this IEnumerable<Character> targets, TranslatableItem item)
    {
      targets.SendSystemMessage(item.Key, item.Args);
    }

    public static void SendSystemMessage(this IEnumerable<Character> targets, string[] texts, params object[] args)
    {
      foreach(Character target in targets)
      {
        if(target != null)
          target.SendSystemMessage(texts.Localize(target.Locale, args));
      }
    }

    /// <summary>Sends a system message.</summary>
    /// <param name="targets">an enumerable collection of players to send the message to</param>
    /// <param name="message">the message to send</param>
    public static void SendSystemMessage(this IEnumerable<Character> targets, RealmLangKey langKey,
      params object[] args)
    {
      foreach(Character target in targets)
      {
        if(target != null)
          target.SendSystemMessage(langKey, args);
      }
    }

    /// <summary>Sends a system message.</summary>
    /// <param name="targets">an enumerable collection of players to send the message to</param>
    /// <param name="message">the message to send</param>
    public static void SendSystemMessage(this IEnumerable<Character> targets, string message, params object[] args)
    {
      targets.SendSystemMessage(string.Format(message, args));
    }

    /// <summary>Sends a system message.</summary>
    /// <param name="targets">an enumerable collection of players to send the message to</param>
    /// <param name="message">the message to send</param>
    public static void SendSystemMessage(this IEnumerable<Character> targets, string message)
    {
      foreach(Character target in targets)
      {
        if(target != null)
        {
          using(RealmPacketOut globalChatMessage =
            CreateGlobalChatMessage("~Server~", message, Color.DodgerBlue, target.Client.Locale))
            target.Send(globalChatMessage, false);
        }
      }
    }

    /// <summary>
    /// Sends the amount of experience gained to the characters combat log.
    /// </summary>
    /// <param name="target">the character to receieve the combat log message</param>
    /// <param name="message">the message to display in the characters combat log</param>
    public static void SendCombatLogExperienceMessage(IPacketReceiver target, ClientLocale locale, RealmLangKey key,
      params object[] args)
    {
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.CombatXPGain,
        ChatLanguage.Universal, EntityId.Zero, EntityId.Zero, null,
        RealmLocalizer.Instance.Translate(locale, key, args), ChatTag.None))
        target.Send(charChatMessage, false);
    }

    /// <summary>Sends a whisper from one player to another.</summary>
    /// <param name="sender">the sender of the whisper</param>
    /// <param name="receiver">the target of the whisper</param>
    /// <param name="message">the message to send</param>
    public static void SendWhisper(IChatter sender, IChatter receiver, string message)
    {
      if(message.Length == 0)
        return;
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Whisper,
        sender.SpokenLanguage, sender, receiver, null, message))
        receiver.Send(charChatMessage, false);
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.WhisperInform,
        sender.SpokenLanguage, sender, receiver, null, message))
        sender.Send(charChatMessage, false);
    }

    /// <summary>Sends a whisper from one player to another.</summary>
    /// <param name="sender">the sender of the whisper</param>
    /// <param name="receiver">the target of the whisper</param>
    /// <param name="message">the message to send</param>
    public static void SendRaidBossWhisper(WorldObject sender, IChatter receiver, string message)
    {
      if(message.Length == 0)
        return;
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.RaidBossWhisper,
        ChatLanguage.Universal, sender.EntityId, receiver.EntityId, sender.Name, message, ChatTag.None))
        receiver.Send(charChatMessage, false);
    }

    /// <summary>Sends a message to guild chat.</summary>
    /// <param name="sender">the sender/guild member of the message</param>
    /// <param name="message">the message to send</param>
    internal static void SendGuildMessage(IChatter sender, Guild guild, string message)
    {
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Guild,
        sender.SpokenLanguage, sender, sender, null, message))
        guild.SendToChatListeners(charChatMessage);
    }

    /// <summary>Sends an officer-only message to guild chat.</summary>
    /// <param name="sender">the sender/guild member of the message</param>
    /// <param name="message">the message to send</param>
    internal static void SendGuildOfficerMessage(IChatter sender, Guild guild, string message)
    {
      using(RealmPacketOut charChatMessage = CreateCharChatMessage(ChatMsgType.Officer,
        sender.SpokenLanguage, sender, sender, null, message))
        guild.SendToOfficers(charChatMessage);
    }

    /// <summary>Sends a monster message.</summary>
    /// <param name="obj">the monster the message is being sent from</param>
    /// <param name="chatType">the type of message</param>
    /// <param name="language">the language to send the message in</param>
    /// <param name="message">the message to send</param>
    public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language,
      string message)
    {
      SendMonsterMessage(obj, chatType, language, message,
        chatType == ChatMsgType.MonsterYell ? YellRadius : ListeningRadius);
    }

    /// <summary>Sends a monster message.</summary>
    /// <param name="obj">the monster the message is being sent from</param>
    /// <param name="chatType">the type of message</param>
    /// <param name="language">the language to send the message in</param>
    /// <param name="message">the message to send</param>
    public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language,
      string[] localizedMsgs)
    {
      SendMonsterMessage(obj, chatType, language, localizedMsgs,
        chatType == ChatMsgType.MonsterYell ? YellRadius : ListeningRadius);
    }

    /// <summary>Sends a monster message.</summary>
    /// <param name="obj">the monster the message is being sent from</param>
    /// <param name="chatType">the type of message</param>
    /// <param name="language">the language to send the message in</param>
    /// <param name="message">the message to send</param>
    /// <param name="radius">The radius or -1 to be heard by everyone in the Map</param>
    public static void SendMonsterMessage(WorldObject obj, ChatMsgType chatType, ChatLanguage language,
      string message, float radius)
    {
      if(obj == null)
        return;
      int num = obj.IsAreaActive ? 1 : 0;
    }

    public static void SendMonsterMessage(WorldObject chatter, ChatMsgType chatType, ChatLanguage language,
      string[] localizedMsgs, float radius)
    {
      if(chatter == null || !chatter.IsAreaActive)
        return;
      RealmPacketOut packet = CreateObjectChatMessage(chatType, language, chatter);
      try
      {
        chatter.IterateEnvironment(radius, obj =>
        {
          if(obj is Character)
          {
            packet.WriteUIntPascalString(localizedMsgs.Localize(((Character) obj).Client.Info.Locale));
            packet.Write(chatter is Unit ? (byte) ((Unit) chatter).ChatTag : (byte) 0);
            ((Character) obj).Send(packet.GetFinalizedPacket());
          }

          return true;
        });
      }
      finally
      {
        if(packet != null)
          packet.Dispose();
      }
    }

    /// <summary>
    /// Converts chat channel flags from DBC format to client format.
    /// </summary>
    /// <param name="dbcFlags">the DBC chat channel flags</param>
    /// <returns>converted client chat channel flags</returns>
    public static ChatChannelFlagsClient Convert(ChatChannelFlags dbcFlags)
    {
      ChatChannelFlagsClient channelFlagsClient = ChatChannelFlagsClient.Predefined;
      if(dbcFlags.HasFlag(ChatChannelFlags.Trade))
        channelFlagsClient |= ChatChannelFlagsClient.Trade;
      if(dbcFlags.HasFlag(ChatChannelFlags.CityOnly))
        channelFlagsClient |= ChatChannelFlagsClient.CityOnly;
      return !dbcFlags.HasFlag(ChatChannelFlags.LookingForGroup)
        ? channelFlagsClient | ChatChannelFlagsClient.FFA
        : channelFlagsClient | ChatChannelFlagsClient.LFG;
    }

    /// <summary>
    /// Reads a string from a packet, and treats it like a chat message, purifying it.
    /// </summary>
    /// <param name="packet">the packet to read from</param>
    /// <returns>the purified chat message</returns>
    private static string ReadMessage(RealmPacketIn packet)
    {
      string msg = packet.ReadCString();
      ChatUtility.Purify(ref msg);
      return msg;
    }

    /// <summary>
    /// Takes a string, and scrambles any letters or numbers with random letters.
    /// </summary>
    /// <param name="originalMsg">the original unscrambled string</param>
    /// <returns>the randomized/scrambled string</returns>
    private static string ScrambleMessage(string originalMsg)
    {
      Random random = new Random(1132532542);
      StringBuilder stringBuilder = new StringBuilder(originalMsg.Length);
      for(int index = 0; index < originalMsg.Length; ++index)
      {
        if(char.IsLetterOrDigit(originalMsg[index]))
          stringBuilder.Append((char) random.Next(97, 122));
        else
          stringBuilder.Append(originalMsg[index]);
      }

      return stringBuilder.ToString();
    }

    /// <summary>Triggers a chat notification event.</summary>
    /// <param name="chatter">the person chatting</param>
    /// <param name="message">the chat message</param>
    /// <param name="language">the chat language</param>
    /// <param name="chatType">the type of chat</param>
    /// <param name="target">the target of the message (channel, whisper, etc)</param>
    public static void ChatNotify(IChatter chatter, string message, ChatLanguage language, ChatMsgType chatType,
      IGenericChatTarget target)
    {
      ChatNotifyDelegate messageSent = MessageSent;
      if(messageSent == null)
        return;
      messageSent(chatter, message, language, chatType, target);
    }

    public static bool IsYell(this ChatMsgType type)
    {
      if(type != ChatMsgType.Yell)
        return type == ChatMsgType.MonsterYell;
      return true;
    }

    /// <summary>Event for chat notifications.</summary>
    public static event ChatNotifyDelegate MessageSent;

    [PacketHandler(RealmServerOpCode.NormalChat)]
    public static void NormalChatRequest(IRealmClient client, RealmPacketIn packet)
    {
      packet.Position -= 2;
      string str = packet.ReadAsciiString(client.Locale);
      if(str.Length < 1 || RealmCommandHandler.HandleCommand(client.ActiveCharacter, str,
           client.ActiveCharacter.Target as Character))
        return;
      if(client.ActiveCharacter.ChatBanned)
      {
        client.ActiveCharacter.SendInfoMsg("Your chat is banned.");
      }
      else
      {
        bool flag = client.Locale == Locale.Start || Asda2EncodingHelper.IsPrueEnglish(str);
        client.ActiveCharacter.SendPacketToArea(
          CreateNormalChatMessagePacket(client.ActiveCharacter.Name, str, client.Locale,
            client.ActiveCharacter), true, false, flag ? Locale.Any : client.Locale, new float?());
      }
    }

    public static void SendSystemChatResponse(IRealmClient client, string msg)
    {
      client.Send(CreateNormalChatMessagePacket("System", msg, Locale.Start, null), false);
    }

    public static RealmPacketOut CreateNormalChatMessagePacket(string sender, string message, Locale locale,
      Character chr = null)
    {
      RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.NormalChatResponse);
      realmPacketOut.WriteInt16(chr.SessionId);
      realmPacketOut.WriteInt32(chr.AccId);
      realmPacketOut.WriteFixedAsciiString(sender, 20, locale);
      realmPacketOut.WriteAsciiString(message, locale);
      return realmPacketOut;
    }

    [PacketHandler(RealmServerOpCode.WishperChatRequest)]
    public static void WishperChatRequest(IRealmClient client, RealmPacketIn packet)
    {
      packet.Position -= 24;
      byte soulmate = packet.ReadByte();
      string name = packet.ReadAsdaString(20, Locale.Start);
      string msg = packet.ReadAsciiString(client.Locale);
      if(msg.Length > 100)
      {
        client.ActiveCharacter.SendSystemMessage(
          string.Format("Can't send wishper to {0} cause it's length more than 100 symbols.", name));
      }
      else
      {
        if(msg.Length < 1 || RealmCommandHandler.HandleCommand(client.ActiveCharacter, msg,
             client.ActiveCharacter.Target as Character))
          return;
        Character character = World.GetCharacter(name, false);
        if(character == null)
          client.ActiveCharacter.SendSystemMessage(
            string.Format("Can't send wishper to {0} cause can't found it.", name));
        else if(!client.ActiveCharacter.EnableWishpers)
          client.ActiveCharacter.SendSystemMessage(string.Format("Sorry, but {0} rejects all wishpers.",
            name));
        else if(client.ActiveCharacter.ChatBanned)
        {
          client.ActiveCharacter.SendInfoMsg("Your chat is banned.");
        }
        else
        {
          SendWishperChatResponse(client, soulmate, client.ActiveCharacter.SessionId,
            client.ActiveCharacter.SessionId, client.ActiveCharacter.Name, msg, null);
          SendWishperChatResponse(character.Client, soulmate, client.ActiveCharacter.SessionId,
            character.SessionId, client.ActiveCharacter.Name, msg, client);
        }
      }
    }

    public static void SendWishperChatResponse(IRealmClient recieverClient, byte soulmate, int senderAccId,
      short rcvSessId, string sender, string msg, IRealmClient senderClient = null)
    {
      if(!Asda2EncodingHelper.IsPrueEnglish(msg) && senderClient != null)
      {
        int locale = (int) senderClient.Locale;
        if(senderClient.Locale != recieverClient.Locale)
        {
          senderClient.Send(
            CreateGlobalChatMessage("Chat manager",
              "You can send only english text to this character", Color.Red, Locale.Start), false);
          return;
        }
      }

      AchievementProgressRecord progressRecord =
        recieverClient.ActiveCharacter.Achievements.GetOrCreateProgressRecord(7U);
      switch(++progressRecord.Counter)
      {
        case 1500:
          recieverClient.ActiveCharacter.DiscoverTitle(Asda2TitleId.Whispering45);
          break;
        case 3000:
          recieverClient.ActiveCharacter.GetTitle(Asda2TitleId.Whispering45);
          break;
      }

      progressRecord.SaveAndFlush();
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WishperChat))
      {
        packet.WriteByte(soulmate);
        packet.WriteByte(1);
        packet.WriteInt32(senderAccId);
        packet.WriteInt16(rcvSessId);
        packet.WriteFixedAsciiString(sender, 21, Locale.Start);
        packet.WriteAsciiString(msg, recieverClient.Locale);
        if(senderClient != null)
          senderClient.Send(packet, false);
        recieverClient.Send(packet, false);
      }
    }

    [PacketHandler(RealmServerOpCode.PartyChat)]
    public static void PartyChatRequest(IRealmClient client, RealmPacketIn packet)
    {
      packet.Position -= 22;
      string msg = packet.ReadAsciiString(client.Locale);
      if(msg.Length < 1 || RealmCommandHandler.HandleCommand(client.ActiveCharacter, msg,
           client.ActiveCharacter.Target as Character) ||
         !client.ActiveCharacter.IsInGroup)
        return;
      SendPartyChatResponse(client.ActiveCharacter, msg);
    }

    public static void SendPartyChatResponse(Character sender, string msg)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PartyChatResponse))
      {
        packet.WriteInt16(sender.SessionId);
        packet.WriteFixedAsciiString(sender.Name, 20, Locale.Start);
        packet.WriteInt16(sender.SessionId);
        packet.WriteByte(0);
        packet.WriteAsciiString(msg, sender.Client.Locale);
        sender.Group.Send(packet, false);
      }
    }

    [PacketHandler(RealmServerOpCode.GlobalChatWithItem)]
    public static void GlobalChatWithItemRequest(IRealmClient client, RealmPacketIn packet)
    {
      packet.Position -= 24;
      if(!client.ActiveCharacter.Asda2Inventory.UseGlobalChatItem())
        return;
      ++packet.Position;
      string str = packet.ReadAsciiString(client.Locale);
      if(str.Length < 1 || RealmCommandHandler.HandleCommand(client.ActiveCharacter, str,
           client.ActiveCharacter.Target as Character))
        return;
      if(str.Length > 200)
        client.ActiveCharacter.YouAreFuckingCheater("Global chat message more than 200 symbols.", 80);
      else if(client.ActiveCharacter.ChatBanned)
      {
        client.ActiveCharacter.SendInfoMsg("Your chat is banned.");
      }
      else
      {
        if(Asda2EventMgr.IsGuessWordEventStarted)
          Asda2EventMgr.TryGuess(str, client.ActiveCharacter);
        Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
        SendGlobalChatWithItemResponseResponse(client.ActiveCharacter.Name, str,
          client.ActiveCharacter.ChatColor, locale);
      }
    }

    public static void SendGlobalChatRemoveItemResponse(IRealmClient client, bool success, Asda2Item globalChatItem)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GlobalChatRemoveItem))
      {
        packet.WriteByte(success ? 1 : 0);
        packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
        packet.WriteInt32(globalChatItem == null ? 0 : globalChatItem.ItemId);
        packet.WriteByte(globalChatItem == null ? (byte) 0 : (byte) globalChatItem.InventoryType);
        packet.WriteInt16(globalChatItem == null ? 0 : globalChatItem.Slot);
        packet.WriteInt16(0);
        packet.WriteInt32(globalChatItem == null ? 0 : globalChatItem.Amount);
        packet.WriteByte(0);
        packet.WriteInt16(globalChatItem == null ? 0 : globalChatItem.Weight);
        packet.WriteInt16(-1);
        packet.WriteInt16(-1);
        packet.WriteInt16(-1);
        packet.WriteInt16(-1);
        packet.WriteInt16(0);
        packet.WriteInt16(0);
        packet.WriteByte(0);
        packet.WriteInt16(-1);
        packet.WriteInt16(0);
        packet.WriteInt16(-1);
        packet.WriteInt16(0);
        packet.WriteInt16(-1);
        packet.WriteInt16(0);
        packet.WriteInt16(-1);
        packet.WriteInt16(-1);
        packet.WriteInt16(-1);
        packet.WriteInt16(0);
        packet.WriteByte(0);
        packet.WriteByte(0);
        client.Send(packet, false);
      }
    }

    public static void SendGlobalChatWithItemResponseResponse(string sender, string mesage, Color color,
      Locale locale)
    {
      World.Broadcast(sender, mesage, color, locale);
    }

    public static RealmPacketOut CreateGlobalChatMessage(string sender, string message, Color color, Locale locale)
    {
      RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.GlobalChatWithItemResponse);
      realmPacketOut.WriteInt32(1);
      realmPacketOut.WriteInt32(color.ARGBValue);
      realmPacketOut.WriteInt32(0);
      realmPacketOut.WriteFixedAsciiString(sender, 20, locale);
      realmPacketOut.WriteAsciiString(message, locale);
      return realmPacketOut;
    }

    public static void SendGlobalMessageResponse(string name, Asda2GlobalMessageType type, int itemId = 0,
      short upgradeValue = 0, short mobId = 0)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GlobalMessage))
      {
        packet.WriteByte((byte) type);
        packet.WriteInt32(itemId);
        packet.WriteInt16(upgradeValue);
        packet.WriteByte(0);
        packet.WriteInt32(mobId);
        packet.WriteFixedAsciiString(name, 20, Locale.Start);
        World.Broadcast(packet, true, Locale.Any);
      }
    }

    [PacketHandler(RealmServerOpCode.CreateChatRoom)]
    public static void CreateChatRoomRequest(IRealmClient client, RealmPacketIn packet)
    {
      bool isPrivate = packet.ReadByte() == 1;
      byte maxMemberCount = packet.ReadByte();
      string str1 = packet.ReadAsdaString(28, Locale.Start);
      packet.Position += 2;
      string str2 = packet.ReadAsdaString(8, Locale.Start);
      if(client.ActiveCharacter.ChatRoom != null)
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.YouAreAlreadyInChatRoom,
          client.ActiveCharacter.ChatRoom);
      else if(client.ActiveCharacter.IsAsda2BattlegroundInProgress)
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.UnableOpenOnBattle,
          client.ActiveCharacter.ChatRoom);
      else if(isPrivate && string.IsNullOrWhiteSpace(str2))
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.SetPassword,
          client.ActiveCharacter.ChatRoom);
      else if(!Asda2EncodingHelper.IsPrueEnglish(str1))
      {
        client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("Room name");
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.UnableToOpen,
          client.ActiveCharacter.ChatRoom);
      }
      else if(!Asda2EncodingHelper.IsPrueEnglish(str2))
      {
        client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("password");
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.UnableToOpen,
          client.ActiveCharacter.ChatRoom);
      }
      else
      {
        if(maxMemberCount > 20 || maxMemberCount < 2)
          maxMemberCount = 20;
        client.ActiveCharacter.ChatRoom =
          new Asda2Chatroom(client.ActiveCharacter, isPrivate, maxMemberCount, str1, str2);
        SendChatRoomCreatedResponse(client, CreateChatRoomStatus.Ok, client.ActiveCharacter.ChatRoom);
        SendChatRoomVisibleResponse(client.ActiveCharacter, ChatRoomVisibilityStatus.Visible,
          client.ActiveCharacter.ChatRoom, null);
      }
    }

    public static void SendChatRoomCreatedResponse(IRealmClient client, CreateChatRoomStatus status,
      Asda2Chatroom room)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChatRoomCreated))
      {
        packet.WriteByte((byte) status);
        packet.WriteByte(room == null ? 0 : (room.IsPrivate ? 1 : 0));
        packet.WriteByte(room == null ? 0 : room.MaxMembersCount);
        packet.WriteFixedAsciiString(room == null ? "" : room.Name, 28, Locale.Start);
        packet.WriteInt16(0);
        client.Send(packet, false);
      }
    }

    public static void SendChatRoomVisibleResponse(Character owner, ChatRoomVisibilityStatus status,
      Asda2Chatroom room, Character character = null)
    {
      if(character != null)
      {
        using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChatRoomVisible))
        {
          packet.WriteByte((byte) status);
          packet.WriteInt32(owner.AccId);
          packet.WriteByte(room == null ? 0 : (room.IsPrivate ? 1 : 0));
          packet.WriteInt16(room == null ? 0 : room.MaxMembersCount);
          packet.WriteFixedAsciiString(room == null ? "" : room.Name, 28, Locale.Start);
          packet.WriteInt16(0);
          packet.WriteByte(0);
          character.Send(packet, true);
        }
      }
      else
      {
        using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChatRoomVisible))
        {
          packet.WriteByte((byte) status);
          packet.WriteInt32(owner.AccId);
          packet.WriteByte(room == null ? 0 : (room.IsPrivate ? 1 : 0));
          packet.WriteInt16(room == null ? 0 : room.MaxMembersCount);
          packet.WriteFixedAsciiString(room == null ? "" : room.Name, 28, Locale.Start);
          packet.WriteInt16(0);
          packet.WriteByte(0);
          owner.SendPacketToArea(packet, true, true, Locale.Any, new float?());
        }
      }
    }

    [PacketHandler(RealmServerOpCode.EnterChatRoom)]
    public static void EnterChatRoomRequest(IRealmClient client, RealmPacketIn packet)
    {
      int num = packet.ReadInt16();
      uint accId = packet.ReadUInt32();
      string password = packet.ReadAsdaString(8, Locale.Start);
      Character characterByAccId = World.GetCharacterByAccId(accId);
      if(characterByAccId == null || characterByAccId.ChatRoom == null)
        SendEnterChatRoomResultResponse(client, EnterChatRoomStatus.WrongChatRoomInfo,
          null);
      else
        characterByAccId.ChatRoom.TryJoin(client.ActiveCharacter, password);
    }

    [PacketHandler(RealmServerOpCode.CloseChatRoom)]
    public static void CloseChatRoomRequest(IRealmClient client, RealmPacketIn packet)
    {
      SendChatRoomClosedResponse(client, ChatRoomClosedStatus.Ok);
      if(client.ActiveCharacter.ChatRoom == null)
        return;
      client.ActiveCharacter.ChatRoom.Leave(client.ActiveCharacter);
    }

    public static void SendEnterChatRoomResultResponse(IRealmClient client, EnterChatRoomStatus status,
      Asda2Chatroom room)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EnterChatRoomResult))
      {
        packet.WriteByte((byte) status);
        packet.WriteByte(room == null ? 0 : (room.IsPrivate ? 1 : 2));
        packet.WriteByte(room == null ? 0 : room.MaxMembersCount);
        packet.WriteByte(room == null ? 0 : room.Members.Count);
        packet.WriteFixedAsciiString(room == null ? "" : room.Name, 28, Locale.Start);
        packet.WriteByte(0);
        packet.WriteByte(99);
        Character[] characterArray = room == null ? new Character[0] : room.Members.Values.ToArray();
        for(int index = 0; index < 20; ++index)
        {
          Character character = characterArray.Length <= index ? null : characterArray[index];
          packet.WriteByte(character == null ? 0 : (room == null || character != room.Owner ? 0 : 1));
          packet.WriteInt32(character == null ? -1 : (int) character.AccId);
          packet.WriteInt16(character == null ? -1 : character.SessionId);
        }

        client.Send(packet, true);
      }
    }

    public static void SendChatRoomEventResponse(Asda2Chatroom client, ChatRoomEventType status,
      Character triggerer)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChatRoomEvent))
      {
        packet.WriteByte((byte) status);
        packet.WriteInt16(triggerer == null ? 0 : triggerer.SessionId);
        packet.WriteInt32(triggerer == null ? 0U : triggerer.AccId);
        client.Send(packet, true, Locale.Any);
      }
    }

    public static void SendChatRoomClosedResponse(IRealmClient client, ChatRoomClosedStatus status)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChatRoomClosed))
      {
        packet.WriteByte((byte) status);
        client.Send(packet, false);
      }
    }

    [PacketHandler(RealmServerOpCode.SendRoomChatMessage)]
    public static void SendRoomChatMessageRequest(IRealmClient client, RealmPacketIn packet)
    {
      int color = packet.ReadInt32();
      packet.Position += 4;
      string str = packet.ReadAsciiString(client.Locale);
      if(client.ActiveCharacter.ChatRoom == null)
      {
        client.ActiveCharacter.SendInfoMsg("You are not in chat room.");
      }
      else
      {
        Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
        SendRoomChatMsgResponse(client, client.ActiveCharacter.AccId, color, str, locale);
      }
    }

    public static void SendRoomChatMsgResponse(IRealmClient client, uint senderAccId, int color, string msg,
      Locale locale)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RoomChatMsg))
      {
        packet.WriteInt32(senderAccId);
        packet.WriteInt32(color);
        packet.WriteInt32(0);
        packet.WriteAsciiString(msg, locale);
        client.ActiveCharacter.ChatRoom.Send(packet, true, locale);
      }
    }

    [PacketHandler(RealmServerOpCode.DissmissPlayerFromChatRoom)]
    public static void DissmissPlayerFromChatRoomRequest(IRealmClient client, RealmPacketIn packet)
    {
      uint targetAccId = packet.ReadUInt32();
      if(client.ActiveCharacter.ChatRoom == null)
        client.ActiveCharacter.SendInfoMsg("You are not in chat room.");
      else
        client.ActiveCharacter.ChatRoom.Dissmiss(client.ActiveCharacter, targetAccId);
    }

    public static void SendDissmisedFromCharRoomResultResponse(IRealmClient client,
      DissmissCharacterFromChatRoomResult status)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DissmisedFromCharRoomResult))
      {
        packet.WriteByte((byte) status);
        client.Send(packet, true);
      }
    }

    /// <summary>Delegate used for parsing an incoming chat message.</summary>
    /// <param name="type">the type of chat message indicated by the client</param>
    /// <param name="language">the chat language indicated by the client</param>
    /// <param name="packet">the actual chat message packet</param>
    public delegate void ChatParserDelegate(Character sender, ChatMsgType type, ChatLanguage language,
      RealmPacketIn packet);

    /// <summary>
    /// Delegate used for passing chat notification information.
    /// </summary>
    /// <param name="chatter">the person hatting</param>
    /// <param name="message">the chat message</param>
    /// <param name="lang">the language of the message</param>
    /// <param name="chatType">the type of message</param>
    /// <param name="target">the target of the message (channel, whisper, etc)</param>
    public delegate void ChatNotifyDelegate(IChatter chatter, string message, ChatLanguage lang,
      ChatMsgType chatType, IGenericChatTarget target);

    public enum Asda2GlobalMessageType
    {
      HasObinedItem = 0,
      HasUpgradeItem = 1,
      HasUpgradeFail = 2,
      HasDefeated = 3,
      HasSucPetSysntes = 7
    }
  }
}