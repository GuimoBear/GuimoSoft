using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using GuimoSoft.Logger;

namespace GuimoSoft.Core.AspNetCore
{
    internal class CoreValueObjectsInitializerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var LoggerContextAccessor = context.RequestServices.GetRequiredService<IApiLoggerContextAccessor>();

            LoggerContextAccessor.Context.Add(Core.Constants.KEY_CORRELATION_ID, context.RequestServices.GetRequiredService<CorrelationId>().ToString());
            LoggerContextAccessor.Context.Add(Core.Constants.KEY_TENANT, context.RequestServices.GetRequiredService<Tenant>().ToString());

            await next(context);
        }
    }
}
