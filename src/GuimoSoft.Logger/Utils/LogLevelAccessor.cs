using Microsoft.Extensions.Logging;
using System.Threading;

namespace GuimoSoft.Logger.Utils
{
    internal static class LogLevelAccessor
    {
        private static AsyncLocal<LogLevel?> _logLevelCurrent = new AsyncLocal<LogLevel?>();

        public static LogLevel LogLevel
        {
            get
            {
                return _logLevelCurrent.Value ?? LogLevel.Trace;
            }
            set
            {
                _logLevelCurrent.Value = value;
            }
        }
    }
}
