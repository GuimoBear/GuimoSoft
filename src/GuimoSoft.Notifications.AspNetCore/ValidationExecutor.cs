using FluentValidation;
using GuimoSoft.Notifications.Interfaces;
using System;

namespace GuimoSoft.Notifications.AspNetCore
{
    public abstract class ValidationExecutorBase<TErrorCode>
        where TErrorCode : struct, Enum
    {
        public abstract void Validate(object instance, INotificationContext<TErrorCode> notificationContext);
    }

    public class ValidationExecutor<TErrorCode, TModel> : ValidationExecutorBase<TErrorCode>
        where TErrorCode : struct, Enum
        where TModel : NotifiableObject
    {
        private readonly AbstractValidator<TModel> _validator;

        public ValidationExecutor(AbstractValidator<TModel> validator)
        {
            _validator = validator;
        }

        public override void Validate(object instance, INotificationContext<TErrorCode> notificationContext)
        {
            if (instance is TModel notifiableObject)
            {
                notifiableObject.Validate(notifiableObject, _validator);
                if (!notifiableObject.IsValid)
                {
                    notificationContext.AddNotifications(notifiableObject.Notifications);
                    if (notifiableObject is IObjectWithAssociatedErrorCode<TErrorCode> associatedErrorCodeObject)
                        notificationContext.AssociateErrorCode(associatedErrorCodeObject.GetInvalidErrorCode());
                }
            }
        }
    }
}
