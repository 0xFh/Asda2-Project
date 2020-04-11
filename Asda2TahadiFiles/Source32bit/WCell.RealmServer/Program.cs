using NLog;
using WCell.Core;
using WCell.RealmServer.IPC;

namespace WCell.RealmServer
{
  /// <summary>Base class for starting the realm server.</summary>
  public class Program
  {
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    /// <summary>Starts up the realm server.</summary>
    public static void Start()
    {
      ServerApp<RealmServer>.Instance.Start();
      IPCServiceHost.StartService();
    }

    public static void Main()
    {
      Start();
    }
  }
}