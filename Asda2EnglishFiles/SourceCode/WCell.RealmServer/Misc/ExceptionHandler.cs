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
            get { return DateTime.Now - ExceptionHandler.lastExceptionTime; }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth)]
        public static void Init()
        {
            LogUtil.ExceptionRaised += new Action<string, Exception>(ExceptionHandler.OnException);
        }

        private static void OnException(string msg, Exception ex)
        {
            if (ex != null)
            {
                ExceptionHandler.Exceptions.Add(new ExceptionInfo(msg, ex));
                double totalMinutes = ExceptionHandler.TimeSinceLastException.TotalMinutes;
                if (totalMinutes > 60.0)
                {
                    ExceptionHandler.excepRaisingSpeed = 1.0;
                    ExceptionHandler.recentExceptions = 0;
                }
                else
                {
                    ++ExceptionHandler.recentExceptions;
                    ExceptionHandler.excepRaisingSpeed =
                        (3.0 * ExceptionHandler.excepRaisingSpeed + 1.0 / totalMinutes) / 4.0;
                }

                if (ExceptionHandler.recentExceptions > 5 && ExceptionHandler.excepRaisingSpeed > 50.0 &&
                    !ServerApp<WCell.RealmServer.RealmServer>.IsShuttingDown)
                    return;
                ExceptionHandler.lastExceptionTime = DateTime.Now;
            }

            ExceptionHandler.NotifyException(msg, ex);
        }

        private static void NotifyException(string msg, Exception ex)
        {
            foreach (Character allCharacter in World.GetAllCharacters())
            {
                if (allCharacter.Role.Status == RoleStatus.Admin)
                {
                    if (ex != null)
                        allCharacter.SendSystemMessage(ChatUtility.Colorize("Exception raised: ", Color.Red));
                    allCharacter.SendSystemMessage(ChatUtility.Colorize(msg, Color.Red));
                    if (ex != null)
                        allCharacter.SendSystemMessage(ChatUtility.Colorize(ex.Message, Color.Red));
                }
            }
        }
    }
}