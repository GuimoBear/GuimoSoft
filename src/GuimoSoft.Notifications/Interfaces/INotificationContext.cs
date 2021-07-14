using GuimoSoft.Core;
using System;
using System.Collections.Generic;

namespace GuimoSoft.Notifications.Interfaces
{
    public interface INotificationContext<TErrorCode>
        where TErrorCode : struct, Enum
    {
        bool HasNotifications { get; }

        TErrorCode ErrorCode { get; }
        IEnumerable<Notification> Notifications { get; }

        void AddNotification(string field, string message, object value = null);
        void AddNotification(Notification notification);
        void AddNotifications(IEnumerable<Notification> notifications);
        void AssociateErrorCode(TErrorCode errorCode);
        NotificationResult<TErrorCode> GetResult();
    }
}
