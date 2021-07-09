using System;
using System.Text.Json.Serialization;

namespace GuimoSoft
{
    public class NotificationResult<TEnumType>
        where TEnumType : Enum
    {
        [JsonPropertyName("messages")]
        public NotificationList<TEnumType> Notifications { get; }

        public NotificationResult(NotificationList<TEnumType> notifications)
        {
            Notifications = notifications;
        }
    }
}
