using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;

namespace GuimoSoft.Logger.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class LoggerIgnoreAttribute : Attribute
    {
        private static readonly LogLevel[] _allLogLevels = Enum.GetValues<LogLevel>();

        public readonly ImmutableArray<LogLevel> IgnoredLevels;

        public LoggerIgnoreAttribute() 
        {
            IgnoredLevels = ImmutableArray.Create(_allLogLevels);
        }

        public LoggerIgnoreAttribute(LogLevel ignoredLevel, params LogLevel[] ignoredLevels)
        {
            var _ignoredLevels = new LogLevel[1 + ignoredLevels.Length];
            _ignoredLevels[0] = ignoredLevel;
            if (ignoredLevels.Length > 0)
                Array.Copy(ignoredLevels, 0, _ignoredLevels, 1, ignoredLevels.Length);
            IgnoredLevels = ImmutableArray.Create(_ignoredLevels);
        }
    }
}
