using GuimoSoft.Logger.AspNetCore;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GuimoSoft.Logger.Tests
{
    public class ApiLoggerAccessorInitializerMiddlewareTests
    {
        [Fact]
        public void When_ConstructWithNullAccessor_Then_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ApiLoggerAccessorInitializerMiddleware(null));
        }

        [Fact]
        public async Task When_CallInvokeAsync_Then_CreateContextInstance()
        {
            var moqAccessor = new Mock<IApiLoggerContextAccessor>();
            moqAccessor
                .SetupSet(x => x.Context = It.IsAny<IDictionary<string, object>>())
                .Verifiable();

            var sut = new ApiLoggerAccessorInitializerMiddleware(moqAccessor.Object);

            await sut.InvokeAsync(new DefaultHttpContext(), _ => Task.CompletedTask);

            moqAccessor
                .VerifySet(x => x.Context = It.IsAny<IDictionary<string, object>>(), Times.Exactly(2));

            moqAccessor
                .VerifySet(x => x.Context = new ConcurrentDictionary<string, object>(), Times.Once);

            moqAccessor
                .VerifySet(x => x.Context = null, Times.Once);
        }
    }
}
