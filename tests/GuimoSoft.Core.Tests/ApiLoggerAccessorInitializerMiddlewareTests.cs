using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using GuimoSoft.Core.AspNetCore;
using GuimoSoft.Logger;
using Xunit;

namespace GuimoSoft.Core.Tests
{
    public class CoreValueObjectsInitializerMiddlewareTests
    {
        [Fact]
        public async Task When_CallInvokeAsync_Then_CreateContextInstance()
        {
            CorrelationId correlationId = "test";
            Tenant tenant = "test";

            var moqAccessor = new Mock<IApiLoggerContextAccessor>();
            moqAccessor
                .Setup(x => x.Context.Add(It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable();

            var sut = new CoreValueObjectsInitializerMiddleware();

            var moqServiceProvider = new Mock<IServiceProvider>();
            moqServiceProvider
                .Setup(x => x.GetService(typeof(IApiLoggerContextAccessor)))
                .Returns(moqAccessor.Object);
            moqServiceProvider
                .Setup(x => x.GetService(typeof(CorrelationId)))
                .Returns(correlationId);
            moqServiceProvider
                .Setup(x => x.GetService(typeof(Tenant)))
                .Returns(tenant);

            var moqHttpContext = new Mock<HttpContext>();

            moqHttpContext
                .Setup(x => x.RequestServices)
                .Returns(moqServiceProvider.Object);

            await sut.InvokeAsync(moqHttpContext.Object, _ => Task.CompletedTask);

            moqServiceProvider
                .Verify(x => x.GetService(typeof(IApiLoggerContextAccessor)), Times.Once);
            moqServiceProvider
                .Verify(x => x.GetService(typeof(CorrelationId)), Times.Once);
            moqServiceProvider
                .Verify(x => x.GetService(typeof(Tenant)), Times.Once);

            moqAccessor
                .Verify(x => x.Context.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(2));

            moqAccessor
                .Verify(x => x.Context.Add(Constants.KEY_CORRELATION_ID, correlationId.ToString()), Times.Once);

            moqAccessor
                .Verify(x => x.Context.Add(Constants.KEY_TENANT, tenant.ToString()), Times.Once);
        }
    }
}
