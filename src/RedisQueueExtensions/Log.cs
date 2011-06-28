using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurock.SmartInspect;

namespace RedisQueueExtensions
{
        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error
        }

        public class Log
        {
            public static LogLevel Level = LogLevel.Info;

            public static Action<LogLevel, string, Exception, object[]> LogAction = (level, message, ex, args) =>
            {
                if (level >= Level)
                    Console.WriteLine("{0} [{1}] {2} {3}", DateTime.Now, level, message, ex);

                // Add  SmartInspect Output
                if (ex != null)
                    SiAuto.Main.LogException(message, ex);
                else
                    if (args != null)
                        SiAuto.Main.LogMessage(message, args);
                    else SiAuto.Main.LogMessage(message);
            };

            public static void Warn(string message, Exception ex = null)
            {
                LogAction(LogLevel.Warn, message, ex, null);
            }

            public static void Error(string message, Exception ex = null)
            {
                LogAction(LogLevel.Error, message, ex, null);
            }

            public static void Debug(string message, Exception ex = null)
            {
                LogAction(LogLevel.Debug, message, ex, null);
            }

            public static void Info(string message, Exception ex = null)
            {
                LogAction(LogLevel.Info, message, ex, null);
            }

            public static void Debug(string msg, params string[] args)
            {
                LogAction(LogLevel.Debug, msg, null, args);
            }
        }
}
