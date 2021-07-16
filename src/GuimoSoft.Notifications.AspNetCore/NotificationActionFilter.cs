using GuimoSoft.Notifications.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuimoSoft.Notifications.AspNetCore
{
    public class NotificationActionFilter<TErrorCode> : IAsyncActionFilter
        where TErrorCode : struct, Enum
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var notificationContext = context.HttpContext.RequestServices.GetService(typeof(INotificationContext<TErrorCode>)) as INotificationContext<TErrorCode>;

            foreach (var notifiableObject in context.ActionArguments
                                                    .Where(arg => arg.Value is NotifiableObject)
                                                    .Select(arg => arg.Value))
            {
                foreach (var validationExecutor in AbstractValidatorCache<TErrorCode>.Get(notifiableObject.GetType()))
                    validationExecutor.Validate(notifiableObject, notificationContext);
            }

            if (!notificationContext.HasNotifications)
                await next();

            if (notificationContext.HasNotifications)
                context.Result = new BadRequestObjectResult(notificationContext.GetResult());
        }
    }
}
