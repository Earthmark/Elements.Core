using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Elements.Core
{
    public static class UniLog
    {
        /// <summary>
        /// This will flush the log after every single message. This should be turned on temporarily only for debugging
        /// purposes though, because it can have pretty significant performance impact
        /// </summary>
        public static bool FlushEveryMessage;

        public static string GenerateLogName(string versionNumber, string suffix = "") => System.Environment.MachineName + " - " + versionNumber + " - " + System.DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + suffix + ".log";

        public static Func<string> MessagePrefix;

        public static event Action<string> OnLog;
        public static event Action<string> OnWarning;
        public static event Action<string> OnError;
        public static event Action OnFlush;

        public static void Flush() => OnFlush?.Invoke();

        public static void Log(object obj, bool stackTrace = false) => Log(obj?.ToString(), stackTrace);

        public static void Log(string message, bool stackTrace = false)
        {
            if (MessagePrefix != null)
                message = MessagePrefix() + message;

            if (stackTrace)
                message = message + "\n\n" + Environment.StackTrace;

            var _log = OnLog;

            if (_log == null)
                Console.WriteLine(message);
            else
            {
                _log?.Invoke(message);

                if (FlushEveryMessage)
                    Flush();
            }
        }

        public static void Warning(string message, bool stackTrace = false)
        {
            if (MessagePrefix != null)
                message = MessagePrefix() + message;

            if (stackTrace)
                message = message + "\n\n" + Environment.StackTrace;

            var _log = OnWarning;

            if (_log == null)
                Console.WriteLine(message);
            else
            {
                _log?.Invoke(message);

                if (FlushEveryMessage)
                    Flush();
            }
        }

        public static void Error(string message, bool stackTrace = true)
        {
            if (MessagePrefix != null)
                message = MessagePrefix() + message;

            if (stackTrace)
                message = message + "\n\n" + Environment.StackTrace;

            var _log = OnError;

            if (_log == null)
                Console.WriteLine(message);
            else
            {
                _log?.Invoke(message);

                if (FlushEveryMessage)
                    Flush();
            }
        }
    }

    public class UniLogTraceListener : TraceListener
    {
        public string Prefix { get; set; }

        public override void Write(string message)
        {
            UniLog.Log(Prefix + message);
        }

        public override void WriteLine(string message)
        {
            UniLog.Log(Prefix + message);
        }         
    }
}
