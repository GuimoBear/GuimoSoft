using System;
using System.Text.Json.Serialization;

namespace GuimoSoft.Core
{
    public class NotificationResult<TEnumType>
        where TEnumType : struct, Enum
    {
        [JsonPropertyName("messages")]
        public NotificationList<TEnumType> Notifications { get; }

        public NotificationResult(NotificationList<TEnumType> notifications)
        {
            Notifications = notifications;
        }
    }
}
