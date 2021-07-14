using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GuimoSoft.Logger.AspNetCore
{
    internal sealed class ApiLoggerAccessorInitializerMiddleware : IMiddleware
    {
        private readonly IApiLoggerContextAccessor contextAccessor;

        public ApiLoggerAccessorInitializerMiddleware(IApiLoggerContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            contextAccessor.Context = new ConcurrentDictionary<string, object>();
            await next(context);
            contextAccessor.Context = null;
        }
    }
}
