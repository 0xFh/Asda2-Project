using Cell.Core;
using NLog;
using System;
using System.Reflection;
using WCell.Core.Localization;
using WCell.Util;

namespace WCell.Core.Network
{
  /// <summary>Manages packet handlers and the execution of them.</summary>
  public abstract class PacketManager<C, P, A> where C : IClient where P : PacketIn where A : PacketHandlerAttribute
  {
    public static Action<C, P> DefaultUnhandledPacketHandler = (client, packet) =>
      client.Server.Warning(client, WCell_Core.UnhandledPacket, (object) packet.PacketId,
        (object) packet.PacketId.RawId, (object) packet.Length);

    private readonly Logger s_log = LogManager.GetCurrentClassLogger();
    protected PacketHandler<C, P>[] m_handlers;

    public event Action<C, P> UnhandledPacket;

    protected PacketManager()
    {
      m_handlers = new PacketHandler<C, P>[MaxHandlers];
      UnhandledPacket += DefaultUnhandledPacketHandler;
    }

    public abstract uint MaxHandlers { get; }

    public PacketHandler<C, P>[] Handlers
    {
      get { return m_handlers; }
    }

    public PacketHandler<C, P> this[PacketId packetId]
    {
      get { return m_handlers[packetId.RawId]; }
      set { m_handlers[packetId.RawId] = value; }
    }

    /// <summary>
    /// Registers a packet handler delegate for a specific packet.
    /// </summary>
    /// <param name="packetId">the PacketID of the packet to register the handler for</param>
    /// <param name="fn">the handler delegate to register for the specified packet type</param>
    public void Register(PacketId packetId, Action<C, P> fn, bool isGamePacket, bool requiresLogin)
    {
      if(m_handlers[packetId.RawId] != null)
        s_log.Debug(string.Format(WCell_Core.HandlerAlreadyRegistered, packetId,
          m_handlers[packetId.RawId].Handler, fn));
      if(fn != null)
        m_handlers[packetId.RawId] = new PacketHandler<C, P>(fn, isGamePacket, requiresLogin);
      else
        m_handlers[packetId.RawId] = null;
    }

    public void Unregister(PacketId type)
    {
      m_handlers[type.RawId] = null;
    }

    /// <summary>Handles a packet that has no handler.</summary>
    /// <param name="client">the client the packet is from</param>
    /// <param name="packet">the unhandled packet</param>
    public virtual void HandleUnhandledPacket(C client, P packet)
    {
      Action<C, P> unhandledPacket = UnhandledPacket;
      if(unhandledPacket == null)
        return;
      unhandledPacket(client, packet);
    }

    /// <summary>Attempts to handle an incoming packet.</summary>
    /// <param name="client">the client the packet is from</param>
    /// <param name="packet">the packet to be handled</param>
    /// <returns>true if the packet handler executed successfully; false otherwise</returns>
    public abstract bool HandlePacket(C client, P packet);

    /// <summary>
    /// Registers all packet handlers defined in the given type.
    /// </summary>
    /// <param name="type">the type to search through for packet handlers</param>
    public void Register(Type type)
    {
      foreach(MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
      {
        A[] customAttributes = method.GetCustomAttributes<A>();
        if(customAttributes.Length != 0)
        {
          try
          {
            Action<C, P> fn = (Action<C, P>) Delegate.CreateDelegate(typeof(Action<C, P>), method);
            foreach(A a in customAttributes)
              Register(a.Id, fn, a.IsGamePacket, a.RequiresLogin);
          }
          catch(Exception ex)
          {
            throw new Exception("Unable to register PacketHandler " + (type.FullName + "." + method.Name) +
                                ".\n Make sure its arguments are: " + typeof(C).FullName + ", " +
                                typeof(P).FullName + ".\n" + ex.Message);
          }
        }
      }
    }

    /// <summary>
    /// Automatically detects and registers all PacketHandlers within the given Assembly
    /// </summary>
    public void RegisterAll(Assembly asm)
    {
      foreach(Type type in asm.GetTypes())
        Register(type);
    }
  }
}