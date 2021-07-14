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
        [JsonPropertyName("message")]
        public string Message { get => GetMessageOrDefault(ErrorCode); }
        [JsonPropertyName("fields")]
        public List<Notification> Notifications { get; private set; } = new List<Notification>();

        public NotificationList(TErrorCode errorCode, Notification notification)
        {
            ErrorCode = GetErrorCodeOrDefault(errorCode);
            Notifications.Add(notification);
        }

        public NotificationList(TErrorCode errorCode, List<Notification> notifications)
        {
            ErrorCode = GetErrorCodeOrDefault(errorCode);
            Notifications = notifications ?? Notifications;
        }

        public NotificationList(TErrorCode errorCode)
        {
            ErrorCode = GetErrorCodeOrDefault(errorCode);
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

        private static string GetMessageOrDefault(TErrorCode errorCode)
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
