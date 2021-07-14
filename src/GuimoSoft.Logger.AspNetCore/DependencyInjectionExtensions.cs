using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GuimoSoft.Logger.AspNetCore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApiLogger(this IServiceCollection services)
        {
            services.TryAddSingleton<IApiLoggerContextAccessor, ApiLoggerContextAccessor>();
            services.TryAddSingleton<ApiLoggerAccessorInitializerMiddleware>();
            services.TryAddSingleton(typeof(IApiLogger<>), typeof(ApiLogger<>));
            return services;
        }

        public static IApplicationBuilder UseApiLoggerContextAccessor(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiLoggerAccessorInitializerMiddleware>();
            return app;
        }
    }
}
