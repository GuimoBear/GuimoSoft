using GuimoSoft.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace GuimoSoft.Examples.Logger.Middlewares
{
    public class ApiLoggerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var accessor = context.RequestServices.GetRequiredService<IApiLoggerContextAccessor>();

            accessor.Context.Add("request-path", context.Request.Path.ToString());

            await next(context);
        }
    }
}
