using System;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Collections;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Misc
{
  public static class ExceptionHandler
  {
    /// <summary>
    /// Sends exceptions to online staff with at least the given Rank.
    /// </summary>
    public static int ExceptionNotificationRank = 1000;

    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1.0);
    public static readonly SynchronizedList<ExceptionInfo> Exceptions = new SynchronizedList<ExceptionInfo>();
    private static int recentExceptions;
    private static double excepRaisingSpeed;
    private static DateTime lastExceptionTime;

    public static TimeSpan TimeSinceLastException
    {
      get { return DateTime.Now - lastExceptionTime; }
    }

    [Initialization(InitializationPass.Tenth)]
    public static void Init()
    {
      LogUtil.ExceptionRaised += OnException;
    }

    private static void OnException(string msg, Exception ex)
    {
      if(ex != null)
      {
        Exceptions.Add(new ExceptionInfo(msg, ex));
        double totalMinutes = TimeSinceLastException.TotalMinutes;
        if(totalMinutes > 60.0)
        {
          excepRaisingSpeed = 1.0;
          recentExceptions = 0;
        }
        else
        {
          ++recentExceptions;
          excepRaisingSpeed =
            (3.0 * excepRaisingSpeed + 1.0 / totalMinutes) / 4.0;
        }

        if(recentExceptions > 5 && excepRaisingSpeed > 50.0 &&
           !ServerApp<RealmServer>.IsShuttingDown)
          return;
        lastExceptionTime = DateTime.Now;
      }

      NotifyException(msg, ex);
    }

    private static void NotifyException(string msg, Exception ex)
    {
      foreach(Character allCharacter in World.GetAllCharacters())
      {
        if(allCharacter.Role.Status == RoleStatus.Admin)
        {
          if(ex != null)
            allCharacter.SendSystemMessage(ChatUtility.Colorize("Exception raised: ", Color.Red));
          allCharacter.SendSystemMessage(ChatUtility.Colorize(msg, Color.Red));
          if(ex != null)
            allCharacter.SendSystemMessage(ChatUtility.Colorize(ex.Message, Color.Red));
        }
      }
    }
  }
}