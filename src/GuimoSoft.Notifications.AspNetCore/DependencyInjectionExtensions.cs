using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using GuimoSoft.Notifications.Interfaces;

namespace GuimoSoft.Notifications.AspNetCore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNotificationContext<TErrorCode>(this IServiceCollection services, params Assembly[] assembliesToFindValidators)
            where TErrorCode : Enum
        {
            if (assembliesToFindValidators is not null)
                AbstractValidatorCache<TErrorCode>.FindValidators(assembliesToFindValidators);
            return services.AddScoped<INotificationContext<TErrorCode>, NotificationContext<TErrorCode>>();
        }

        public static MvcOptions AddNotificationFilter<TErrorCode>(this MvcOptions options)
            where TErrorCode : Enum
        {
            options.Filters.Add<NotificationActionFilter<TErrorCode>>();

            return options;
        }
    }
}
