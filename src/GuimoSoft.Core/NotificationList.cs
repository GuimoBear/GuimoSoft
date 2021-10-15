using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GuimoSoft.Core
{
    public class NotificationList<TErrorCode>
        where TErrorCode : struct, Enum
    {
        [JsonPropertyName("code")]
        public TErrorCode ErrorCode { get; }
        [JsonPropertyName("event")]
        public string Event { get => GeTEventOrDefault(ErrorCode); }
        [JsonPropertyName("fields")]
        public List<Notification> Notifications { get; private set; }

        public NotificationList(TErrorCode errorCode, Notification notification)
        {
            ErrorCode = GetErrorCodeOrDefault(errorCode);
            Notifications = new List<Notification> { notification };
        }

        public NotificationList(TErrorCode errorCode, List<Notification> notifications)
        {
            ErrorCode = GetErrorCodeOrDefault(errorCode);
            Notifications = notifications ?? new List<Notification>();
        }

        public NotificationList(TErrorCode errorCode)
            : this(errorCode, default(List<Notification>))
        {
        }

        private static TErrorCode GetErrorCodeOrDefault(TErrorCode errorCode)
        {
            var enumType = typeof(TErrorCode);
            var memberInfos = enumType.GetMember(errorCode.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            if (enumValueMemberInfo is not null)
                return errorCode;
            if (!_defaultValues.TryGetValue(typeof(TErrorCode), out var defaultEnumValue))
            {
                defaultEnumValue = Enum.Parse<TErrorCode>(Enum.GetNames(typeof(TErrorCode)).FirstOrDefault());
                _defaultValues.Add(typeof(TErrorCode), defaultEnumValue);
            }
            return defaultEnumValue;
        }

        private static string GeTEventOrDefault(TErrorCode errorCode)
        {
            var enumType = typeof(TErrorCode);
            var memberInfos = enumType.GetMember(errorCode.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var attribute =
                    enumValueMemberInfo.GetCustomAttribute<DescriptionAttribute>();

            if (attribute is not null)
                return attribute.Description;
            else
                return errorCode.ToString();
        }

        private static readonly IDictionary<Type, TErrorCode> _defaultValues
            = new ConcurrentDictionary<Type, TErrorCode>();
    }
}
