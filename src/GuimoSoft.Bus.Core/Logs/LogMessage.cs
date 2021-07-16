using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Core.Logs
{
    public class LogMessage
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public LogMessage(string message, LogLevel level)
        {
            Message = message;
            Level = level;
        }

        public static LogMessage Trace(string message)
            => new LogMessage(message, LogLevel.Trace);

        public static LogMessage Debug(string message)
            => new LogMessage(message, LogLevel.Debug);

        public static LogMessage Information(string message)
            => new LogMessage(message, LogLevel.Information);

        public static LogMessage Warning(string message)
            => new LogMessage(message, LogLevel.Warning);

        public static LogMessage Error(string message)
            => new LogMessage(message, LogLevel.Error);

        public static LogMessage Critical(string message)
            => new LogMessage(message, LogLevel.Critical);

        public static LogMessage None(string message)
            => new LogMessage(message, LogLevel.None);
    }
}
