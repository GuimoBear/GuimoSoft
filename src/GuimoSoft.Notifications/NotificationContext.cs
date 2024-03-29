﻿using GuimoSoft.Core;
using GuimoSoft.Notifications.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuimoSoft.Notifications
{
    public class NotificationContext<TErrorCode> : INotificationContext<TErrorCode>
        where TErrorCode : struct, Enum
    {
        private readonly List<Notification> _notifications
            = new List<Notification>();

        public bool HasNotifications => _notifications.Any();

        public TErrorCode ErrorCode { get; private set; }

        public IEnumerable<Notification> Notifications => _notifications;

        public void AddNotification(string field, string @event, object value = null)
        {
            _notifications.Add(new Notification(field, @event, value));
        }

        public void AddNotification(Notification notification)
        {
            if (notification is not null)
                _notifications.Add(notification);
        }

        public void AddNotifications(IEnumerable<Notification> notifications)
        {
            if (notifications is not null)
            {
                foreach (var notificacao in notifications.Where(not => not is not null))
                {
                    _notifications.Add(notificacao);
                }
            }
        }

        public void AssociateErrorCode(TErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public NotificationResult<TErrorCode> GetResult()
        {
            return new NotificationResult<TErrorCode>(new NotificationList<TErrorCode>(ErrorCode, _notifications));
        }
    }
}
