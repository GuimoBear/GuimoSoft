using GuimoSoft.Core.Providers.Interfaces;
using GuimoSoft.Logger.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GuimoSoft.Core.AspNetCore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCoreComponents(this IServiceCollection services)
        {
            services.AddApiLogger();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IProviderExtension>(_ => new ProviderExtension());
            services.TryAddScoped(prov => new CorrelationIdProvider(prov.GetService<IHttpContextAccessor>(), prov.GetService<IProviderExtension>()));
            services.TryAddScoped<ICorrelationIdProvider>(prov => prov.GetService<CorrelationIdProvider>());
            services.TryAddScoped<ICorrelationIdSetter>(prov => prov.GetService<CorrelationIdProvider>());
            services.TryAddScoped(prov => prov.GetService<ICorrelationIdProvider>().Get());
            services.TryAddScoped(prov => new TenantProvider(prov.GetService<IHttpContextAccessor>(), prov.GetService<IProviderExtension>()));
            services.TryAddScoped<ITenantProvider>(prov => prov.GetService<TenantProvider>());
            services.TryAddScoped<ITenantSetter>(prov => prov.GetService<TenantProvider>());
            services.TryAddScoped(prov => prov.GetService<ITenantProvider>().Obter());
            services.TryAddSingleton<CoreValueObjectsInitializerMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseCoreComponents(this IApplicationBuilder app)
        {
            app.UseApiLoggerContextAccessor();
            app.UseMiddleware<CoreValueObjectsInitializerMiddleware>();
            return app;
        }
    }
}
