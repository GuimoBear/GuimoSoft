using Microsoft.Extensions.Logging;
using System;

namespace GuimoSoft.Logger.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class LoggerIgnoreAttribute : Attribute
    {
        private static readonly LogLevel[] _allLogLevels = Enum.GetValues<LogLevel>();

        public readonly LogLevel[] IgnoredLevels = _allLogLevels;

        public LoggerIgnoreAttribute() { }

        public LoggerIgnoreAttribute(LogLevel ignoredLevel, params LogLevel[] ignoredLevels)
        {
            IgnoredLevels = new LogLevel[1 + ignoredLevels.Length];
            IgnoredLevels[0] = ignoredLevel;
            if (ignoredLevels.Length > 0)
                Array.Copy(ignoredLevels, 0, IgnoredLevels, 1, ignoredLevels.Length);
        }
    }
}
