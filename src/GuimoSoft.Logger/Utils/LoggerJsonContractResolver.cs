using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GuimoSoft.Logger.Attributes;

namespace GuimoSoft.Logger.Utils
{
    internal class LoggerJsonContractResolver : DefaultContractResolver
    {
        public static readonly LoggerJsonContractResolver Instance
            = new LoggerJsonContractResolver();

        private static readonly List<MemberInfo> emptyMemberInfoList = new List<MemberInfo>();

        private readonly ConcurrentDictionary<Key, List<MemberInfo>> membersInfoCache
            = new ConcurrentDictionary<Key, List<MemberInfo>>();

        private LoggerJsonContractResolver() : base() { }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var currentLogLevel = LogLevelAccessor.LogLevel;
            var key = new Key(objectType, currentLogLevel);
            if (!membersInfoCache.TryGetValue(key, out var members))
            {
                if (NotContainsLogLevelRestriction(objectType, currentLogLevel))
                    members = base.GetSerializableMembers(objectType)
                                  .Where(mi => NotContainsLogLevelRestriction(mi, currentLogLevel)).ToList();
                else
                    members = emptyMemberInfoList;
            }
            return members;
        }

        private static bool NotContainsLogLevelRestriction(MemberInfo memberInfo, LogLevel logLevel)
        {
            var attribute = memberInfo.GetCustomAttribute<LoggerIgnoreAttribute>();
            return attribute is null ||
                   !attribute.IgnoredLevels.Contains(logLevel);
        }

        private struct Key
        {
            private readonly int hashCode;

            public Key(Type type, LogLevel logLevel)
            {
                hashCode = HashCode.Combine(type, logLevel);
            }

            public override bool Equals(object obj)
            {
                return obj is Key key &&
                       hashCode == key.hashCode;
            }

            public override int GetHashCode()
                => hashCode;
        }
    }
}
