using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GuimoSoft
{
    public class NotificationList<TErrorCode>
        where TErrorCode : Enum
    {
        [JsonPropertyName("code")]
        public TErrorCode ErrorCode { get; }
        [JsonPropertyName("message")]
        public string Message { get => GetMessage(ErrorCode); }
        [JsonPropertyName("fields")]
        public List<Notification> Notifications { get; private set; } = new List<Notification>();

        public NotificationList(TErrorCode errorCode, Notification notification)
        {
            ErrorCode = errorCode;
            Notifications.Add(notification);
        }

        public NotificationList(TErrorCode errorCode, List<Notification> notifications)
        {
            ErrorCode = errorCode;
            Notifications = notifications ?? Notifications;
        }

        public NotificationList(TErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }
        private static string GetMessage(TErrorCode errorCode)
        {
            var enumType = typeof(TErrorCode);
            var memberInfos = enumType.GetMember(errorCode.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var attribute =
                  enumValueMemberInfo.GetCustomAttribute<DescriptionAttribute>();

            if (attribute is not null)
                return attribute.Description;
            return errorCode.ToString();
        }
    }
}
