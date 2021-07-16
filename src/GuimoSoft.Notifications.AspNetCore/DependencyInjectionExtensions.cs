using GuimoSoft.Notifications.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace GuimoSoft.Notifications.AspNetCore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNotificationContext<TErrorCode>(this IServiceCollection services, params Assembly[] assembliesToFindValidators)
            where TErrorCode : struct, Enum
        {
            if (assembliesToFindValidators is not null)
                AbstractValidatorCache<TErrorCode>.FindValidators(assembliesToFindValidators);
            return services.AddScoped<INotificationContext<TErrorCode>, NotificationContext<TErrorCode>>();
        }

        public static MvcOptions AddNotificationFilter<TErrorCode>(this MvcOptions options)
            where TErrorCode : struct, Enum
        {
            options.Filters.Add<NotificationActionFilter<TErrorCode>>();

            return options;
        }
    }
}
